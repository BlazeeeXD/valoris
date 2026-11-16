using NUnit.Framework;
using UnityEngine;

public class PlacementRulesTests
{
    private GeneratedLevelData _level;

    [SetUp]
    public void SetUp()
    {
        _level = new GeneratedLevelData(24, 24, 1, Vector2Int.zero, new Vector2Int(23, 23));
        // Simulate an immutable base wall generated before player placement.
        _level.SetCell(5, 5, 1);
    }

    [Test]
    public void StartGoalAndExistingWallsCannotBeOverwritten()
    {
        Assert.That(PlacementRules.PlaceItem(_level, Vector2Int.zero, 2), Is.False);
        Assert.That(PlacementRules.PlaceItem(_level, new Vector2Int(23, 23), 2), Is.False);
        Assert.That(PlacementRules.PlaceItem(_level, new Vector2Int(5, 5), 3), Is.False);
    }

    [Test]
    public void PlayerPlacedItemCanBeRemovedButBaseWallCannot()
    {
        var position = new Vector2Int(2, 3);
        Assert.That(PlacementRules.PlaceItem(_level, position, 2), Is.True);
        Assert.That(PlacementRules.RemoveItem(_level, position), Is.True);
        Assert.That(_level.GetCell(2, 3), Is.EqualTo(0));
        Assert.That(PlacementRules.RemoveItem(_level, new Vector2Int(5, 5)), Is.False);
        Assert.That(_level.GetCell(5, 5), Is.EqualTo(1));
    }

    [Test]
    public void UnknownCellTypeCannotBePlaced()
    {
        Assert.That(PlacementRules.PlaceItem(_level, new Vector2Int(3, 3), 4), Is.False);
    }
}
