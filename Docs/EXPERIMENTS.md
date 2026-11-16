# Experiments and benchmark protocol

## B1 hard-map regression

The prior development log records a paired ablation on `hard_01.json` with 3,000 episodes, run seed 7 and epsilon decay 0.9995:

| Configuration | Rolling training success (last window) | Final greedy evaluation |
|---|---:|---|
| Fixed contextual blend 0.30 | 91% | Failure at 300 steps |
| Spatial-only influence 0.00 | 78% | Success at 72 steps |
| Adaptive contextual influence (wMax 0.30, decay visits 20) | 85% | Success at 76 steps |

These figures are historical output supplied in the development conversation, not a fresh execution of the exact archive. The spatial-only 72-step path was recorded as the BFS shortest route in that fixture. The adaptive result is four steps longer; it shows that the hard-map regression was avoided, not that transfer is statistically beneficial.

## B2 paired benchmark

`benchmark` compares two arms on each same map and run seed:

1. **Reset arm:** new agent and fresh contextual/spatial memory for every map.
2. **Persistent arm:** one agent per run seed; spatial memory resets inside `TrainLevel`, contextual memory persists across level transitions.

Both arms receive the same level, episode budget, hyperparameters and `RandomSeed`. Fixture maps are ordered by level number then filename; generated maps are produced deterministically from a run seed. The report includes final greedy success, steps, reward, termination reason, rolling training success/reward and contextual state growth.

Example:

```powershell
dotnet run --project .\Tools\Valoris.Cli -- benchmark --generated-maps 5 --difficulty hard --episodes 1000 --seeds 7,11,19 --csv .\Data\Results\transfer_summary.csv --episodes-csv .\Data\Results\transfer_episodes.csv
```

Interpretation rules:

- A positive persistent-minus-reset success-rate delta is evidence only for the tested map/seed set.
- Compare paired rows; do not compare unrelated single runs.
- Report the number of maps and seeds, plus training and final-evaluation metrics.
- Increase seed/map coverage before making a broad transfer claim.
- Do not tune on the test map and then report the same map as an independent validation set.

The Python helper `Tools/Analysis/analyze_benchmark.py` reads the exported CSV to summarize paired outcomes.
