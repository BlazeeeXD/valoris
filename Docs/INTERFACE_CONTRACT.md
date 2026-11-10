# Valoris Core Interface Contract

## Board and coordinates

- Campaign board: 24 x 24.
- Origin: top-left; X increases right; Y increases down.
- Cell array order: row-major, `index = y * width + x`.
- Cell IDs: `Empty=0`, `Wall=1`, `Trap=2`, `Mud=3`.
- Start and Goal are separate `GridPosition` values; they are not `CellType` values.
- Actions: `Up=0`, `Right=1`, `Down=2`, `Left=3`. No diagonal movement.

## Environment rules

- Wall and out-of-bounds: no movement; wall penalty applies if a caller attempts the move.
- Trap: entering ends the episode as failure.
- Mud: traversable with an additional penalty.
- Goal: entering ends the episode as success.
- Validator: BFS confirms at least one route avoiding walls and lethal traps.
- A locked level must not be mutated after it is handed to the Core package.
- Core must not reference Unity presentation types or `UnityEngine`.

## Shared API

- `LevelDefinition`: immutable snapshot of the final locked board.
- `ILevelValidator.Validate(level)`: checks the board and reports whether a safe route exists.
- `IOralisAgent.BeginCampaign(seed)`: resets campaign memory; call once per campaign.
- `IOralisAgent.TrainLevel(level, options)`: trains on the locked level, then returns a final evaluation.
- `TrainingResult`: episode metrics, final evaluation and memory summary.
- `EvaluationResult.Path`: ordered `GridPosition` values including the start and each resulting position; Unity may animate this path.

## Learning semantics

- State includes absolute grid position plus local terrain and goal context.
- The agent selects only actions whose neighbouring cell is not a wall; traps remain selectable and carry terminal penalties.
- Q-learning bootstraps only over legal actions in the next observation.
- Evaluation disables epsilon exploration; equal-valued legal actions use seeded random tie-breaking.
- Q-table memory persists between levels in one campaign and resets at `BeginCampaign`.
- This is approximate transfer across procedural levels, not a claim of perfect generalization.

## Package boundaries

The Core package is embedded at `Packages/com.valoris.core`. The CLI and test projects reference its `Valoris.Core.csproj`; Unity compiles the same runtime sources through `Valoris.Core.asmdef`.
