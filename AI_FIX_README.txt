VALORIS AI FIX — PATCH CONTENTS

This patch updates the active headless AI pipeline and adds regression tests.

INSTALL
1. Close Unity.
2. Extract the contents of this ZIP into the existing valoris repository root.
3. Allow existing files to be overwritten when prompted.
4. Run REMOVE_UNUSED_STUBS.cmd from the repository root.
5. Build and test using the commands in Docs\AI_FIX_NOTES.md.
6. Reopen Unity after the Core build passes.

KEY CHANGES
- StateEncoder now includes absolute grid position + local terrain/goal context.
- Q-learning action selection and Bellman bootstrap ignore walls/out-of-bounds moves.
- Greedy ties are randomized with the seeded RNG instead of always selecting Up.
- Default epsilon decay is 0.9995 (previously 0.995).
- EpisodeRunner isolates one episode; TrainingRunner orchestrates episodes.
- CLI reports rolling 100-episode success rate and average reward.
- Hard-map regression test uses Data\Maps\Fixtures\hard_01.json.
- CLI accepts --alpha, --gamma, --epsilon-decay, --min-epsilon and --epsilon.

IMPORTANT
This patch is source-reviewed but was not compiled in the Linux authoring environment because the .NET SDK was unavailable there. Run the Windows build/test commands before merging. The regression test is intended to confirm the hard-map fix on your .NET 10 environment.
