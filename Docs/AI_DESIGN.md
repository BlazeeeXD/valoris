# Valoris AI Design — position-aware Q-learning baseline

## Objective

Build a tabular reinforcement-learning agent for a deterministic 24 x 24 grid. One `ValorisAgent` instance represents one campaign. Its Q-table remains available across levels in that campaign and is cleared by `BeginCampaign`.

## Problems found in the first baseline

1. The old state encoder omitted the agent's absolute position. Distinct intersections with identical local observations and similar goal direction/distance were collapsed into the same Q-state. This caused state aliasing even within one fixed map.
2. The old policy selected from all four actions, including moves into walls and out of bounds. These invalid moves consumed training steps and could be selected repeatedly during greedy evaluation.
3. The old default epsilon decay (`0.995` per episode) reached the 0.05 floor after approximately 598 episodes. On longer sparse-reward mazes, exploration was reduced too early.
4. Greedy evaluation resolved equal Q-values by always selecting the first action. An unseen state therefore defaulted to Up, even when Up was blocked.
5. The Q-learning bootstrap maximum included blocked actions, allowing values for invalid moves to influence updates.

## Current state encoding

The encoder now includes:

- Absolute agent position `(x, y)` on the fixed 24 x 24 board.
- Four adjacent cell types (north, east, south, west).
- Goal-relative X and Y direction signs.
- A coarse Manhattan-distance band.

The encoded key is sparse in the Q-table. The theoretical key space is:

`24 * 24 * 4^4 * 3^3 = 3,981,312`

This distinguishes different positions within a fixed level, while the local terrain context avoids blindly reusing a value when nearby cells have changed between levels. It does not guarantee perfect generalization across arbitrary maps: two maps can still share the same position and local observation while having different distant topology.

## Legal-action masking

Walls and out-of-bounds neighbours are excluded during both exploratory and greedy action selection. The Q-learning bootstrap maximum is also calculated only over legal next actions. Traps remain selectable because entering a trap is a real game action with a terminal penalty; mud remains traversable.

Wall collision semantics remain in `GridEnvironment` as a defensive rule and for direct simulation tests, but the learning policy should not deliberately choose a known wall.

## Q-learning update

For each transition:

`Q(s,a) <- Q(s,a) + alpha * (r + gamma * max_legal_a' Q(s',a') - Q(s,a))`

For terminal transitions, the bootstrap term is zero. Training uses epsilon-greedy action selection over legal actions. Evaluation disables exploration and randomizes only among equal-valued legal actions using the seeded run RNG.

## Exploration defaults

- Initial epsilon: 1.0
- Minimum epsilon: 0.05
- Per-episode decay: 0.9995

At 1,000 episodes epsilon is approximately 0.606; at 3,000 episodes approximately 0.223; the floor is reached after roughly 5,990 episodes. These are starting parameters, not universal optima. Record any overrides in experiments.

## Reward defaults

- Goal: +100
- Trap: -100
- Wall attempt: -3 in addition to ordinary step cost
- Mud: -2 in addition to ordinary step cost
- Ordinary move: -0.5
- Step limit: -25 additional terminal penalty

Rewards are initial parameters. Tune them only through recorded experiments.

## Lifecycle

1. Create one agent and call `BeginCampaign(seed)` once.
2. Call `TrainLevel(level, options)` for each immutable locked level.
3. The same Q-table remains in memory between levels in the campaign.
4. Each level starts a fresh exploration schedule but keeps the learned Q-values.
5. The returned greedy evaluation alone determines the level winner.
6. Start a new campaign with `BeginCampaign` to clear memory.

## Evaluation discipline

Compare persistent and reset-memory agents on identical map fixtures, map order, training budgets and seed sets. A single benchmark run is not sufficient evidence that persistent memory helps. Report success rate, evaluation steps, reward and learning curves across multiple seeds.

## Hard-map regression baseline

The supplied `hard_01.json` fixture has a safe BFS route of 72 steps. The revised encoder, legal-action mask and slower exploration schedule are intended to make the 3,000-episode seeded regression test solve this fixture. This must be confirmed by running the C# test suite on the development machine; the Python-side diagnostic simulation is not a substitute for compiling and executing the C# implementation.
