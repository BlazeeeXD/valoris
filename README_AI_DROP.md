# Valoris AI Core — first implementation drop

This archive contains the headless Core package, CLI, tests, and 24x24 JSON fixtures.

## Copy

Extract this archive into the existing Unity project root (`valoris`) and allow files to merge. It intentionally does not include or replace `UnityProject` folders because the Unity project is already at the repository root. It also does not modify `Packages/manifest.json`.

## Build

```powershell
dotnet --list-sdks
dotnet build .\Packages\com.valoris.core\Valoris.Core.csproj
dotnet build .\Tools\Valoris.Cli\Valoris.Cli.csproj
dotnet test .\Tests\Valoris.Core.Tests\Valoris.Core.Tests.csproj
```

```powershell
dotnet run --project .\Tools\Valoris.Cli -- inspect --map .\Data\Maps\Fixtures\easy_01.json
dotnet run --project .\Tools\Valoris.Cli -- train --map .\Data\Maps\Fixtures\easy_01.json --episodes 1000 --seed 7 --csv .\Data\Results\easy_01.csv
dotnet run --project .\Tools\Valoris.Cli -- benchmark --maps .\Data\Maps\Fixtures --episodes 1000 --seed 7
```

`evaluate` runs training followed by the final greedy evaluation for a single fixture. `benchmark` compares a fresh agent per map against one agent retaining its Q-table across the ordered fixture set. This first benchmark is a software experiment harness; use repeated seeds before drawing research conclusions.

## Important

The package has not been compiled in this generation environment because the .NET SDK is not installed there. Run the build and test commands locally and report the first compiler error verbatim if one appears. Do not silently alter the contract to work around an error.
