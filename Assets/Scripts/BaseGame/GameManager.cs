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
    [SerializeField] private Camera _cam;
    public Camera Camera => _cam;
    [Header("Prefabs")]
    public GameObject dotPrefab;
    public GameObject obstaclePrefab;
    public readonly Color DEFAULT_COLOR = new Color32(0xA7, 0x9A, 0xCD, 0xFF);
    private float time;
    public int CurrentLevel
    {
        get => PlayerPrefs.GetInt("Current Level", 1);
        set => PlayerPrefs.SetInt("Current Level", value);
    }
    private GameState _state;
    public GameState State
    {
        get => _state;
        set => _state = value;
    }
    private void Awake()
    {
        Instance = this;
        Application.targetFrameRate = 60;
    }
    private void Start()
    {
        LoadLevel(CurrentLevel);
    }
    private void Update()
    {
#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.C))
        {
            NextLevel();
        }
        if (Input.GetKeyDown(KeyCode.Z))
        {
            Replay();
        }
        if (Input.GetKeyDown(KeyCode.X))
        {
            CurrentLevel = 1;
            Replay();
        }
#endif
        if (_state == GameState.Playing)
        {
            if (time > 0)
            {
                time -= Time.deltaTime;
            }
            if (time < 0)
            {
                if (_state == GameState.Win) return;
                _state = GameState.Lose;
                DOVirtual.DelayedCall(0.25f, () => UIManager.Instance.ShowResult(false, true));
            }
            UIManager.Instance.UpdateTime(Mathf.Max(0f, time));
        }
    }
    public void Replay()
    {
        allTiles.Clear();
        ClearExisting();
        LoadLevel(CurrentLevel);
    }
    public void NextLevel()
    {
        CurrentLevel++;
        allTiles.Clear();
        ClearExisting();
        LoadLevel(CurrentLevel);
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
        _state = GameState.Win;
        DOVirtual.DelayedCall(0.25f, () => UIManager.Instance.ShowResult(true, true));
    }
    void LoadLevel(int level)
    {
        string path = $"Levels/Level_{level}";
        TextAsset jsonFile = Resources.Load<TextAsset>(path);
        _state = GameState.Waiting;
        if (jsonFile == null)
        {
            level = Random.Range(10, 21);
            path = $"Levels/Level_{level}";
            jsonFile = Resources.Load<TextAsset>(path);
        }

        LevelData data = JsonUtility.FromJson<LevelData>(jsonFile.text);
        generator.width = data.width;
        generator.height = data.height;
        float targetAspect = 10.8f / 19.2f;
        float currentAspect = (float)Screen.width / Screen.height;

        float verticalFOV = (data.camSize - 1.5f) * (targetAspect / currentAspect);
        Camera.main.fieldOfView = verticalFOV;

        generator.GenerateTiles();
        time = 120f;
        UIManager.Instance.UpdateViewLevel(level, time);
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
    public enum GameState
    {
        Waiting,
        Playing,
        Win,
        Lose
    }
}
