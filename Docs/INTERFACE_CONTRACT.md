# Unity / AI interface contract

## Ownership

Unity owns the mutable player board, placement rules, UI, campaign score and animation. `Valoris.Core` owns authoritative DTOs/enums, level validation, movement semantics, Q-learning, training, evaluation and memory. The AI receives a copied `LevelDefinition`, never a Unity object.

## Values

```csharp
public enum CellType : byte { Empty = 0, Wall = 1, Trap = 2, Mud = 3 }
public enum MoveAction : byte { Up = 0, Right = 1, Down = 2, Left = 3 }
```

- `GridPosition` uses integer X/Y with top-left origin.
- Row-major array index: `y * width + x`.
- Board is 24×24 in this release; `cells.Length == 576`.
- Start and Goal are separate coordinates and must be distinct, in bounds, and Empty.

## LevelDefinition

`LevelSnapshotFactory.Create(GeneratedLevelData)` copies Unity's current cells to `LevelDefinition`. The Core DTO clones the supplied array and exposes only indexed reads or copies. A may continue to display the mutable board, but changes after snapshot creation do not alter the data being trained on.

Required fields: schema version, positive level number, seed, width, height, row-major numeric cells, Start and Goal.

## Validation

`ILevelValidator.Validate(level)` returns `LevelValidationResult` (`IsValid`, `HasSafePath`, `Messages`). It checks supported schema, 24×24 dimensions, cell IDs, exact array length, endpoints and BFS reachability without Wall/Trap cells. Mud is traversable.

## Agent lifecycle

```csharp
IOralisAgent agent = new ValorisAgent();
agent.BeginCampaign(campaignSeed); // once per campaign
TrainingResult result = agent.TrainLevel(lockedLevel, options); // once per level
AgentMemorySummary summary = agent.GetMemorySummary();
```

`TrainingResult` contains episode metrics, final greedy evaluation and memory summary. `EvaluationResult.Path` includes the starting coordinate and every subsequent position. `ReachedGoal` and `TerminationReason` decide the visual outcome; the path alone does not imply success.

## Memory

- Both tables update independently on each training transition.
- Spatial table and per-action visit counts reset at the start of every level.
- Context table is retained between levels of one campaign.
- `BeginCampaign` clears both tables, visit counts and counters.
- Context blending is action-specific and decays as spatial state-action visits accumulate.

## Threading

Core is Unity-independent and may run on a worker task. Unity object access, UI updates and path playback remain on the main thread. The game passes the immutable snapshot to `Task.Run` and resumes presentation after the awaited result.
