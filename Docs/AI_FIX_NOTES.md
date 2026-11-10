# Valoris AI Fix — source audit and changes

## Reproduced issue

The first baseline learned the open map but failed on `hard_01.json` at both 5,000 and 10,000 episodes. The hard fixture is valid and has a safe route, so the failure was not caused by an impossible map.

## Changes in this patch

1. **Position-aware state encoding:** The state key now contains `(x, y)` plus the existing local terrain and goal context. This prevents different intersections in the same map from being merged into one state.
2. **Legal-action masking:** The agent no longer chooses known wall/out-of-bounds actions during exploration or evaluation.
3. **Masked Bellman target:** The next-state maximum is computed only across legal actions.
4. **Unbiased greedy ties:** Equal-valued legal actions are selected with the seeded RNG; an unvisited state no longer defaults to Up.
5. **Slower epsilon decay:** Default per-episode decay changes from 0.995 to 0.9995.
6. **EpisodeRunner extraction:** One-episode logic is now isolated and testable.
7. **Training diagnostics:** CLI output includes recent-window success rate and average reward.
8. **CLI tuning switches:** `--alpha`, `--gamma`, `--epsilon-decay`, `--min-epsilon` and `--epsilon` are configurable for training. Benchmark supports alpha, gamma, epsilon decay and minimum epsilon.
9. **Regression tests:** Tests cover positional state distinction, legal-action selection, masked Q maxima, one-step evaluation, episode limits and the supplied hard map.

## Important limitation

This is a targeted tabular Q-learning fix, not a claim that the agent now generalizes optimally to every random map. Position plus local context makes the current map much more learnable; persistent transfer across different maps remains an experimental question. Compare against a reset-memory agent over multiple seeds before claiming a memory advantage.

## Verify on Windows

From the repository root:

```cmd
dotnet build Packages\com.valoris.core\Valoris.Core.csproj
dotnet build Tools\Valoris.Cli\Valoris.Cli.csproj
dotnet test Tests\Valoris.Core.Tests\Valoris.Core.Tests.csproj
```

Then run the hard-map regression manually:

```cmd
dotnet run --project Tools\Valoris.Cli -- train --map Data\Maps\Fixtures\hard_01.json --episodes 3000 --seed 7 --epsilon-decay 0.9995
```

If the test fails on the Windows runtime, preserve the complete output and seed. Do not immediately increase the episode count; compare the C# result with the expected state-count growth, legal-action behavior and episode success rate first.
