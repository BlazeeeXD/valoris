using NUnit.Framework;
using UnityEngine;
using Valoris.Core.Contracts;
using Valoris.Core.Grid;

public class UnityLevelContractTests
{
    [Test]
    public void SnapshotUsesRowMajorCellsAndIsImmutableFromUnitySource()
    {
        var source = new GeneratedLevelData(24, 24, 9, Vector2Int.zero, new Vector2Int(23, 23));
        source.levelNumber = 1;
        source.SetCell(1, 0, 1);
        source.SetCell(0, 1, 3);
        source.SetCell(23, 22, 2);

        LevelDefinition snapshot = LevelSnapshotFactory.Create(source);
        source.SetCell(1, 0, 0);
        source.SetCell(0, 1, 0);

        Assert.That(snapshot.GetCellValue(1, 0), Is.EqualTo(1));
        Assert.That(snapshot.GetCellValue(0, 1), Is.EqualTo(3));
        Assert.That(snapshot.GetCellValue(23, 22), Is.EqualTo(2));
        Assert.That(snapshot.CellCount, Is.EqualTo(576));
        Assert.That(new LevelValidator().Validate(snapshot).IsValid, Is.True);
    }

    [Test]
    public void BuilderProducesValidBaseMapAndPreservesEndpoints()
    {
        var walls = new bool[24, 24];
        walls[4, 4] = true;
        GeneratedLevelData level = LevelBuilder.FromWalls(
            walls, 1, 77, Vector2Int.zero, new Vector2Int(23, 23));

        Assert.That(level.GetCell(4, 4), Is.EqualTo(1));
        Assert.That(level.GetCell(0, 0), Is.EqualTo(0));
        Assert.That(level.GetCell(23, 23), Is.EqualTo(0));
        Assert.That(new LevelValidator().Validate(LevelSnapshotFactory.Create(level)).HasSafePath, Is.True);
    }

    [Test]
    public void SameSeedMazeIsDeterministicAndHasSafePath()
    {
        var gameObject = new GameObject("MazeGeneratorTest");
        try
        {
            var generator = gameObject.AddComponent<MazeGenerator>();
            bool[,] first = generator.GenerateMaze(24, 24, 456);
            bool[,] second = generator.GenerateMaze(24, 24, 456);
            Assert.That(generator.HasSafePath(first, Vector2Int.zero, new Vector2Int(23, 23)), Is.True);
            for (int x = 0; x < 24; x++)
            for (int y = 0; y < 24; y++)
                Assert.That(first[x, y], Is.EqualTo(second[x, y]));
        }
        finally { Object.DestroyImmediate(gameObject); }
    }

    [Test]
    public void MazeSupportsNonCornerEndpoints()
    {
        var gameObject = new GameObject("MazeEndpointTest");
        try
        {
            var generator = gameObject.AddComponent<MazeGenerator>();
            var start = new Vector2Int(4, 18);
            var goal = new Vector2Int(20, 3);
            bool[,] walls = generator.GenerateMaze(24, 24, 991, start, goal);
            Assert.That(walls[start.x, start.y], Is.False);
            Assert.That(walls[goal.x, goal.y], Is.False);
            Assert.That(generator.HasSafePath(walls, start, goal), Is.True);
        }
        finally { Object.DestroyImmediate(gameObject); }
    }

}
