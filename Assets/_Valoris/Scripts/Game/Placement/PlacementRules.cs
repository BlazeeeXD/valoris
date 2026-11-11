
using UnityEngine;

public static class PlacementRules
{
    private const byte Empty = 0;
    private const byte Wall = 1;
    private const byte Trap = 2;
    private const byte Mud = 3;

    public static bool CanPlaceItem(
        GeneratedLevelData level,
        Vector2Int position,
        byte itemType)
    {
        if (level == null || level.cells == null ||
            level.playerPlacedItems == null)
            return false;

        if (!IsInsideGrid(level, position))
            return false;

        if (position == level.start || position == level.goal)
            return false;

        if (itemType < Wall || itemType > Mud)
            return false;

        return level.GetCell(position.x, position.y) == Empty;
    }

    public static bool PlaceItem(
        GeneratedLevelData level,
        Vector2Int position,
        byte itemType)
    {
        if (!CanPlaceItem(level, position, itemType))
            return false;

        level.SetCell(position.x, position.y, itemType);
        level.MarkPlayerPlaced(position.x, position.y);

        return true;
    }

    public static bool RemoveItem(
        GeneratedLevelData level,
        Vector2Int position)
    {
        if (level == null || level.cells == null ||
            level.playerPlacedItems == null)
            return false;

        if (!IsInsideGrid(level, position))
            return false;

        if (!level.IsPlayerPlaced(position.x, position.y))
            return false;

        level.SetCell(position.x, position.y, Empty);
        level.ClearPlayerPlaced(position.x, position.y);

        return true;
    }

    private static bool IsInsideGrid(
        GeneratedLevelData level,
        Vector2Int position)
    {
        return position.x >= 0 &&
               position.x < level.width &&
               position.y >= 0 &&
               position.y < level.height;
    }
}
