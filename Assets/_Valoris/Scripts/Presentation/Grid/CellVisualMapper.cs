using Valoris.Core.Contracts;
using UnityEngine;
using UnityEngine.Tilemaps;

public static class CellVisualMapper
{
    public static TileBase GetTile(CellType type, GridManager grid)
    {
        if (grid == null) return null;
        switch (type)
        {
            case CellType.Empty: return grid.floorTile;
            case CellType.Wall: return grid.wallTile;
            case CellType.Trap: return grid.trapTile;
            case CellType.Mud: return grid.mudTile;
            default: return null;
        }
    }

    public static string GetLabel(CellType type)
    {
        switch (type)
        {
            case CellType.Empty: return "Empty";
            case CellType.Wall: return "Wall";
            case CellType.Trap: return "Trap";
            case CellType.Mud: return "Mud";
            default: return "Unknown";
        }
    }
}
