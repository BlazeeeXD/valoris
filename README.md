# Oralis (Valoris project)

Oralis is a top-down 2D grid roguelite where the player places a limited random loadout of sabotage items on a seeded 24×24 map. After the board is locked, a headless tabular Q-learning agent trains on the exact immutable board snapshot, performs a final greedy evaluation, and replays the resulting path in Unity. A successful agent run gives the AI one campaign point; failure gives the player one point.

The repository root is both the Git root and the Unity project root. **Open this exact folder in Unity Hub.** This archive preserves the uploaded project's Unity Editor version (`6000.5.9f1`) and package manifest.

## Play the game

1. Open the repository root in Unity Hub and allow package import/compilation to finish.
2. Open `Assets/_Valoris/Scenes/MainGame.unity` (also configured as the first build scene).
3. Press Play.
4. Press `1`, `2` or `3` to select Wall, Trap or Mud (selection is based on each item's CellType, not its Inspector array order). Left-click an empty tile to place the selected sabotage. Right-click a player-placed item to remove it.
5. Press **Lock Board**. The game validates that a route still exists, freezes placement, trains the AI off the Unity main thread, then replays the final greedy path.
6. Press `N` to proceed to the next level. The first side to eight points wins; otherwise the campaign ends after fifteen levels. Press `R` after the campaign finishes to start a new one.

The HUD shows the level, score, remaining loadout, training/evaluation result and memory summary. The same level seed also determines the per-level item quantities.

## Build the headless AI and run tests

Install the .NET 10 SDK. From PowerShell or a terminal at this repository root:

```powershell
dotnet build .\Packages\com.valoris.core\Valoris.Core.csproj
dotnet build .\Tools\Valoris.Cli\Valoris.Cli.csproj
dotnet test .\Tests\Valoris.Core.Tests\Valoris.Core.Tests.csproj
```

Useful CLI commands:

```powershell
# Inspect a fixed 24×24 fixture and print it as ASCII.
dotnet run --project .\Tools\Valoris.Cli -- inspect --map .\Data\Maps\Fixtures\easy_01.json

# Train on the B1 hard-map regression fixture.
dotnet run --project .\Tools\Valoris.Cli -- train --map .\Data\Maps\Fixtures\hard_01.json --episodes 3000 --seed 7 --epsilon-decay 0.9995

# Generate a seeded, validated map JSON file.
dotnet run --project .\Tools\Valoris.Cli -- generate --seed 44 --level 2 --difficulty hard --out .\Data\Maps\Generated\hard_02.json

# Compare fresh-memory and campaign-persistent contextual memory across generated maps and seeds.
dotnet run --project .\Tools\Valoris.Cli -- benchmark --generated-maps 5 --difficulty hard --episodes 1000 --seeds 7,11,19 --csv .\Data\Results\transfer_summary.csv --episodes-csv .\Data\Results\transfer_episodes.csv
```

After the benchmark completes, analyze the summary CSV with the paired, run-seed cluster-bootstrap helper:

```powershell
python .\Tools\Analysis\analyze_benchmark.py .\Data\Results\transfer_summary.csv
```

See `Docs/TEST_PLAN.md`, `Docs/EXPERIMENTS.md` and `Docs/VERIFICATION_STATUS.md` for the full verification matrix and experiment interpretation.

## Repository layout

- `Assets/_Valoris/` — Unity scenes, tiles, ScriptableObjects, gameplay, input, UI and presentation.
- `Packages/com.valoris.core/` — Unity-independent contracts, validation, simulation, procedural benchmark maps, dual-memory Q-learning and training/evaluation.
- `Tools/Valoris.Cli/` — terminal inspection, training, generation and paired transfer benchmarks.
- `Tools/Analysis/` — benchmark CSV analysis helper.
- `Tests/Valoris.Core.Tests/` — .NET automated tests for Core and learning lifecycle.
- `Data/` — reproducible map fixtures and experiment outputs.
- `Docs/` — architecture, interface contract, gameplay, experiments and test plan.

## Unity-generated folders

`Library/`, `Temp/`, `Logs/`, `UserSettings/`, `bin/` and `obj/` are intentionally not included. Unity regenerates its local cache folders on first open; .NET regenerates build folders when the CLI/tests are built. Do not commit those generated folders.
