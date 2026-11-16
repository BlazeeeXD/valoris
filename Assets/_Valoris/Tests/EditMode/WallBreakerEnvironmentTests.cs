
using NUnit.Framework;
using Valoris.Core.Contracts;
using Valoris.Core.Grid;

public class WallBreakerEnvironmentTests
{
    [Test]
    public void WallBreakerRemovesPlayerPlacedWallWithoutMovingAgent()
    {
        LevelDefinition level = CreateLevel(
            new[] { 1 },
            new[] { true }
        );

        var environment = new GridEnvironment();
        environment.Reset(level);

        Assert.That(environment.Observe().EastIsPlayerWall, Is.True);

        StepResult result = environment.Step(MoveAction.BreakRight);

        Assert.That(result.Position, Is.EqualTo(new GridPosition(0, 0)));
        Assert.That(result.Observation.East, Is.EqualTo(CellType.Empty));
        Assert.That(result.Observation.WallBreakerAvailable, Is.False);
        Assert.That(environment.WallBreakerAvailable, Is.False);
    }

    [Test]
    public void WallBreakerCannotDestroyPermanentMazeWall()
    {
        LevelDefinition level = CreateLevel(
            new[] { 1 },
            new[] { false }
        );

        var environment = new GridEnvironment();
        environment.Reset(level);

        Assert.Throws<System.InvalidOperationException>(
            () => environment.Step(MoveAction.BreakRight)
        );

        Assert.That(environment.WallBreakerAvailable, Is.True);
        Assert.That(environment.Observe().East, Is.EqualTo(CellType.Wall));
    }

    [Test]
    public void WallBreakerCanOnlyBeUsedOncePerEpisode()
    {
        LevelDefinition level = CreateLevel(
            new[] { 1, 24 },
            new[] { true, true }
        );

        var environment = new GridEnvironment();
        environment.Reset(level);

        environment.Step(MoveAction.BreakRight);

        Assert.That(environment.WallBreakerAvailable, Is.False);

        Assert.Throws<System.InvalidOperationException>(
            () => environment.Step(MoveAction.BreakDown)
        );
    }

    [Test]
    public void ResetEpisodeRestoresWallAndWallBreakerCharge()
    {
        LevelDefinition level = CreateLevel(
            new[] { 1 },
            new[] { true }
        );

        var environment = new GridEnvironment();
        environment.Reset(level);

        environment.Step(MoveAction.BreakRight);

        environment.ResetEpisode();

        GridObservation observation = environment.Observe();

        Assert.That(environment.Position, Is.EqualTo(level.Start));
        Assert.That(observation.East, Is.EqualTo(CellType.Wall));
        Assert.That(observation.EastIsPlayerWall, Is.True);
        Assert.That(observation.WallBreakerAvailable, Is.True);
    }

    private static LevelDefinition CreateLevel(
        int[] wallIndices,
        bool[] playerPlacedWallFlags)
    {
        const int gridSize = 24;
        const int cellCount = gridSize * gridSize;

        var cells = new int[cellCount];
        var playerPlacedWalls = new bool[cellCount];

        for (int i = 0; i < wallIndices.Length; i++)
        {
            int index = wallIndices[i];

            cells[index] = (int)CellType.Wall;
            playerPlacedWalls[index] = playerPlacedWallFlags[i];
        }

        return new LevelDefinition(
            LevelDefinition.CurrentSchemaVersion,
            1,
            123,
            gridSize,
            gridSize,
            cells,
            new GridPosition(0, 0),
            new GridPosition(23, 23),
            playerPlacedWalls
        );
    }
}
