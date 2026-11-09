# Creates the initial Valoris repository structure inside an existing Unity project.
# Run this from the project root: the folder containing Assets, Packages, and ProjectSettings.

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$root = (Get-Location).Path
$required = @('Assets', 'Packages', 'ProjectSettings')
foreach ($name in $required) {
    if (-not (Test-Path (Join-Path $root $name))) {
        throw "Run this script from the Unity project root. Missing folder: $name"
    }
}

$directories = @(
    'Assets\_Valoris\Art\Tilesets',
    'Assets\_Valoris\Art\Characters',
    'Assets\_Valoris\Art\Items',
    'Assets\_Valoris\Art\Effects',
    'Assets\_Valoris\Art\UI',
    'Assets\_Valoris\Audio\Music',
    'Assets\_Valoris\Audio\SFX',
    'Assets\_Valoris\Animations\Agent',
    'Assets\_Valoris\Animations\Environment',
    'Assets\_Valoris\Animations\UI',
    'Assets\_Valoris\Prefabs\Agent',
    'Assets\_Valoris\Prefabs\Environment',
    'Assets\_Valoris\Prefabs\Items',
    'Assets\_Valoris\Prefabs\UI',
    'Assets\_Valoris\Scenes',
    'Assets\_Valoris\ScriptableObjects\Items',
    'Assets\_Valoris\ScriptableObjects\Campaign',
    'Assets\_Valoris\ScriptableObjects\Configuration',
    'Assets\_Valoris\Scripts\Game\Generation',
    'Assets\_Valoris\Scripts\Game\Placement',
    'Assets\_Valoris\Scripts\Game\Inventory',
    'Assets\_Valoris\Scripts\Game\Campaign',
    'Assets\_Valoris\Scripts\Game\Controllers',
    'Assets\_Valoris\Scripts\Presentation\Grid',
    'Assets\_Valoris\Scripts\Presentation\Agent',
    'Assets\_Valoris\Scripts\Presentation\UI',
    'Assets\_Valoris\Scripts\Presentation\Effects',
    'Assets\_Valoris\Scripts\Integration',
    'Assets\_Valoris\Scripts\Editor\DebugTools',
    'Assets\_Valoris\Tests\EditMode',
    'Assets\_Valoris\Tests\PlayMode',

    'Packages\com.valoris.core\Runtime\Contracts',
    'Packages\com.valoris.core\Runtime\Grid',
    'Packages\com.valoris.core\Runtime\Learning',
    'Packages\com.valoris.core\Runtime\Training',
    'Packages\com.valoris.core\Runtime\Diagnostics',

    'Tools\Valoris.Cli\Commands',
    'Tools\Valoris.Cli\Rendering',
    'Tools\Valoris.Cli\Serialization',
    'Tools\Valoris.Cli\Reporting',

    'Tests\Valoris.Core.Tests\Grid',
    'Tests\Valoris.Core.Tests\Learning',
    'Tests\Valoris.Core.Tests\Training',
    'Tests\Valoris.Core.Tests\Helpers',

    'Data\Maps\Fixtures',
    'Data\Experiments',
    'Data\Results',
    'Docs'
)

foreach ($relative in $directories) {
    New-Item -ItemType Directory -Force -Path (Join-Path $root $relative) | Out-Null
}

function Write-NewFile {
    param(
        [Parameter(Mandatory = $true)][string]$RelativePath,
        [Parameter(Mandatory = $true)][string]$Content
    )
    $fullPath = Join-Path $root $RelativePath
    if (-not (Test-Path $fullPath)) {
        $parent = Split-Path -Parent $fullPath
        if ($parent -and -not (Test-Path $parent)) {
            New-Item -ItemType Directory -Force -Path $parent | Out-Null
        }
        [System.IO.File]::WriteAllText($fullPath, $Content, [System.Text.UTF8Encoding]::new($false))
    }
}

function Add-PlaceholderFiles {
    param([string[]]$RelativePaths)
    foreach ($relative in $RelativePaths) {
        Write-NewFile -RelativePath $relative -Content "// TODO: Implement this component. See Docs/ARCHITECTURE.md and Docs/INTERFACE_CONTRACT.md.`r`n"
    }
}

