# Valoris architecture

## End-to-end loop

1. `ProceduralLevelGenerator` asks the scene's seeded `MazeGenerator` for a 24×24 base layout. The base map contains only Empty and Wall cells; Start and Goal are separate coordinates.
2. `InventoryManager` creates a deterministic random count for each configured sabotage item from the level seed. `PlacementController` lets the player place/remove Wall, Trap and Mud while respecting inventory and endpoint rules.
3. The Lock Board action copies the final cells into an immutable `LevelDefinition`. The AI never receives a Tilemap, GameObject or mutable Unity data.
4. `LevelValidator` checks schema, dimensions, row-major cell values, endpoints and a BFS route that avoids walls and lethal traps.
5. `GameFlowController` selects Easy, Normal or Hard once at the start of play. Easy runs the local heuristic; Normal and Hard use the adaptive dual-memory Q-learning policy.
6. `ValorisAgentBridge` owns one `ValorisAgent` for the campaign. Adaptive training runs on a worker task; Core is pure C# and does not call Unity APIs.
7. `TrainingResult.FinalEvaluation.Path` is replayed by `AgentPathAnimator`. The result state is displayed as success/failure. `CampaignManager`—not the agent—awards the point.
8. The next level receives a derived seed and new item loadout. Spatial memory resets at each level while contextual memory persists until a new campaign begins.


## Gameplay difficulty

- **Easy:** a local four-neighbour heuristic. It does not use Q-learning or cross-level memory.
- **Normal:** adaptive spatial/contextual Q-learning with a default 500-episode per-level budget. Contextual memory still transfers between levels.
- **Hard:** the original adaptive agent with its 3,000-episode per-level budget.

Normal and Hard share the same update rule and memory lifecycle; their default difference is training budget. These values are balance settings, not empirically calibrated win-rate guarantees. See `QUICK_START_DIFFICULTY.md`.

## Assembly and package boundaries

- `Valoris.Core` is an embedded Unity package at `Packages/com.valoris.core`. Its `Valoris.Core.asmdef` has `noEngineReferences: true`; it must never reference UnityEngine.
- `Valoris.Game` compiles the player-facing Unity code under `Assets/_Valoris/Scripts` and references `Valoris.Core` plus the Input System.
- `Valoris.Game.Editor` contains Editor-only debug tooling.
- The .NET CLI and xUnit project reference the same Core `.csproj`, preventing a separate AI implementation from drifting away from Unity's package.
- The root directory remains both Git root and Unity project root. Preserve `Packages/manifest.json` and `packages-lock.json`.

## Data contract

The authoritative contract is documented in `Docs/INTERFACE_CONTRACT.md` and `Docs/LEVEL_JSON_SCHEMA.md`. The first release is 24×24, top-left origin, row-major index `y * width + x`; cell IDs are Empty=0, Wall=1, Trap=2, Mud=3; actions are Up=0, Right=1, Down=2, Left=3. Start and Goal are separate `GridPosition` values.

## Campaign rules

A campaign has at most 15 levels. The first side to eight wins takes the campaign; otherwise the result is decided after level 15. The agent reports an evaluation; only Unity's campaign manager changes the score. The per-level board lock is released after its result is recorded so the next round can be locked independently.

## Known boundaries

- This is a tabular Q-learning demonstrator, not a general agent with guaranteed zero-shot generalization.
- Context encoding intentionally aliases distant layouts. The adaptive transfer weight reduces but does not eliminate that risk.
- The Unity base-map generator and the Core benchmark generator are separate seeded generators. Both emit the same contract; their random layout algorithms are intentionally not claimed to be identical.
- No campaign save file or between-campaign learning persistence is included in this MVP.
