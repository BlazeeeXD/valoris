# Verification status for the merged difficulty build

## Included in this archive

- Complete Unity project, scene, project settings, packages, AI Core, CLI, fixtures, tests, and benchmark tooling.
- A's updated Unity generation code: seeded randomized depth-first maze carving plus opened loops / alternative routes, with corresponding procedural-level-generator and campaign progression changes.
- A's non-Wall-Breaker Unity / UI changes, including the updated scene and inventory UI.
- Original four-action AI contract restored. The Wall Breaker actions, state flags, special validation path, evaluator animation handling, and Wall Breaker-specific tests have been removed.
- Original adaptive dual-memory Q-learning remains in place. Spatial memory resets between levels; contextual memory persists across a campaign and resets at a new campaign.
- Easy / Normal / Hard selection overlay. Easy uses a deliberately weak local greedy heuristic (distance + small 0.25 revisit cost) without Q-learning and a 230-step evaluation cap by default. Normal uses the original dual-memory adaptive Q-learning agent with 500 episodes per level and persistent contextual memory; its final replay includes an 8-step recent-position loop guard that only overrides a move when the greedy choice would revisit a recent tile and another legal alternative exists. Hard keeps the original greedy evaluator and agent with 3,000 episodes per level.
- `Directory.Build.props` redirects the Core .NET build output to `.dotnet` outside the Unity package folder. The Unity compilation cleanup script is included for stale local `bin` / `obj` artifacts.
- Unused C# TODO placeholders and the unsafe cleanup script that removed the active `BenchmarkCommand.cs` have been omitted.

## Static checks performed for this archive

- Removed-feature audit: no Wall Breaker action, state, or environment symbols remain in the source code.
- Project file and configuration inspection: Core `.csproj`, `Directory.Build.props`, package manifest / lock file, and Unity assembly definitions were inspected for consistency.
- Difficulty wiring inspection: the selected mode flows from the Unity difficulty menu through `TrainingOptions` into `ValorisAgent`; Easy branches to the local heuristic and gets a shorter evaluation cap, while Normal / Hard use the adaptive transfer learner. The short-loop replay guard is enabled only for Normal.
- Core tests include Easy skipping Q-learning updates, Normal cross-level memory persistence, and a new regression test proving that Normal final evaluation breaks a deliberate two-position oscillation when an alternate route exists.
- Static project audit after this balance patch: all 8 JSON files and 4 MSBuild XML files parsed; all 4 Unity `.asmdef` files also parsed as JSON; 110 C# files passed a delimiter-balance smoke check; 235 Unity `.meta` GUIDs were unique; no missing `.csproj` project references were found. This is a static consistency check, not a C# compile.
- The Core action contract and Q-table remain four-action; the CLI `BenchmarkCommand.cs` remains present. The unsafe stub-removal script is not included.

## Checks not performed here

The packaging environment does not have `dotnet`, a C# compiler, or the Unity Editor available. Therefore this exact archive has **not** been freshly compiled, xUnit-tested, imported by Unity, run in Play Mode, or built as a player. The earlier V0.1 report of 60 passing Core tests is historical and must not be treated as verification of this merged archive. Run `QUICK_START_DIFFICULTY.md` and `Docs/TEST_PLAN.md` on the Windows development machine before treating the package as verified.

## Generation note

The current A-side maze generator creates a seeded DFS maze and opens a fraction of remaining internal connections to add loops. The shipped gameplay configuration and the updated EditMode tests use the default corner endpoints: start `(0,0)`, goal `(23,23)`. Do not assume arbitrary inspector endpoints are guaranteed to connect without further validation/testing.
