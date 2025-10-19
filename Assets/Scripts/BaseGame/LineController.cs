using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

public class LineController : MonoBehaviour
{
    [SerializeField] private LineRenderer _lineRenderer;
    [HideInInspector] public List<Tile> Tiles = new List<Tile>();
    private Color _lineColor;
    public bool IsCompleted { get; private set; } = false;
    public Color LineColor => _lineColor;
    public void SetColor(Color color)
    {
        _lineColor = color;
        if (_lineRenderer != null)
        {
            _lineRenderer.startColor = color;
            _lineRenderer.endColor = color;
            if (_lineRenderer.material != null) _lineRenderer.material.color = color;
        }
    }

    public void AddTile(Tile tile)
    {
        if (tile == null)
        {
            return;
        }

        if (Tiles.Contains(tile))
        {
            CutTo(tile);
            return;
        }
        if (tile.CurrentLine != null && tile.CurrentLine != this)
        {
            LineController otherLine = tile.CurrentLine;
            int intersectIndex = otherLine.Tiles.IndexOf(tile);

            if (intersectIndex > 0)
                otherLine.CutTo(otherLine.Tiles[intersectIndex - 1]);
            else
                otherLine.ClearLine();

            if (otherLine.GetCountInLine <= 1)
                Destroy(otherLine.gameObject);
        }

        tile.CurrentLine = this;
        Tiles.Add(tile);
        tile.IsOk(true);

        Vector3 pos = tile.transform.position;
        pos.z = 0.2f;

        if (_lineRenderer != null)
        {
            _lineRenderer.positionCount = Tiles.Count;
            _lineRenderer.SetPosition(Tiles.Count - 1, pos);
        }

        tile.transform.DOKill();
        tile.transform.DOScale(1.12f, 0.08f).OnComplete(() => tile.transform.DOScale(1f, 0.08f));

        if (Tiles.Count > 1 && Tiles[0].GetTileColor() == tile.GetTileColor() && tile.IsDot)
        {
            IsCompleted = true;
            AudioManager.Instance.PlayDone();
            GameManager.Instance.CheckWin();
        }
        else
        {
            AudioManager.Instance.PlayUp();
        }
    }

    public void CutTo(Tile tile)
    {
        int index = Tiles.IndexOf(tile);
        if (index < 0) return;

        for (int i = Tiles.Count - 1; i > index; i--)
        {
            Tiles[i].ResetColorTile();
            Tiles.RemoveAt(i);
        }
        IsCompleted = false;
        AudioManager.Instance.PlayDown();
        UpdateLinePositions();
    }
    public void ClearLine()
    {
        foreach (var t in Tiles)
        {
            if (t != null)
            {
                t.CurrentLine = null;
                t.IsOk(false);
                t.ResetColorTile();
            }
        }

        Tiles.Clear();
        if (_lineRenderer != null) _lineRenderer.positionCount = 0;
        IsCompleted = false;

        Destroy(gameObject);
    }

    private void UpdateLinePositions()
    {
        if (_lineRenderer != null)
        {
            _lineRenderer.positionCount = Tiles.Count;
            for (int i = 0; i < Tiles.Count; i++)
            {
                Vector3 pos = Tiles[i].transform.position;
                pos.z = 0.2f;
                _lineRenderer.SetPosition(i, pos);
            }
        }
    }

    public int GetCountInLine => _lineRenderer.positionCount;
}
