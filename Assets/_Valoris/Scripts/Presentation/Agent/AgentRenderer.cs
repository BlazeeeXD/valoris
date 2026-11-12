using UnityEngine;
using Valoris.Core.Contracts;

/// <summary>Draws a lightweight agent marker above the tilemap without modifying board cells.</summary>
public class AgentRenderer : MonoBehaviour
{
    [SerializeField] private Sprite agentSprite;
    [SerializeField] private Color markerColor = new Color(0.15f, 0.9f, 1f, 1f);
    [SerializeField] private float markerScale = 0.72f;

    private GridManager _grid;
    private GameObject _markerObject;
    private SpriteRenderer _renderer;
    private Texture2D _generatedTexture;
    private Sprite _generatedSprite;

    public AgentVisualState State { get; private set; } = AgentVisualState.Hidden;

    public void Initialize(GridManager grid)
    {
        _grid = grid;
        if (_markerObject == null)
        {
            _markerObject = new GameObject("AI Agent Marker");
            _markerObject.transform.SetParent(transform, false);
            _renderer = _markerObject.AddComponent<SpriteRenderer>();
            _renderer.sortingOrder = 100;
        }
        if (agentSprite == null)
        {
            _generatedTexture = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            _generatedTexture.filterMode = FilterMode.Point;
            for (int y = 0; y < 16; y++)
            for (int x = 0; x < 16; x++)
            {
                float dx = x - 7.5f;
                float dy = y - 7.5f;
                _generatedTexture.SetPixel(x, y, dx * dx + dy * dy <= 50f ? Color.white : Color.clear);
            }
            _generatedTexture.Apply();
            _generatedSprite = Sprite.Create(_generatedTexture, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16f);
            agentSprite = _generatedSprite;
        }
        _renderer.sprite = agentSprite;
        _renderer.color = markerColor;
        _markerObject.transform.localScale = Vector3.one * markerScale;
        Hide();
    }

    public void ShowAt(GridPosition position, AgentVisualState state = AgentVisualState.Moving)
    {
        if (_grid == null || _renderer == null) return;
        _markerObject.transform.position = _grid.GetCellCenterWorld(new Vector2Int(position.X, position.Y));
        _renderer.color = state == AgentVisualState.Success
            ? new Color(0.25f, 1f, 0.35f, 1f)
            : state == AgentVisualState.Failure
                ? new Color(1f, 0.22f, 0.2f, 1f)
                : markerColor;
        _markerObject.SetActive(true);
        State = state;
    }

    public void Hide()
    {
        if (_markerObject != null) _markerObject.SetActive(false);
        State = AgentVisualState.Hidden;
    }

    private void OnDestroy()
    {
        if (_generatedSprite != null) Destroy(_generatedSprite);
        if (_generatedTexture != null) Destroy(_generatedTexture);
        if (_markerObject != null) Destroy(_markerObject);
    }
}
