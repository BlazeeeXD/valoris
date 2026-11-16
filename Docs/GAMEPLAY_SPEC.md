# Gameplay specification

## Goal

The player tries to prevent the AI from reaching the goal. The AI trains on the final, player-modified map and then makes one deterministic greedy evaluation attempt. AI success gives the AI a point; any final-evaluation failure gives the player a point.

## Board and coordinates

- Fixed board: 24 columns × 24 rows.
- Unity top-left origin is `(0,0)`; X increases to the right and Y increases downward.
- Core cell array is row-major: `index = y * width + x`.
- The first scene uses Start `(0,0)` and Goal `(23,23)`.
- No diagonal movement.

## Player controls

| Input | Action |
|---|---|
| `1` | Select Wall |
| `2` | Select Trap |
| `3` | Select Mud |
| Left mouse | Place selected sabotage on an empty non-endpoint tile |
| Right mouse | Remove a player-placed sabotage item and restore its inventory unit |
| Lock Board button | Validate, freeze board and begin training/evaluation |
| `N` | Start next campaign level after the result replay |
| `R` | Start a new campaign after the current campaign finishes |

The loadout is seeded from the level seed. Each non-null item definition contributes a random quantity from one up to its configured `startingQuantity`; this makes a repeat of the same seed reproducible.

## Sabotage semantics

- Wall: cannot be entered; attempted movement remains in place and incurs the wall penalty.
- Trap: can be entered, but immediately terminates the evaluation as failure.
- Mud: traversable and applies an additional negative reward.
- Start/Goal cannot be overwritten.
- A board is rejected unless BFS finds at least one safe route from Start to Goal that avoids Walls and Traps. Mud is allowed on a safe route.

## Campaign

- Default maximum: 15 levels.
- First to 8 wins takes the campaign.
- The final greedy evaluation, not the best training episode, decides each point.
- A new level resets spatial Q-values/visits and generates a new deterministic sabotage loadout; contextual Q-values continue within the same campaign.
- A new campaign resets both memory layers.

## MVP non-goals

No multiplayer, combat, moving hazards, physics platforming, neural-network RL, online services, between-campaign save progression or guaranteed generalization to unseen layouts.
