# Valoris AI Design — dual-memory adaptive Q-learning

## Objective

Build a tabular reinforcement-learning agent for a deterministic 24 × 24 grid. One `ValorisAgent` instance represents one campaign. The agent learns on each locked level, reuses transferable contextual knowledge between levels, and clears map-specific spatial knowledge when a new level begins.

## State representations

### Spatial state

`StateEncoder` combines:

- Absolute agent position `(x, y)` on the 24 × 24 board.
- North, east, south and west cell types.
- Goal-relative X and Y direction signs.
- A coarse Manhattan-distance band (`<= 3`, `<= 8`, `> 8`).

The theoretical key space is `24 × 24 × 4^4 × 3^3 = 3,981,312`. Q-values are allocated sparsely only for visited states. Position-aware encoding prevents distant intersections with identical local observations from collapsing into one spatial state on the same board.

### Context state

`ContextEncoder` uses the four neighbouring cell types plus the same goal-relative direction and distance band, but excludes absolute position. Its theoretical key space is `4^4 × 3^3 = 6,912`. This intentionally aliases states that look locally similar, including cases where distant topology differs. Contextual memory is therefore a transfer hint, not a map-complete representation.

## Dual-memory policy

Every learning transition updates both Q-tables independently with the same reward and legal-action Bellman update:

- **Spatial table:** map-specific; cleared by `AgentMemory.BeginLevel()`.
- **Contextual table:** transferable; retained between levels in the same campaign.
- **New campaign:** `BeginCampaign(seed)` clears both tables, visit counts and campaign counters.

At action selection, the two Q-values are blended with an action-specific contextual weight:

`w_context(s,a) = w_max / (1 + N_spatial(s,a) / k)`

`Q_combined(s,a) = (1 - w_context) Q_spatial(s,a) + w_context Q_context(s,a)`

Defaults are `w_max = 0.30` and `k = 20` spatial state-action visits. Thus the contextual influence is 30% before local evidence, 15% after 20 visits and 7.5% after 60 visits. Visit counts are per state-action pair; evidence for one move does not suppress transfer for the other three moves.

This formula is an experimental heuristic. It is not claimed to be optimal. The previous fixed 30% blend failed the final hard-map evaluation while the spatial-only ablation succeeded; adaptive weighting restored a successful hard-map evaluation in the latest supplied run. The cross-map benchmark exists to measure whether contextual transfer actually helps on a broader set.

## Environment and learning rules

- Top-left origin; X increases right; Y increases down; row-major index `y * width + x`.
- Actions are Up, Right, Down and Left. No diagonal movement.
- Walls and out-of-bounds cells are masked during exploration, greedy selection and Bellman bootstrapping. Direct environment calls still handle wall collisions defensively.
- Traps remain selectable and terminate the episode as failure.
- Mud is traversable and applies an additional penalty.
- Reaching the goal ends the episode successfully.
- Epsilon-greedy training uses a seeded RNG. Final evaluation uses greedy selection with exploration disabled and seeded tie-breaking.
- Reaching the configured step limit terminates the episode with the timeout penalty.

## Reward defaults

| Event | Reward effect |
|---|---:|
| Reach goal | +100 |
| Enter trap | -100 |
| Attempt wall/out-of-bounds | -3 plus ordinary step cost |
| Enter mud | -2 plus ordinary step cost |
| Ordinary move | -0.5 |
| Exceed step limit | -25 additional terminal penalty |

These are initial experiment parameters; do not tune them without recording the exact configuration.

## Lifecycle

1. Create one `ValorisAgent` and call `BeginCampaign(campaignSeed)` once.
2. For each immutable, validated `LevelDefinition`, call `TrainLevel(level, options)`.
3. `TrainLevel` clears only spatial memory before training; contextual Q-values remain available.
4. Run the fixed training budget and then a final greedy evaluation with exploration disabled.
5. Only `FinalEvaluation.ReachedGoal` determines the level result.
6. Call `BeginCampaign` to reset both memories for the next campaign.

## Evaluation discipline

A single run cannot establish transfer quality. Compare reset-memory and persistent-context agents on the exact same map sequence, episode budget and seed. Repeat over multiple run seeds. Report success rate, evaluation steps, reward and memory size. A positive average on a fixture set is evidence for that set only; test procedurally generated maps before making a generalization claim.
