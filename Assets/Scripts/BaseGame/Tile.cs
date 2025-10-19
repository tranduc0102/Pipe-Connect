using DG.Tweening;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Experimental.GlobalIllumination;

public class Tile : MonoBehaviour
{
    public Vector2Int GridPos { get; private set; }

    private bool _isOK;
    private bool _isObstacle;
    private bool _isDot;
    private Color _color;

    private LineController currentLine;
    [SerializeField] private MeshRenderer _render;

    public bool IsOK => _isOK;
    public bool IsObstacle => _isObstacle;
    public bool IsDot => _isDot;
    public Color TileColor => _color;

    public LineController CurrentLine
    {
        get => currentLine;
        set => currentLine = value;
    }

    public void Init(int x, int y)
    {
        GridPos = new Vector2Int(x, y);
    }

    public void IsNode(int x, int y, bool isNode, Color color, bool isObstacle = false)
    {
        Init(x, y);
        _isDot = isNode;
        _isObstacle = isObstacle;

        if (isNode)
            SetColor(color);
        else
            _render.material.color = Color.white;
    }

    public void SetColor(Color color)
    {
        _color = color;
        if (_render != null)
        {
            _render.material.DOKill();
             Color tileColor = new Color(color.r + 50f / 255f, color.g + 50f / 255f, color.b + 50f / 255f);
            _render.material.DOColor(tileColor, 0.2f).SetEase(Ease.OutQuad);
        }
    }

    public Color GetTileColor() => _color;

    public void SetConnect(Tile tile)
    {
        if (tile.IsDot && tile._color == _color)
            _isOK = true;
    }

    public void IsOk(bool isOK) => _isOK = isOK;

    public void ResetTile()
    {
        _isOK = false;
        _isObstacle = false;
        _isDot = false;
        currentLine = null;
        SetColor(new Color32(0xA7, 0x9A, 0xCD, 0xFF));
    }
    public void ResetColorTile()
    {
        _isOK = false;
        currentLine = null;
        if (_isDot) return;
        SetColor(GameManager.Instance.DEFAULT_COLOR);
    }


    public void MarkAsDot(Color color)
    {
        _isDot = true;
        _isObstacle = false;
        _color = color;
        SetColor(color);
    }

    public void MarkAsObstacle()
    {
        _isObstacle = true;
        _isDot = false;
        SetColor(Color.gray);
    }
    public TileData ToTileData()
    {
        return new TileData
        {
            x = GridPos.x,
            y = GridPos.y,
            isDot = _isDot,
            isObstacle = _isObstacle,
            color = _color
        };
    }

    public void FromTileData(TileData data)
    {
        Init(data.x, data.y);

        if (data.isDot)
            MarkAsDot(data.color);
        else if (data.isObstacle)
            MarkAsObstacle();
        else
            ResetTile();
    }
}
