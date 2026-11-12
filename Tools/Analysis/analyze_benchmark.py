#!/usr/bin/env python3
"""Analyze Valoris paired reset-vs-persistent benchmark summary CSVs.

Uses only the Python standard library. The bootstrap resamples whole run seeds
(cluster bootstrap), keeping the multiple maps within a campaign together.
"""

from __future__ import annotations

import argparse
import csv
import math
import random
import statistics
import sys
from collections import defaultdict
from pathlib import Path
from typing import Dict, Iterable, List, Sequence

REQUIRED_COLUMNS = {
    "runSeed",
    "map",
    "resetSuccess",
    "resetEvaluationSteps",
    "resetEvaluationReward",
    "resetTrainingSuccessRate",
    "resetTrainingAverageReward",
    "persistentSuccess",
    "persistentEvaluationSteps",
    "persistentEvaluationReward",
    "persistentTrainingSuccessRate",
    "persistentTrainingAverageReward",
}


def parse_bool(value: str, *, field: str, row_number: int) -> bool:
    normalized = value.strip().lower()
    if normalized in {"true", "1", "yes"}:
        return True
    if normalized in {"false", "0", "no"}:
        return False
    raise ValueError(f"row {row_number}: {field} must be true/false, got {value!r}")


def percentile(sorted_values: Sequence[float], probability: float) -> float:
    if not sorted_values:
        raise ValueError("percentile requires at least one value")
    if len(sorted_values) == 1:
        return float(sorted_values[0])
    position = (len(sorted_values) - 1) * probability
    lower = math.floor(position)
    upper = math.ceil(position)
    fraction = position - lower
    return sorted_values[lower] + (sorted_values[upper] - sorted_values[lower]) * fraction


def load_rows(path: Path) -> List[dict]:
    if not path.is_file():
        raise FileNotFoundError(f"Benchmark CSV not found: {path}")

    with path.open("r", newline="", encoding="utf-8-sig") as handle:
        reader = csv.DictReader(handle)
        columns = set(reader.fieldnames or [])
        missing = sorted(REQUIRED_COLUMNS - columns)
        if missing:
            raise ValueError("CSV is missing required columns: " + ", ".join(missing))

        rows: List[dict] = []
        for line_number, source in enumerate(reader, start=2):
            try:
                row = {
                    "run_seed": int(source["runSeed"]),
                    "map": source["map"],
                    "reset_success": parse_bool(source["resetSuccess"], field="resetSuccess", row_number=line_number),
                    "reset_steps": int(source["resetEvaluationSteps"]),
                    "reset_reward": float(source["resetEvaluationReward"]),
                    "reset_training_success": float(source["resetTrainingSuccessRate"]),
                    "reset_training_reward": float(source["resetTrainingAverageReward"]),
                    "persistent_success": parse_bool(source["persistentSuccess"], field="persistentSuccess", row_number=line_number),
                    "persistent_steps": int(source["persistentEvaluationSteps"]),
                    "persistent_reward": float(source["persistentEvaluationReward"]),
                    "persistent_training_success": float(source["persistentTrainingSuccessRate"]),
                    "persistent_training_reward": float(source["persistentTrainingAverageReward"]),
                }
            except (TypeError, ValueError) as exc:
                raise ValueError(f"row {line_number}: {exc}") from exc
            rows.append(row)

    if not rows:
        raise ValueError("Benchmark CSV contains no result rows")
    return rows


def cluster_bootstrap_ci(
    per_seed_deltas: Sequence[float],
    samples: int,
    seed: int,
) -> tuple[float, float]:
    if samples <= 0:
        raise ValueError("bootstrap samples must be positive")
    rng = random.Random(seed)
    count = len(per_seed_deltas)
    means = []
    for _ in range(samples):
        draw = [per_seed_deltas[rng.randrange(count)] for _ in range(count)]
        means.append(statistics.fmean(draw))
    means.sort()
    return percentile(means, 0.025), percentile(means, 0.975)


