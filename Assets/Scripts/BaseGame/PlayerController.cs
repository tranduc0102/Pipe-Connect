using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Camera _camera;
    [SerializeField] private LineController _linePrefab;
    public GameObject touchHover;
    public Image touchHoverImage;

    private LineController _currentLine;
    private Tile _startTile;
    private Tile _lastTile;

    private void Update()
    {
        TouchHoverPosition(Input.mousePosition);
        if (IsPointerOverUIElement() || 
            GameManager.Instance.State == GameManager.GameState.Win ||
            GameManager.Instance.State == GameManager.GameState.Lose)
        {
            touchHover.SetActive(false);
            return;
        }
        if (Input.GetMouseButtonDown(0))
            OnPress();
        else if (Input.GetMouseButton(0))
            OnDrag();
        else if (Input.GetMouseButtonUp(0))
            OnUp();
    }

    private void OnPress()
    {
        Tile tile = GetTileUnderMouse();
        if (tile == null || tile.IsObstacle) return;
        if (tile.CurrentLine != null)
        {
            LineController line = tile.CurrentLine;
            TouchSetColor(tile.TileColor);
            ActiveTouch(true);
            if (tile.IsDot && (tile == line.Tiles[0] || tile == line.Tiles[line.Tiles.Count - 1]))
            {
                line.ClearLine();
                _currentLine = Instantiate(_linePrefab);
                _currentLine.SetColor(tile.GetTileColor());
                _currentLine.AddTile(tile);
                _startTile = tile;
                _lastTile = tile;
                return;
            }

            _currentLine = line;
            _currentLine.CutTo(tile);
            _lastTile = tile;
            _startTile = _currentLine.Tiles[0];
            return;
        }
        if (GameManager.Instance.State == GameManager.GameState.Waiting)
        {
            GameManager.Instance.State = GameManager.GameState.Playing;
        }
        if (tile.IsDot && !tile.CurrentLine)
        {
            ClearExistingLineOfSameColor(tile.GetTileColor());

            _currentLine = Instantiate(_linePrefab);
            _currentLine.SetColor(tile.GetTileColor());
            _currentLine.AddTile(tile);

            _startTile = tile;
            _lastTile = tile;
            TouchSetColor(tile.TileColor);
            ActiveTouch(true);
        }
    }

    private void OnDrag()
    {
        if (_currentLine == null) return;
        Tile tile = GetTileUnderMouse();
        if (tile == null || tile == _lastTile || tile.IsObstacle) return;
        if (tile.CurrentLine != _currentLine && _currentLine.IsCompleted)
        {
            return;
        }
        if (tile.IsDot && tile.GetTileColor() != _startTile.GetTileColor()) return;

        Vector2Int lastPos = _lastTile.GridPos;
        Vector2Int newPos = tile.GridPos;
        int dist = Mathf.Abs(newPos.x - lastPos.x) + Mathf.Abs(newPos.y - lastPos.y);
        if (dist != 1) return;

        _currentLine.AddTile(tile);
        _lastTile = tile;
        tile.SetColor(_currentLine.LineColor);
    }

    private void OnUp()
    {
        if (_currentLine != null)
        {
            if (_currentLine.GetCountInLine <= 1)
            {
                _startTile.CurrentLine = null;
                Destroy(_currentLine.gameObject);
            }
            ActiveTouch(false);
            _currentLine = null;
            _startTile = null;
            _lastTile = null;
        }
    }

    private Tile GetTileUnderMouse()
    {
        Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
            return hit.collider.GetComponent<Tile>();
        return null;
    }

    private void ClearExistingLineOfSameColor(Color color)
    {
        LineController[] allLines = FindObjectsOfType<LineController>();
        foreach (var line in allLines)
        {
            if (line != null && line.LineColor == color)
            {
                line.ClearLine();
            }
        }
    }
    public void TouchSetColor(Color color)
    {
        color.a = 0.5f;
        touchHoverImage.color = color;
    }

    public void ActiveTouch(bool isActive)
    {
        touchHover.SetActive(isActive);
    }

    public void TouchHoverPosition(Vector3 position)
    {
        touchHover.transform.position = position;
    }
    private bool IsPointerOverUIElement()
    {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, results);

        foreach (RaycastResult unused in results)
        {
            if (unused.gameObject.layer == LayerMask.NameToLayer("UI"))
            {
                return true;
            }
        }

        return false;
    }

}
