# Level JSON schema — version 1

The CLI and Unity adapter exchange the same `LevelDefinition` snapshot. The JSON uses numeric cell IDs and a flat row-major cell array.

## Required properties

| Property | Type | Rule |
|---|---|---|
| `schemaVersion` | integer | Must equal `1` for this release. |
| `levelNumber` | integer | Positive campaign level number. |
| `seed` | integer | Seed used to generate the map; retained for reproduction. |
| `width` | integer | `24` for the first release. |
| `height` | integer | `24` for the first release. |
| `cells` | integer array | Exactly `width * height` values, stored row-major. |
| `start` | `{ "x": integer, "y": integer }` | Inside the board and on an Empty cell. |
| `goal` | `{ "x": integer, "y": integer }` | Inside the board, different from start, and on an Empty cell. |

## Cell and action IDs

| Cell | ID | Meaning |
|---|---:|---|
| Empty | 0 | Traversable normal tile. |
| Wall | 1 | Impassable. |
| Trap | 2 | Entry terminates the attempt as failure. |
| Mud | 3 | Traversable with an additional penalty. |

Start and Goal are separate coordinates and are not cell IDs. Actions are `Up=0`, `Right=1`, `Down=2`, and `Left=3`; diagonal movement is not supported.

## Example shape

```json
{
  "schemaVersion": 1,
  "levelNumber": 1,
  "seed": 42,
  "width": 24,
  "height": 24,
  "cells": [
    1, 1, 1, 1,
    1, 0, 0, 1
  ],
  "start": { "x": 1, "y": 1 },
  "goal": { "x": 2, "y": 1 }
}
```

The short `cells` array above only illustrates the shape; a valid 24 × 24 level must contain exactly 576 entries.

## Validation

Core rejects unsupported schema versions, dimensions other than 24 × 24, invalid cell IDs, incorrect cell counts, out-of-bounds or overlapping endpoints, endpoints placed on non-empty cells, and any board without a safe BFS route from start to goal. A safe route may cross Mud but never a Wall or Trap.

`LevelDefinition` copies the supplied cell array at construction and returns copies from `CopyCells()`. Callers must still treat the locked snapshot as immutable after handing it to the agent.
