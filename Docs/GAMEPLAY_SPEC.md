# Gameplay specification

## Goal

The player tries to prevent the AI from reaching the goal. The selected agent runs on the final, player-modified map. AI success gives the AI a point; any final-run failure gives the player a point.

## AI difficulty

- **Easy:** a simple local heuristic with no Q-learning and no knowledge transferred between levels.
- **Normal:** adaptive spatial/contextual Q-learning with 500 training episodes per level by default. Contextual learning persists across levels.
- **Hard:** the original full adaptive Q-learning agent with 3,000 training episodes per level by default.

The selected mode remains active for the campaign. Episode budgets are adjustable balance values; they do not guarantee a particular win rate.

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
| `N` | Start the next level after the result replay |
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
- The final agent run, not the best training episode, decides each point.
- A new level resets spatial Q-values/visits and generates a new deterministic sabotage loadout; contextual Q-values continue within the same campaign.
- A new campaign resets both memory layers.

## MVP non-goals

No multiplayer, combat, moving hazards, physics platforming, neural-network RL, online services, between-campaign save progression or guaranteed generalization to unseen layouts.
