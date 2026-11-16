# Oralis / Valoris - Quick Start and Difficulty Guide

This is the complete Unity project plus the headless AI Core, CLI, tests, data fixtures, and A's updated map-generation / Unity changes.

## Start the Unity game

1. Extract the archive to a normal writable folder.
2. Open the extracted folder in Unity Hub using the Unity version recorded in `ProjectSettings/ProjectVersion.txt`.
3. Open `Assets/_Valoris/Scenes/MainGame.unity`.
4. Press Play.
5. The difficulty selector appears over the game. Choose Easy, Normal, or Hard. The board stays locked against placement until a choice is made.
6. Place sabotage, press the existing Lock Board control, and watch the AI replay its route.
7. Use `N` for the next level. Use `R` after the campaign completes to start a new campaign. A new campaign resets the agent's memory; moving to the next level preserves contextual memory.

The difficulty selector is drawn by `GameFlowController.OnGUI()`. It does not require a new Canvas object, button wiring, or scene editing.

## When does the AI lose?

The AI receives the point only when its final evaluation terminates with `ReachedGoal`. The player receives the point when the AI enters a trap, runs out of available moves, or reaches its evaluation step limit without reaching the goal. Each step is one attempted grid move. Easy now has a separate 230-move cap by default; Normal and Hard keep the existing 300-move evaluation cap. For example, the Easy run shown in the supplied screenshot needed 258 moves, so that same route would now time out at 230 and award the point to the player.

## Merge into the existing Git working copy

Do not overwrite the only copy of your current project before checking this archive.

1. Extract the ZIP to a separate folder and open that folder in Unity Hub first.
2. In your existing repository, switch to `blaze` and check that the working tree is clean:

   ```powershell
   git switch blaze
   git status --short
   git branch backup/blaze-before-difficulty-20261116
   ```

   If `git status --short` prints changes, commit or stash them before copying.
3. Close Unity. Copy the extracted project's contents into the existing repository root. The archive contains no `.git` directory, so the existing repository metadata stays in place. Do not delete your `.git` directory.
4. Reopen the existing repository in Unity Hub and follow the build/test checklist below.
5. Only after checks pass, commit the merge on `blaze`. Do not force-push `main` for this change.

## Difficulty behaviour

| Mode | Default training budget | Agent behaviour |
|---|---:|---|
| Easy | No Q-learning; 230-step evaluation cap by default | A deliberately weak local heuristic scores only the four neighbouring cells using distance to the goal, a small within-level revisit cost (0.25), and immediate terrain. It does not search the whole map, train Q-tables, or retain knowledge between levels. The shorter path budget gives the player a better chance to win. |
| Normal | 500 episodes per level | The adaptive dual-memory Q-learning agent. Contextual memory persists across levels; spatial memory resets for each level. During final path replay only, it avoids returning to a tile visited in the last 8 moves when another legal alternative exists. |
| Hard | 3,000 episodes per level | The original full adaptive Q-learning agent and its existing greedy evaluation, unchanged. Contextual memory persists across levels. |

Normal deliberately keeps cross-level contextual learning. It uses the same Q-learning update, state encoders, legal-action mask, transfer policy, and memory lifecycle as Hard. In Normal only, the final evaluator checks whether the policy's chosen next tile is among the last 8 path positions. If so, it selects the highest-valued legal alternative outside that short loop. If every legal move requires backtracking (a genuine dead end), it falls back to the learned action. This is a replay-time guard only: it does not rewrite Q-values, alter training, or affect Hard. The 500-episode budget remains a starting balance value, not a measured win-rate guarantee.

Easy skips Q-learning entirely. Its navigator is intentionally local and can loop or choose a poor route even when a valid route exists. It is not BFS and it does not use the learned Q-tables. The revisit cost is deliberately small; increasing `RevisitPenalty` in `GreedyHeuristicEvaluator.cs` makes Easy more capable of escaping loops, while decreasing it makes Easy weaker.

To tune the difficulty settings, select the GameFlowController object in the scene and edit:

