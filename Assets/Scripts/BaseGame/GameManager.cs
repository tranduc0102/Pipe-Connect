using DG.Tweening;
using OneLine;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [SerializeField] private List<Tile> allTiles = new List<Tile>();

    [Header("References")]
    public MapGenerator generator;

    [Header("Prefabs")]
    public GameObject dotPrefab;
    public GameObject obstaclePrefab;
    public readonly Color DEFAULT_COLOR = new Color32(0xA7, 0x9A, 0xCD, 0xFF);
    [SerializeField] private int _currrentLevel;
    public int CurrentLevel => _currrentLevel;
    public Camera cam;
    public TextMeshProUGUI _txtLevel;
    private void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        LoadLevel(_currrentLevel);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            NextLevel();
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            NextLevel1();
        }
    }
    public void NextLevel1()
    {
        _currrentLevel--;
        allTiles.Clear();
        ClearExisting();
        LoadLevel(_currrentLevel);
    }
    public void NextLevel()
    {
        _currrentLevel++;
        allTiles.Clear();
        ClearExisting();
        LoadLevel(_currrentLevel);
    }
    public void RegisterTile(Tile tile)
    {
        if (!allTiles.Contains(tile))
            allTiles.Add(tile);
    }

    public void CheckWin()
    {
        foreach (var tile in allTiles)
        {
            if (!tile.IsOK)
                return;
        }
        Debug.Log("WIN");
        DOVirtual.DelayedCall(0.5f, NextLevel);
    }
    void LoadLevel(int level)
    {
        string path = $"Levels/Level_{level}";
        TextAsset jsonFile = Resources.Load<TextAsset>(path);

        if (jsonFile == null)
        {
            level = Random.Range(10, 21);
            path = $"Levels/Level_{level}";
            jsonFile = Resources.Load<TextAsset>(path);
        }
        _txtLevel.transform.DOScale(Vector3.zero, 0.15f)
                      .SetEase(Ease.OutBack).OnComplete(delegate
                      {
                          _txtLevel.text = "Level " + CurrentLevel;
                          _txtLevel.transform.DOScale(Vector3.one, 0.25f)
                  .SetEase(Ease.InBack);
                      });

        LevelData data = JsonUtility.FromJson<LevelData>(jsonFile.text);
        generator.width = data.width;
        generator.height = data.height;
        cam.fieldOfView = data.camSize;
        generator.GenerateTiles();

        foreach (var tileData in data.tiles)
        {
            Tile tile = generator.tiles.Find(t => t.GridPos.x == tileData.x && t.GridPos.y == tileData.y);
            if (tile == null) continue;

            tile.FromTileData(tileData);
            if (!tileData.isObstacle)
            {
                RegisterTile(tile);
            }
            if (tileData.isDot)
                SpawnVisual(tile, dotPrefab, tileData.color);

        }
    }
    private void SpawnVisual(Tile tile, GameObject prefab, Color color)
    {
        if (prefab == null) return;

        Vector3 spawnPos = tile.transform.position + Vector3.down * 0.5f;

        GameObject obj = Instantiate(prefab, spawnPos, Quaternion.identity, tile.transform);
        obj.name = $"{tile.GridPos.x}_{tile.GridPos.y}_{prefab.name}";
        obj.transform.localPosition = new Vector3(0f, 0.5f, 0.5f);
        Renderer rend = obj.GetComponent<Renderer>();
        if (rend) rend.material.color = color;

        if (prefab.name.ToLower().Contains("dot"))
            obj.tag = "Dot";
        else if (prefab.name.ToLower().Contains("obstacle"))
            obj.tag = "Obstacle";
    }
    private void ClearExisting()
    {
        LineController[] allLines = FindObjectsOfType<LineController>();
        foreach (var line in allLines)
        {
            line.ClearLine();
        }
    }
}