# Shared, engine-independent core package source files.
$coreFiles = @(
    'Packages\com.valoris.core\Runtime\Contracts\CellType.cs',
    'Packages\com.valoris.core\Runtime\Contracts\GridPosition.cs',
    'Packages\com.valoris.core\Runtime\Contracts\MoveAction.cs',
    'Packages\com.valoris.core\Runtime\Contracts\TerminationReason.cs',
    'Packages\com.valoris.core\Runtime\Contracts\LevelDefinition.cs',
    'Packages\com.valoris.core\Runtime\Contracts\GridObservation.cs',
    'Packages\com.valoris.core\Runtime\Contracts\TrainingOptions.cs',
    'Packages\com.valoris.core\Runtime\Contracts\EpisodeMetric.cs',
    'Packages\com.valoris.core\Runtime\Contracts\EvaluationResult.cs',
    'Packages\com.valoris.core\Runtime\Contracts\TrainingResult.cs',
    'Packages\com.valoris.core\Runtime\Contracts\AgentMemorySummary.cs',

    'Packages\com.valoris.core\Runtime\Grid\GridEnvironment.cs',
    'Packages\com.valoris.core\Runtime\Grid\GridRules.cs',
    'Packages\com.valoris.core\Runtime\Grid\LevelValidator.cs',

    'Packages\com.valoris.core\Runtime\Learning\QLearningAgent.cs',
    'Packages\com.valoris.core\Runtime\Learning\QTable.cs',
    'Packages\com.valoris.core\Runtime\Learning\StateEncoder.cs',
    'Packages\com.valoris.core\Runtime\Learning\RewardModel.cs',
    'Packages\com.valoris.core\Runtime\Learning\AgentMemory.cs',
    'Packages\com.valoris.core\Runtime\Learning\EpsilonGreedyPolicy.cs',

    'Packages\com.valoris.core\Runtime\Training\TrainingRunner.cs',
    'Packages\com.valoris.core\Runtime\Training\EpisodeRunner.cs',
    'Packages\com.valoris.core\Runtime\Training\PolicyEvaluator.cs',
    'Packages\com.valoris.core\Runtime\Training\MetricsCollector.cs',

    'Packages\com.valoris.core\Runtime\Diagnostics\SimulationTrace.cs',
    'Packages\com.valoris.core\Runtime\Diagnostics\SimulationEvent.cs'
)
Add-PlaceholderFiles $coreFiles

