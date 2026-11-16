@echo off
setlocal

echo Removing obsolete empty scaffold files...

if exist "Packages\com.valoris.core\Runtime\Learning\QLearningAgent.cs" del /q "Packages\com.valoris.core\Runtime\Learning\QLearningAgent.cs"
if exist "Packages\com.valoris.core\Runtime\Learning\RewardModel.cs" del /q "Packages\com.valoris.core\Runtime\Learning\RewardModel.cs"
if exist "Tools\Valoris.Cli\Valoris.Core.Cli.cs" del /q "Tools\Valoris.Cli\Valoris.Core.Cli.cs"

if exist "Tools\Valoris.Cli\Commands\TrainCommand.cs" del /q "Tools\Valoris.Cli\Commands\TrainCommand.cs"
if exist "Tools\Valoris.Cli\Commands\EvaluateCommand.cs" del /q "Tools\Valoris.Cli\Commands\EvaluateCommand.cs"
if exist "Tools\Valoris.Cli\Commands\BenchmarkCommand.cs" del /q "Tools\Valoris.Cli\Commands\BenchmarkCommand.cs"
if exist "Tools\Valoris.Cli\Commands\InspectCommand.cs" del /q "Tools\Valoris.Cli\Commands\InspectCommand.cs"

if exist "Tools\Valoris.Cli\Commands" rmdir "Tools\Valoris.Cli\Commands" 2>nul

echo Done. Active CLI entry points are in CliApplication.cs and its supporting services.
endlocal
