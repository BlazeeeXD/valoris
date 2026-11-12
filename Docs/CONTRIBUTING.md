# Contributing and local development

## Repository shape

The repository root is also the Unity project root. Do not create a nested Unity project. Preserve the supplied `Packages/manifest.json` and `Packages/packages-lock.json`.

## Ownership

- Developer B: `Packages/com.valoris.core/`, `Tools/Valoris.Cli/`, `Tests/Valoris.Core.Tests/`, and the technical documentation for those systems.
- Developer A: Unity gameplay, placement, inventory, campaign, presentation and integration code under `Assets/_Valoris/`.
- The `Assets/_Valoris/Scripts/Game/Generation/` files in this handoff are the one authorized A-side generation segment; changes to other A-owned folders need the A owner's agreement.
- Shared contract changes require both developers to agree and update `Docs/INTERFACE_CONTRACT.md` in the same change.

## Toolchain

Use the Unity Editor version recorded in `ProjectSettings/ProjectVersion.txt` and the .NET 10 SDK for the CLI and test projects. The Core library targets .NET Standard 2.1 and C# 9.0 so Unity and the .NET CLI compile the same source.

## Build and test

Run from the repository root:

```powershell
dotnet build .\Packages\com.valoris.core\Valoris.Core.csproj
dotnet build .\Tools\Valoris.Cli\Valoris.Cli.csproj
dotnet test .\Tests\Valoris.Core.Tests\Valoris.Core.Tests.csproj
```

Useful terminal commands are documented in the root `README.md` and `Docs/TEST_PLAN.md`.

## Change discipline

- Keep the Core package free of `UnityEngine` references.
- Preserve the 24 × 24 top-left-origin, row-major contract.
- Prefer deterministic tests with explicit seeds.
- Do not change a reward or learning parameter without recording it in experiment output.
- Benchmark memory transfer against a reset baseline with the same map sequence, run seed and training options.
- Commit source, tests, fixtures and docs. Do not commit `Library/`, `Temp/`, `Logs/`, `UserSettings/`, `bin/` or `obj/`.
