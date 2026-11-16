using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>Creates a small runtime HUD so the supplied scene needs no hand-wired text references.</summary>
public class CampaignHUDController : MonoBehaviour
{
    private TMP_Text _text;
    private string _status = "Place sabotage: 1=Wall, 2=Trap, 3=Mud | Left-click place | Right-click remove | Lock Board when ready.";
    private string _inventory = "";
    private CampaignManager _campaign;

    public void Initialize(CampaignManager campaign, InventoryManager inventory)
    {
        _campaign = campaign;
        EnsureText();
        Refresh(campaign.State, inventory);
    }

    public void Refresh(CampaignState state, InventoryManager inventory)
    {
        EnsureText();
        if (_text == null || state == null) return;
        _text.text = "LEVEL " + state.CurrentLevel + "/" + state.MaxLevels +
            "     PLAYER " + state.PlayerScore + " : " + state.AiScore + " AI\n" +
            "Completed: " + state.CompletedLevels + "     " + _inventory + "\n\n" + _status;
    }

    public void SetStatus(string status)
    {
        _status = status ?? "";
        if (_campaign != null) Refresh(_campaign.State, FindObjectOfType<InventoryManager>());
        else if (_text != null) _text.text = _status;
    }

    public void SetInventory(string inventory)
    {
        _inventory = inventory ?? "";
        if (_campaign != null) Refresh(_campaign.State, FindObjectOfType<InventoryManager>());
    }

    public string GetInventoryHelp()
        => "Keys 1/2/3 select Wall/Trap/Mud. Left-click places; right-click removes.";

    private void EnsureText()
    {
        if (_text != null) return;
        Canvas canvas = FindObjectOfType<Canvas>();
        if (canvas == null)
        {
            var canvasObject = new GameObject("Oralis HUD Canvas");
            canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvasObject.AddComponent<CanvasScaler>();
            canvasObject.AddComponent<GraphicRaycaster>();
        }

        var panel = new GameObject("Oralis Campaign HUD");
        panel.transform.SetParent(canvas.transform, false);
        var panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0f, 1f);
        panelRect.anchorMax = new Vector2(0f, 1f);
        panelRect.pivot = new Vector2(0f, 1f);
        panelRect.anchoredPosition = new Vector2(18f, -18f);
        panelRect.sizeDelta = new Vector2(620f, 126f);
        Image background = panel.AddComponent<Image>();
        background.color = new Color(0.025f, 0.04f, 0.07f, 0.82f);
        background.raycastTarget = true;

        var textObject = new GameObject("HUD Text");
        textObject.transform.SetParent(panel.transform, false);
        var rect = textObject.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(14f, 10f);
        rect.offsetMax = new Vector2(-14f, -10f);
        _text = textObject.AddComponent<TextMeshProUGUI>();
        if (TMP_Settings.defaultFontAsset != null) _text.font = TMP_Settings.defaultFontAsset;
        _text.fontSize = 17f;
        _text.color = Color.white;
        _text.alignment = TextAlignmentOptions.TopLeft;
        _text.enableWordWrapping = true;
        _text.raycastTarget = false;
    }
}