# Unity/game-side scripts (Developer A in the swapped role assignment).
$gameFiles = @(
    'Assets\_Valoris\Scripts\Game\Generation\ProceduralLevelGenerator.cs',
    'Assets\_Valoris\Scripts\Game\Generation\MazeGenerator.cs',
    'Assets\_Valoris\Scripts\Game\Generation\LevelBuilder.cs',

    'Assets\_Valoris\Scripts\Game\Placement\PlacementController.cs',
    'Assets\_Valoris\Scripts\Game\Placement\PlacementRules.cs',

    'Assets\_Valoris\Scripts\Game\Inventory\InventoryManager.cs',
    'Assets\_Valoris\Scripts\Game\Inventory\ItemDefinition.cs',

    'Assets\_Valoris\Scripts\Game\Campaign\CampaignManager.cs',
    'Assets\_Valoris\Scripts\Game\Campaign\CampaignState.cs',
    'Assets\_Valoris\Scripts\Game\Campaign\CampaignRules.cs',

    'Assets\_Valoris\Scripts\Game\Controllers\GameController.cs',
    'Assets\_Valoris\Scripts\Game\Controllers\GameSession.cs',
    'Assets\_Valoris\Scripts\Game\Controllers\GameFlowController.cs',

    'Assets\_Valoris\Scripts\Presentation\Grid\TilemapGridRenderer.cs',
    'Assets\_Valoris\Scripts\Presentation\Grid\GridInputController.cs',
    'Assets\_Valoris\Scripts\Presentation\Grid\CellVisualMapper.cs',

    'Assets\_Valoris\Scripts\Presentation\Agent\AgentRenderer.cs',
    'Assets\_Valoris\Scripts\Presentation\Agent\AgentPathAnimator.cs',
    'Assets\_Valoris\Scripts\Presentation\Agent\AgentVisualState.cs',

    'Assets\_Valoris\Scripts\Presentation\UI\MainMenuController.cs',
    'Assets\_Valoris\Scripts\Presentation\UI\InventoryUIController.cs',
    'Assets\_Valoris\Scripts\Presentation\UI\TrainingUIController.cs',
    'Assets\_Valoris\Scripts\Presentation\UI\CampaignHUDController.cs',
    'Assets\_Valoris\Scripts\Presentation\UI\ResultsUIController.cs',

    'Assets\_Valoris\Scripts\Presentation\Effects\CellHighlight.cs',
    'Assets\_Valoris\Scripts\Presentation\Effects\PlacementFeedback.cs',

    'Assets\_Valoris\Scripts\Integration\ValorisAgentBridge.cs',
    'Assets\_Valoris\Scripts\Integration\TrainingPlayback.cs',
    'Assets\_Valoris\Scripts\Integration\CampaignIntegration.cs',
    'Assets\_Valoris\Scripts\Integration\LevelSnapshotFactory.cs',

    'Assets\_Valoris\Scripts\Editor\DebugTools\LevelDebugWindow.cs',
    'Assets\_Valoris\Scripts\Editor\DebugTools\GridGizmos.cs'
)
Add-PlaceholderFiles $gameFiles

# Unity assemblies: the game layer references the engine-independent core package.
Write-NewFile 'Assets\_Valoris\Scripts\Valoris.Game.asmdef' @'
{
  "name": "Valoris.Game",
  "rootNamespace": "Valoris.Game",
  "references": [
    "Valoris.Core"
  ],
  "includePlatforms": [],
  "excludePlatforms": [],
  "allowUnsafeCode": false,
  "overrideReferences": false,
  "precompiledReferences": [],
  "autoReferenced": true,
  "defineConstraints": [],
  "versionDefines": [],
  "noEngineReferences": false
}
'@

Write-NewFile 'Assets\_Valoris\Scripts\Editor\Valoris.Game.Editor.asmdef' @'
{
  "name": "Valoris.Game.Editor",
  "rootNamespace": "Valoris.Game.Editor",
  "references": [
    "Valoris.Game"
  ],
  "includePlatforms": [
    "Editor"
  ],
  "excludePlatforms": [],
  "allowUnsafeCode": false,
  "overrideReferences": false,
  "precompiledReferences": [],
  "autoReferenced": true,
  "defineConstraints": [],
  "versionDefines": [],
  "noEngineReferences": false
}
'@

# CLI files (Developer B's headless testing environment).
$cliFiles = @(
    'Tools\Valoris.Cli\Commands\TrainCommand.cs',
    'Tools\Valoris.Cli\Commands\EvaluateCommand.cs',
    'Tools\Valoris.Cli\Commands\InspectCommand.cs',
    'Tools\Valoris.Cli\Commands\BenchmarkCommand.cs',
    'Tools\Valoris.Cli\Rendering\AsciiGridRenderer.cs',
    'Tools\Valoris.Cli\Serialization\LevelJsonReader.cs',
    'Tools\Valoris.Cli\Serialization\LevelJsonWriter.cs',
    'Tools\Valoris.Cli\Reporting\ConsoleReporter.cs',
    'Tools\Valoris.Cli\Reporting\CsvExporter.cs'
)
Add-PlaceholderFiles $cliFiles