def analyze(rows: Sequence[dict], bootstrap_samples: int, bootstrap_seed: int) -> str:
    reset_successes = sum(row["reset_success"] for row in rows)
    persistent_successes = sum(row["persistent_success"] for row in rows)
    count = len(rows)

    persistent_only = sum(row["persistent_success"] and not row["reset_success"] for row in rows)
    reset_only = sum(row["reset_success"] and not row["persistent_success"] for row in rows)
    both_success = sum(row["persistent_success"] and row["reset_success"] for row in rows)
    both_fail = count - persistent_only - reset_only - both_success

    grouped: Dict[int, list] = defaultdict(list)
    for row in rows:
        grouped[row["run_seed"]].append(row)

    per_seed_deltas = []
    for run_seed in sorted(grouped):
        seed_rows = grouped[run_seed]
        seed_delta = statistics.fmean(
            (100.0 if row["persistent_success"] else 0.0)
            - (100.0 if row["reset_success"] else 0.0)
            for row in seed_rows
        )
        per_seed_deltas.append(seed_delta)

    overall_delta = (persistent_successes - reset_successes) * 100.0 / count
    if len(per_seed_deltas) >= 2:
        ci_low, ci_high = cluster_bootstrap_ci(per_seed_deltas, bootstrap_samples, bootstrap_seed)
        ci_text = f"[{ci_low:+.2f}, {ci_high:+.2f}] pp"
    else:
        ci_text = "not estimable (only one run seed)"

    def mean(field: str) -> float:
        return statistics.fmean(row[field] for row in rows)

    lines = [
        "ORALIS / PAIRED MEMORY BENCHMARK ANALYSIS",
        f"Paired map-seed rows: {count}",
        f"Distinct run seeds: {len(grouped)}",
        "",
        "FINAL GREEDY EVALUATION",
        f"Reset-memory successes: {reset_successes}/{count} ({reset_successes * 100.0 / count:.2f}%)",
        f"Persistent-context successes: {persistent_successes}/{count} ({persistent_successes * 100.0 / count:.2f}%)",
        f"Persistent minus reset: {overall_delta:+.2f} percentage points",
        f"Run-seed cluster-bootstrap 95% CI for mean per-seed delta: {ci_text}",
        f"Paired outcomes — both success: {both_success}; both fail: {both_fail}; persistent-only success: {persistent_only}; reset-only success: {reset_only}",
        f"Mean final-evaluation steps (reset / persistent): {mean('reset_steps'):.2f} / {mean('persistent_steps'):.2f}",
        f"Mean final-evaluation reward (reset / persistent): {mean('reset_reward'):.2f} / {mean('persistent_reward'):.2f}",
        "",
        "TRAINING EPISODES",
        f"Mean episode success rate (reset / persistent): {mean('reset_training_success'):.2f}% / {mean('persistent_training_success'):.2f}%",
        f"Mean episode reward (reset / persistent): {mean('reset_training_reward'):.2f} / {mean('persistent_training_reward'):.2f}",
        "",
        "PER-SEED FINAL-EVALUATION SUCCESS DELTAS",
    ]
    for run_seed, delta in zip(sorted(grouped), per_seed_deltas):
        lines.append(f"  seed {run_seed}: {delta:+.2f} pp")

    lines.extend([
        "",
        "Interpretation: a positive delta describes this benchmark set; it does not prove generalization to arbitrary maps.",
        "The confidence interval resamples whole run seeds so maps within a campaign are not treated as independent samples.",
        "With only a handful of run seeds, the interval is exploratory. Increase the seed count before making a strong claim.",
    ])
    return "\n".join(lines)


def main(argv: Iterable[str] | None = None) -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("summary_csv", type=Path, help="CSV produced by the CLI benchmark --csv option")
    parser.add_argument("--bootstrap-samples", type=int, default=10000)
    parser.add_argument("--bootstrap-seed", type=int, default=20261111)
    args = parser.parse_args(argv)

    try:
        rows = load_rows(args.summary_csv)
        print(analyze(rows, args.bootstrap_samples, args.bootstrap_seed))
        return 0
    except (OSError, ValueError) as exc:
        print(f"ERROR: {exc}", file=sys.stderr)
        return 1


if __name__ == "__main__":
    raise SystemExit(main())
