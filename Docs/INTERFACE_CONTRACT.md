# Valoris Core interface contract

- Board: 24 x 24 for campaign release.
- Origin: top-left; X right; Y down.
- Array order: row-major, `index = y * width + x`.
- CellType IDs: Empty=0, Wall=1, Trap=2, Mud=3. Start and Goal are separate GridPosition values.
- Actions: Up=0, Right=1, Down=2, Left=3. No diagonal movement.
- Wall/out-of-bounds attempt: no movement; wall penalty.
- Trap: entering ends the episode as failure.
- Mud: traversable with additional penalty.
- Goal: entering ends the episode as success.
- Validator: BFS confirms at least one route avoiding walls and lethal traps.
- A locked level must not be mutated after being handed to the Core package.
- Unity presentation must not be referenced by Core.

The Core package is embedded at `Packages/com.valoris.core`. The CLI and test projects reference its `Valoris.Core.csproj`; Unity references the package assembly definition.