# Core unit-test placeholders.
$testFiles = @(
    'Tests\Valoris.Core.Tests\Grid\GridMovementTests.cs',
    'Tests\Valoris.Core.Tests\Grid\CollisionTests.cs',
    'Tests\Valoris.Core.Tests\Grid\LevelValidationTests.cs',
    'Tests\Valoris.Core.Tests\Learning\QTableTests.cs',
    'Tests\Valoris.Core.Tests\Learning\QLearningUpdateTests.cs',
    'Tests\Valoris.Core.Tests\Learning\StateEncoderTests.cs',
    'Tests\Valoris.Core.Tests\Learning\MemoryPersistenceTests.cs',
    'Tests\Valoris.Core.Tests\Training\EpisodeTests.cs',
    'Tests\Valoris.Core.Tests\Training\EvaluationTests.cs',
    'Tests\Valoris.Core.Tests\Helpers\TestLevelFactory.cs'
)
Add-PlaceholderFiles $testFiles

# Package metadata and assembly configuration.
Write-NewFile 'Packages\com.valoris.core\package.json' @'
{
  "name": "com.valoris.core",
  "version": "0.1.0",
  "displayName": "Valoris Core",
  "description": "Engine-independent grid simulation and reinforcement-learning core for Valoris.",
  "keywords": [
    "valoris",
    "gridworld",
    "reinforcement-learning",
    "q-learning"
  ]
}
'@

Write-NewFile 'Packages\com.valoris.core\Runtime\Valoris.Core.asmdef' @'
{
  "name": "Valoris.Core",
  "rootNamespace": "Valoris.Core",
  "references": [],
  "includePlatforms": [],
  "excludePlatforms": [],
  "allowUnsafeCode": false,
  "overrideReferences": false,
  "precompiledReferences": [],
  "autoReferenced": true,
  "defineConstraints": [],
  "versionDefines": [],
  "noEngineReferences": true
}
'@

Write-NewFile 'Packages\com.valoris.core\Valoris.Core.csproj' @'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>netstandard2.1</TargetFramework>
    <LangVersion>9.0</LangVersion>
    <Nullable>disable</Nullable>
    <ImplicitUsings>disable</ImplicitUsings>
    <EnableDefaultCompileItems>false</EnableDefaultCompileItems>
    <AssemblyName>Valoris.Core</AssemblyName>
    <RootNamespace>Valoris.Core</RootNamespace>
  </PropertyGroup>
  <ItemGroup>
    <Compile Include="Runtime/**/*.cs" />
  </ItemGroup>
</Project>
'@

# CLI project: references exactly the same core source project used by Unity.
Write-NewFile 'Tools\Valoris.Cli\Valoris.Cli.csproj' @'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="../../Packages/com.valoris.core/Valoris.Core.csproj" />
  </ItemGroup>
</Project>
'@

Write-NewFile 'Tools\Valoris.Cli\Program.cs' @'
using System;

internal static class Program
{
    private static int Main(string[] args)
    {
        Console.WriteLine("Valoris CLI scaffold ready. Commands will be implemented next.");
        return 0;
    }
}
'@

# Unit-test project. Package restore happens when dotnet test is run.
Write-NewFile 'Tests\Valoris.Core.Tests\Valoris.Core.Tests.csproj' @'
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="18.11.0" />
    <PackageReference Include="xunit.v3" Version="4.0.2" />
    <PackageReference Include="xunit.runner.visualstudio" Version="4.0.1">
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="../../Packages/com.valoris.core/Valoris.Core.csproj" />
  </ItemGroup>
</Project>
'@

# Documentation placeholders.
$docs = @(
    'Docs\ARCHITECTURE.md',
    'Docs\INTERFACE_CONTRACT.md',
    'Docs\AI_DESIGN.md',
    'Docs\GAMEPLAY_SPEC.md',
    'Docs\TEST_PLAN.md',
    'Docs\ASSET_CREDITS.md',
    'Docs\CONTRIBUTING.md',
    'Docs\LEVEL_JSON_SCHEMA.md',
    'Data\Maps\Fixtures\README.md',
    'Data\Experiments\README.md',
    'Data\Results\README.md',
    'Assets\_Valoris\README.md',
    'Assets\_Valoris\Scenes\README.md',
    'Assets\_Valoris\Tests\EditMode\README.md',
    'Assets\_Valoris\Tests\PlayMode\README.md'
)
foreach ($relative in $docs) {
    $title = [System.IO.Path]::GetFileNameWithoutExtension($relative)
    Write-NewFile $relative "# $title`r`n`r`nTODO: Document this area as implementation progresses.`r`n"
}

