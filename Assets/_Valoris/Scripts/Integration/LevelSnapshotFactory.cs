
using UnityEngine;

public static class LevelSnapshotFactory
{
    public static GeneratedLevelData CreateSnapshot(
        GeneratedLevelData source)
    {
        if (source == null)
        {
            Debug.LogError("LevelSnapshotFactory: Source level is null!");
            return null;
        }

        if (source.cells == null ||
            source.cells.Length != source.width * source.height)
        {
            Debug.LogError("LevelSnapshotFactory: Invalid cell data!");
            return null;
        }

        GeneratedLevelData snapshot = new GeneratedLevelData(
            source.width,
            source.height,
            source.seed,
            source.start,
            source.goal
        );

        snapshot.schemaVersion = source.schemaVersion;
        snapshot.levelNumber = source.levelNumber;

        snapshot.cells = (byte[])source.cells.Clone();

        snapshot.playerPlacedItems =
            source.playerPlacedItems != null
            ? (bool[])source.playerPlacedItems.Clone()
            : null;

        return snapshot;
    }
}
