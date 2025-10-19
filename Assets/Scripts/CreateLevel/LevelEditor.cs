using DG.Tweening;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace OneLine
{
    public class LevelEditor : MonoBehaviour
    {
        [Header("References")]
        public MapGenerator generator;
        public TMP_InputField widthInput;
        public TMP_InputField heightInput;
        public TMP_InputField levelInput;
        public TMP_InputField sizeCameInput;
        public Button spawnButton;
        public Button saveButton;
        public Button loadButton;
        public Toggle isDotToggle;
        public Toggle isObstacleToggle;
        public TMP_Dropdown colorDropdown;
        public List<Color> colorList;

        [Header("Prefabs")]
        public GameObject dotPrefab;
        public GameObject obstaclePrefab;

        private Tile _selectedTile;

        private void Start()
        {
            spawnButton.onClick.AddListener(OnSpawnClicked);
            saveButton.onClick.AddListener(OnSaveClicked);
            loadButton.onClick.AddListener(OnLoadClicked);
            SetupColorDropdown();
        }

        private void SetupColorDropdown()
        {
            if (colorDropdown == null || colorList == null) return;
            colorDropdown.ClearOptions();
            List<string> options = new List<string>();
            for (int i = 0; i < colorList.Count; i++)
                options.Add($"Color {i + 1}");
            colorDropdown.AddOptions(options);
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit))
                {
                    Tile tile = hit.collider.GetComponent<Tile>();
                    if (tile != null)
                    {
                        _selectedTile = tile;
                        OnTileClicked(tile);
                    }
                }
            }

            for (int i = 0; i < 9; i++)
            {
                KeyCode key = KeyCode.Alpha1 + i; 
                if (Input.GetKeyDown(key))
                {
                    if (i < colorList.Count)
                    {
                        colorDropdown.value = i; 
                        colorDropdown.RefreshShownValue();

                        Debug.Log($"🎨 Đã chọn màu {i + 1}");
                    }
                }
            }
        }

        void OnSpawnClicked()
        {
            if (!int.TryParse(widthInput.text, out int w)) w = 5;
            if (!int.TryParse(heightInput.text, out int h)) h = 5;
            if (!float.TryParse(sizeCameInput.text, out float s)) s = 20.0f;

            ClearAllEditorVisuals();

            if (generator != null)
                generator.ClearGrid();

            generator.width = w;
            generator.height = h;
            generator.cam.fieldOfView = s;
            generator.GenerateTiles();

            foreach (var tile in generator.tiles)
            {
                if (tile.GetComponent<Collider>() == null)
                    tile.gameObject.AddComponent<BoxCollider>();
            }

            Debug.Log($"Spawned grid {w}x{h}");
        }

        private void ClearAllEditorVisuals()
        {
           /* if (generator != null)
            {
                foreach (Transform child in generator.transform)
                {
                    DestroyImmediate(child.gameObject);
                }
                generator.tiles.Clear();
            }
            var dots = GameObject.FindGameObjectsWithTag("Dot");
            foreach (var go in dots) DestroyImmediate(go);

            var obs = GameObject.FindGameObjectsWithTag("Obstacle");
            foreach (var go in obs) DestroyImmediate(go);*/
        }

        void OnTileClicked(Tile tile)
        {
            if (tile == null) return;

            if (isDotToggle.isOn && tile.IsDot)
            {
                RemoveExistingVisual(tile);
                tile.ResetTile();
                tile.SetColor(new Color32(0xA7, 0x9A, 0xCD, 0xFF));
                return;
            }

            if (isObstacleToggle.isOn && tile.IsObstacle)
            {
                RemoveExistingVisual(tile);
                tile.ResetTile();
                tile.SetColor(new Color32(0xA7, 0x9A, 0xCD, 0xFF));
                return;
            }

            RemoveExistingVisual(tile);

            if (isObstacleToggle.isOn)
            {
                tile.MarkAsObstacle();
                SpawnVisual(tile, obstaclePrefab, Color.gray);
            }
            else if (isDotToggle.isOn)
            {
                int colorIndex = Mathf.Clamp(colorDropdown.value, 0, colorList.Count - 1);
                Color c = colorList[colorIndex];
                tile.MarkAsDot(c);
                SpawnVisual(tile, dotPrefab, c);
            }
            else
            {
                tile.ResetTile();
                tile.SetColor(new Color32(0xA7, 0x9A, 0xCD, 0xFF));
            }
        }

        void OnSaveClicked()
        {
            string levelName = levelInput.text.Trim();
            foreach (char c in Path.GetInvalidFileNameChars())
                levelName = levelName.Replace(c.ToString(), "_");

            if (string.IsNullOrEmpty(levelName))
            {
                Debug.LogWarning("⚠️ Please enter a valid level name!");
                return;
            }

            LevelData data = new LevelData
            {
                width = generator.width,
                height = generator.height,
                camSize = generator.cam.fieldOfView,
                tiles = new List<TileData>()
            };

            foreach (var tile in generator.tiles)
                data.tiles.Add(tile.ToTileData());

            string folderPath = Path.Combine(Application.dataPath, "Resources/Levels");
            Directory.CreateDirectory(folderPath);

            string filePath = Path.Combine(folderPath, $"Level_{levelName}.json");
            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(filePath, json);

#if UNITY_EDITOR
            AssetDatabase.Refresh();
#endif

            Debug.Log($"Saved Level_{levelName} to: {filePath}");
        }

        void OnLoadClicked()
        {
            string levelName = levelInput.text.Trim();
            if (string.IsNullOrEmpty(levelName))
            {
                Debug.LogWarning("enter Level");
                return;
            }

            string path = $"Levels/Level_{levelName}";
            TextAsset jsonFile = Resources.Load<TextAsset>(path);

            if (jsonFile == null)
            {
                Debug.LogError($"Level_{levelName}.json not found in Resources!");
                return;
            }

            LevelData data = JsonUtility.FromJson<LevelData>(jsonFile.text);
            generator.width = data.width;
            generator.height = data.height;
            generator.cam.fieldOfView = data.camSize;
            generator.GenerateTiles();

            foreach (var tileData in data.tiles)
            {
                Tile tile = generator.tiles.Find(t => t.GridPos.x == tileData.x && t.GridPos.y == tileData.y);
                if (tile == null) continue;

                tile.FromTileData(tileData);

                if (tileData.isDot)
                    SpawnVisual(tile, dotPrefab, tileData.color);
                else if (tileData.isObstacle)
                    SpawnVisual(tile, obstaclePrefab, Color.gray);
            }

            Debug.Log($"📂 Loaded Level {levelName}");
        }

        private void SpawnVisual(Tile tile, GameObject prefab, Color color)
        {
            if (prefab == null) return;

            Vector3 spawnPos = tile.transform.position + Vector3.down * 0.5f;

            GameObject obj = Instantiate(prefab, spawnPos, Quaternion.identity, tile.transform);
            obj.name = $"{tile.GridPos.x}_{tile.GridPos.y}_{prefab.name}";

            Renderer rend = obj.GetComponent<Renderer>();
            if (rend) rend.material.color = color;

            if (prefab.name.ToLower().Contains("dot"))
                obj.tag = "Dot";
            else if (prefab.name.ToLower().Contains("obstacle"))
                obj.tag = "Obstacle";
        }

        private void RemoveExistingVisual(Tile tile)
        {
            foreach (Transform child in tile.transform)
            {
                if (child.CompareTag("Dot") || child.CompareTag("Obstacle"))
                    DestroyImmediate(child.gameObject);
            }
        }
    }
}
