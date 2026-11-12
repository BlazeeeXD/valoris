using UnityEngine;

/// <summary>Small scene-facing facade for menu buttons and external UI hooks.</summary>
public class GameController : MonoBehaviour
{
    private GameFlowController _flow;

    private void Awake()
    {
        _flow = FindObjectOfType<GameFlowController>();
    }

    public void BeginNewCampaign()
    {
        if (_flow == null) _flow = FindObjectOfType<GameFlowController>();
        if (_flow == null) Debug.LogError("GameController: GameFlowController was not found.");
        else _flow.StartNewCampaign();
    }

    public void StartNextLevel()
    {
        if (_flow == null) _flow = FindObjectOfType<GameFlowController>();
        if (_flow != null) _flow.StartNextLevel();
    }
}