- `easyEvaluationMaxSteps` (default 230; Easy is capped at the lower of this value and the global evaluation limit)
- `normalEpisodesPerLevel` (default 500)
- `episodesPerLevel` (Hard, default 3,000)
- `evaluationMaxSteps` (Normal / Hard final evaluation limit, default 300)

The Easy path does not use `EpisodesPerLevel`; the Core receives a valid placeholder value because the shared options object validates positive episode counts.

For quick balance adjustments, change only these values first:

- `easyEvaluationMaxSteps` in `GameFlowController.cs`: lower it to make Easy easier to beat; raise it if Easy is failing too often. The current 230-step default is intentionally below the previous 300-step limit.
- `normalEpisodesPerLevel`: lower it (for example, 300) if Normal still feels too strong; raise it (for example, 750) if Normal is too weak. The contextual Q-table still persists either way.
- `RevisitPenalty` in `GreedyHeuristicEvaluator.cs`: lower it below `0.25` to make Easy more prone to loops; raise it toward `0.5` to make Easy better at backtracking.

Do not change the Q-learning equation or Hard's defaults while balancing. One variable at a time makes it possible to tell what actually changed the win rate.

## What changed in this merged version



- Retains A's updated Unity maze generation: seeded depth-first maze carving, with additional opened loops / alternative routes. The generator and regression tests are currently configured around the default corner endpoints (0,0) and (23,23).
- Retains the corresponding procedural-level generator integration, scene/UI updates, inventory UI changes, and campaign progression changes.
- Removes the Wall Breaker feature completely. The AI contract is back to the original four movement actions; no player-wall ownership flags, break actions, wall-break validation, or Wall Breaker tests remain.
- Preserves the original AI architecture: spatial Q-table, contextual Q-table, adaptive contextual weighting, and cross-level contextual memory.
- Adds the Easy / Normal / Hard selection overlay, a local Easy heuristic, and episode-budget selection for Normal / Hard.
- Keeps `Directory.Build.props`, which redirects .NET build outputs away from `Packages/com.valoris.core` so Unity does not import a duplicate Core DLL.

`Valoris` is the repository / project name. The game itself is titled `Oralis`.

## Build and test the AI Core on Windows

Run these commands from the extracted project root in PowerShell:

```powershell
dotnet build .\Packages\com.valoris.core\Valoris.Core.csproj
dotnet build .\Tools\Valoris.Cli\Valoris.Cli.csproj
dotnet test .\Tests\Valoris.Core.Tests\Valoris.Core.Tests.csproj
```

Expected baseline from the previous validated V0.1: Core build succeeds, CLI build succeeds, and 60 Core tests pass. This merged package adds difficulty-policy tests; **the new combined project has not been run in Unity or built with the Windows .NET SDK in this environment**, so treat those earlier results as the baseline, not as verification of this archive.

If Unity reports duplicate `Valoris.Core.dll` / asmdef errors from old local build artifacts, close Unity and run:

```powershell
powershell -ExecutionPolicy Bypass -File .\Repair-UnityCompilation.ps1
```

Then reopen Unity. New .NET builds are redirected to the root `.dotnet` folder by `Directory.Build.props`.

## Fast verification checklist

- [ ] Unity opens `MainGame` without compile errors.
- [ ] Difficulty overlay appears before player placement is enabled.
- [ ] Easy stops at its configured 230-step cap; a route taking longer than the cap awards the point to the player.
- [ ] Normal avoids short back-and-forth loops during final replay and still retains contextual memory across levels.
- [ ] Hard behaves like the original greedy agent, with no loop-avoidance override.
- [ ] Change levels with `N`; verify campaign contextual memory count persists across levels.
- [ ] Start a new campaign with `R`; verify memory resets.
- [ ] Try a board with only one legal move at a step; even Easy must take that legal move.
- [ ] Run the Core build, CLI build, and full xUnit suite.

## Important interpretation

Normal and Hard use the same learned agent, but different per-level training budgets and evaluation behaviour: Normal has the short-loop replay guard; Hard remains the original greedy evaluator. Easy uses a separate local heuristic with a shorter evaluation limit. Keep the limits and episode budget as tunable balance parameters; validate actual campaign win rates before calling the modes balanced.