Write-NewFile 'README.md' @'
# Valoris

Development repository for the Valoris 2D grid-based roguelite and persistent Q-learning agent.

## Repository layout

- `Assets/_Valoris/` — Unity game, visuals, gameplay, and integration.
- `Packages/com.valoris.core/` — engine-independent simulation and AI core.
- `Tools/Valoris.Cli/` — headless terminal runner for training and evaluation.
- `Tests/Valoris.Core.Tests/` — automated tests for the core library.
- `Data/` — fixed map fixtures and experiment outputs.
- `Docs/` — architecture, contracts, and development notes.

Open this repository's root folder in Unity Hub. The Unity project root is this folder.
'@

Write-NewFile '.gitignore' @'
# Unity-generated data
[Ll]ibrary/
[Tt]emp/
[Oo]bj/
[Bb]uild/
[Bb]uilds/
[Ll]ogs/
[Uu]serSettings/
MemoryCaptures/
Recordings/

# Unity-generated solution/project files at repository root only.
/*.csproj
/*.sln
/*.slnx

# IDE and user-specific files
.vs/
.idea/
*.user
*.suo
*.userprefs

# .NET build output
**/bin/
**/obj/

# OS files
.DS_Store
Thumbs.db
Desktop.ini

# Logs and crash output
*.log
sysinfo.txt
'@

Write-NewFile '.gitattributes' @'
* text=auto
*.cs text eol=lf
*.csproj text eol=lf
*.json text eol=lf
*.asmdef text eol=lf
*.md text eol=lf
*.meta text eol=lf
*.unity text eol=lf
*.prefab text eol=lf
*.asset text eol=lf
*.png binary
*.jpg binary
*.jpeg binary
*.wav binary
*.ogg binary
*.mp3 binary
'@

Write-NewFile '.editorconfig' @'
root = true

[*]
charset = utf-8
end_of_line = lf
insert_final_newline = true
indent_style = space
indent_size = 4

[*.cs]
indent_size = 4

[*.json]
indent_size = 2

[*.md]
trim_trailing_whitespace = false
'@

# Keep empty asset/data folders visible to Git without creating fake Unity scenes/assets.
$keepFolders = @(
    'Assets\_Valoris\Art\Tilesets', 'Assets\_Valoris\Art\Characters',
    'Assets\_Valoris\Art\Items', 'Assets\_Valoris\Art\Effects', 'Assets\_Valoris\Art\UI',
    'Assets\_Valoris\Audio\Music', 'Assets\_Valoris\Audio\SFX',
    'Assets\_Valoris\Animations\Agent', 'Assets\_Valoris\Animations\Environment', 'Assets\_Valoris\Animations\UI',
    'Assets\_Valoris\Prefabs\Agent', 'Assets\_Valoris\Prefabs\Environment', 'Assets\_Valoris\Prefabs\Items', 'Assets\_Valoris\Prefabs\UI',
    'Assets\_Valoris\ScriptableObjects\Items', 'Assets\_Valoris\ScriptableObjects\Campaign', 'Assets\_Valoris\ScriptableObjects\Configuration'
)
foreach ($folder in $keepFolders) {
    Write-NewFile -RelativePath (Join-Path $folder '.gitkeep') -Content "# Keeps this directory in Git.`r`n"
}
foreach ($folder in @('Data\Maps\Fixtures', 'Data\Experiments', 'Data\Results')) {
    Write-NewFile -RelativePath (Join-Path $folder '.gitkeep') -Content "# Keeps this directory in Git.`r`n"
}

Write-Host ''
Write-Host 'Valoris structure created.' -ForegroundColor Green
Write-Host "Root: $root"
Write-Host 'Existing Unity files and Packages/manifest.json were not overwritten.'
Write-Host 'Next: reopen/refresh Unity, then run dotnet --list-sdks before using the CLI.'
