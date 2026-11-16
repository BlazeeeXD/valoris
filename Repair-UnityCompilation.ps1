$ErrorActionPreference = 'Stop'
$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
$coreRoot = Join-Path $projectRoot 'Packages\com.valoris.core'

if (-not (Test-Path (Join-Path $coreRoot 'Valoris.Core.csproj'))) {
    throw "Run this script from the extracted project root. Could not find Packages\com.valoris.core\Valoris.Core.csproj."
}

# Unity must not import the .NET build output as a second assembly alongside Valoris.Core.asmdef.
foreach ($name in @('bin', 'obj')) {
    $path = Join-Path $coreRoot $name
    if (Test-Path $path) {
        Remove-Item -LiteralPath $path -Recurse -Force
        Write-Host "Removed $path"
    }
}

Write-Host 'Unity compilation cleanup complete.' -ForegroundColor Green
Write-Host 'The updated Directory.Build.props redirects future Valoris.Core build output to .dotnet\.'
Write-Host 'Now reopen Unity and allow scripts to recompile.'
