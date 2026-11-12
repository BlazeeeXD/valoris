using System;
using Valoris.Core.Contracts;

/// <summary>Converts the mutable Unity board to an immutable AI contract snapshot.</summary>
public static class LevelSnapshotFactory
{
    public static LevelDefinition Create(GeneratedLevelData source)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (source.width != LevelDefinition.CampaignWidth || source.height != LevelDefinition.CampaignHeight)
            throw new ArgumentException("The first release requires a 24 x 24 board.", nameof(source));
        if (source.cells == null || source.cells.Length != source.width * source.height)
            throw new ArgumentException("Cell array length does not match width x height.", nameof(source));

        var cells = new int[source.cells.Length];
        for (int i = 0; i < source.cells.Length; i++)
        {
            byte value = source.cells[i];
            if (value > (byte)CellType.Mud)
                throw new ArgumentException("Unknown cell type " + value + " at row-major index " + i + ".", nameof(source));
            cells[i] = value;
        }

        return new LevelDefinition(
            source.schemaVersion,
            source.levelNumber,
            source.seed,
            source.width,
            source.height,
            cells,
            new GridPosition(source.start.x, source.start.y),
            new GridPosition(source.goal.x, source.goal.y));
    }
}
