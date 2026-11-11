
using UnityEngine;

[System.Serializable]
public class CampaignState
{
    public bool boardLocked { get; private set; }

    public bool IsBoardLocked => boardLocked;

    public bool LockBoard()
    {
        if (boardLocked)
            return false;

        boardLocked = true;

        Debug.Log("Board locked. Sabotage placement is now disabled.");

        return true;
    }

    public void ResetBoard()
    {
        boardLocked = false;
        Debug.Log("Board unlocked.");
    }
}
