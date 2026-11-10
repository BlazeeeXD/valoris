# Valoris Architecture

## Repository layout

- `Packages/com.valoris.core`: headless simulation, contracts, tabular Q-learning and training/evaluation. This is the source of truth shared by the CLI and Unity.
- `Tools/Valoris.Cli`: terminal front end for loading JSON map fixtures, inspecting maps, training, evaluating and exporting metrics.
- `Tests/Valoris.Core.Tests`: automated tests for simulation rules, learning, training and memory lifecycle.
- `UnityProject` is not a nested folder in this repository. The Unity project root is the repository root; Unity's `Assets`, `Packages` and `ProjectSettings` directories remain at the root.
- `Data/Maps/Fixtures`: deterministic hand-authored maps for regression tests and repeatable experiments.

## Core boundaries

The Core package must not reference `UnityEngine`. It receives immutable `LevelDefinition` data and operates entirely on ordinary C# types. It returns `TrainingResult` containing episode metrics and an evaluation path. Unity owns rendering, input, animation, inventory, campaign UI and presentation playback.

## Runtime flow

1. Unity generates a base map and applies the player's selected sabotage items.
2. Unity locks the board and creates an immutable `LevelDefinition` snapshot.
3. The Core validator checks dimensions, cell values, endpoints and existence of a safe BFS route.
4. One `ValorisAgent` instance is created per campaign; `BeginCampaign(seed)` is called once.
5. For every locked level, `TrainLevel(level, options)` runs headless episodes against a deterministic `GridEnvironment`.
6. The Q-table is retained in the agent instance across levels in the same campaign.
7. A final greedy evaluation runs with exploration disabled and returns a path.
8. Unity animates the path and awards the level point. The final evaluation determines the winner, not intermediate training episodes.

## Learning design

The current baseline is tabular Q-learning. Its state key includes absolute position on the 24 x 24 map, adjacent terrain types and coarse goal-relative information. This fixes within-map aliasing between different coordinates that share similar local observations. The key space is sparse; only visited states allocate Q-values.

Action selection is epsilon-greedy over legal actions. Wall and out-of-bounds actions are masked during exploration, greedy action selection and the Bellman bootstrap. Trap cells remain available actions because they are meaningful terminal hazards. Greedy ties are resolved using the run's seeded random generator rather than a fixed first-action bias.

Campaign persistence reuses the same Q-table across levels. Since position and local terrain are both in the key, only sufficiently similar state contexts reuse values. This is conservative transfer, not a guarantee of generalization to arbitrary unseen layouts. The persistent-versus-reset benchmark must measure whether this transfer helps or harms.

## Determinism and experiments

Every map carries a level seed and level number. The agent combines campaign seed, options seed, map seed and level number to initialize the run RNG. Record map fixtures, training options and seeds alongside experiment output. Compare reset and persistent agents on the same map order and settings over multiple seeds.

## Unity integration rule

The integration boundary is data-only:

`LevelDefinition -> IOralisAgent.TrainLevel -> TrainingResult.FinalEvaluation.Path`

The Core package never receives a Tilemap, GameObject, Transform, SpriteRenderer, Animator or Rigidbody2D. Unity consumes the returned grid positions and animates them independently.
