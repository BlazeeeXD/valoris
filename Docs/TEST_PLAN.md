# Valoris test plan

## .NET automated tests

The `Tests/Valoris.Core.Tests` project covers:

| Area | Tests |
|---|---|
| Contracts and validation | `LevelValidationTests`, `LevelValidatorTests` |
| Movement and collisions | `GridMovementTests`, `GridEnvironmentTests`, `CollisionTests` |
| Procedural generation | `ProceduralMapGeneratorTests` |
| Spatial/context encoders | `StateEncoderTests`, `ContextEncoderTests` |
| Q-table and learning | `QLearningTests`, `QLearningUpdateTests`, `QTableTests`, `TransferPolicyTests` |
| Training/evaluation | `EpisodeTests`, `EvaluationTests`, `AgentTrainingTests` |
| Campaign memory | `MemoryPersistenceTests`, `DualMemoryTests` |
| Options/reward validation | `TrainingOptionsValidationTests` |

Run from the repository root with .NET 10:

```powershell
dotnet build .\Packages\com.valoris.core\Valoris.Core.csproj
dotnet build .\Tools\Valoris.Cli\Valoris.Cli.csproj
dotnet test .\Tests\Valoris.Core.Tests\Valoris.Core.Tests.csproj
```

## Unity EditMode tests

`Assets/_Valoris/Tests/EditMode/Valoris.Game.Tests.asmdef` contains tests for:
- A campaign lock being released after a score is recorded and the following level being lockable.
- Campaign completion on first-to-eight and 15-level cap rules.
- Deterministic Unity base-map generation, safe route invariants and non-corner endpoints.
- Level snapshot cloning, row-major mapping and BFS validation.
- Placement endpoint protection, cell constraints and remove-only-player-placement behavior.
- Seeded random inventory quantities and keyboard selection by CellType, independent of Inspector array order.

Run these in **Window → General → Test Runner → EditMode** after Unity finishes compiling.

## Manual Play Mode acceptance

1. Press Play in `MainGame.unity`; check the Console for missing scripts, missing tile references or compile errors.
2. Confirm a 24×24 board appears with Start/Goal markers.
3. Press 1/2/3 and place Wall/Trap/Mud. Confirm quantities decrement and the selected item is shown in the HUD.
4. Right-click a player-placed item. Confirm it is removed and its quantity is restored; base maze walls cannot be removed.
5. Build a board that blocks every safe route. Lock must be rejected and placement must remain enabled.
6. Restore a safe route and Lock Board. Confirm input cannot mutate the locked board.
7. Confirm AI training runs, then the final evaluation path is replayed; success is green and failure red.
8. Confirm the correct side receives a point. Press N and verify the next level is editable and lockable (this catches campaign-lock lifecycle regressions).
9. Complete a campaign or use a test seed; confirm first-to-eight / level-15 completion, and R starts a fresh campaign with fresh AI memory.
10. Build a standalone Windows player and verify the same loop using the `MainGame` build scene.

## Benchmark experiment

Run at least five seeds and five generated maps for a first transfer comparison. Preserve both summary and episode CSVs. See `Docs/EXPERIMENTS.md` for the paired protocol and interpretation.
