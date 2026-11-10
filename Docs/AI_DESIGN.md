# Valoris AI Design (initial implementation)

## Scope

The first implementation is a tabular Q-learning agent operating on a deterministic 24x24 grid. One `ValorisAgent` instance represents one campaign. Its Q-table is retained between levels and cleared by `BeginCampaign`.

## State encoding

`StateEncoder` encodes the cell types to the north, east, south and west; the sign of the goal-relative X and Y offsets; and a Manhattan-distance band (0-3, 4-8, 9+). This yields 4^4 * 3 * 3 * 3 = 6,912 possible discrete states. Out-of-bounds neighbours are observed as walls.

This local representation permits reuse of action preferences across maps. It is intentionally approximate: distant maze topology can differ while local observations match (state aliasing). Measure transfer against a reset-memory baseline; do not claim perfect generalization.

## Q-learning

For each transition, update:

Q(s,a) <- Q(s,a) + alpha * (r + gamma * max_a' Q(s',a') - Q(s,a))

For terminal transitions, the bootstrap term is zero. Training uses epsilon-greedy action selection; final evaluation is greedy with exploration disabled.

## Reward defaults

Goal +100, trap -100, wall attempt -3, mud extra -2, ordinary move -0.5, timeout extra -25. These are starting values, not claimed optimal parameters. Change them only as recorded experiments.

## Lifecycle

1. Create agent and call `BeginCampaign(seed)` once.
2. Call `TrainLevel(level, options)` for each immutable locked level.
3. The same Q-table remains in memory between levels.
4. The returned evaluation alone determines the level winner.
5. Start a new campaign with `BeginCampaign` to clear memory.

## Determinism

A run is keyed by campaign seed, training options seed, level seed and level number. Record those values with every experiment.
