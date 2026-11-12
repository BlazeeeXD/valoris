using UnityEngine;

public class PlacementFeedback : MonoBehaviour
{
    public void Accepted(string itemName, Vector2Int position)
        => Debug.Log(itemName + " placed at " + position + ".");

    public void Rejected(string reason)
        => Debug.LogWarning("Placement rejected: " + reason);
}
