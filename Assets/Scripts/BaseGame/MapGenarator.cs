using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;

namespace OneLine
{
    public class MapGenerator : MonoBehaviour
    {
        [Header("Grid Settings")]
        public int width = 5;
        public int height = 5;
        public float spacing = 2f;
        public Tile tilePrefab;
        public Camera cam;

        [Header("FX Settings")]
        public float spawnDelay = 0.03f;    
        public float spawnScaleTime = 0.25f; 
        public float clearScaleTime = 0.15f;  
        public List<Tile> tiles = new List<Tile>();

        public void GenerateTiles()
        {
            transform.position = new Vector3(0f, 0f, 0.5f);
            transform.rotation = Quaternion.Euler(90f, 90f, -90f);

            ClearGrid(true);

            tiles.Clear();

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    Vector3 pos = new Vector3(x * spacing, 0f, y * spacing);
                    var tile = Instantiate(tilePrefab, pos, Quaternion.identity, transform);
                    tile.Init(x, y);
                    tile.transform.localScale = Vector3.zero;

                    tile.transform.DOScale(Vector3.one, spawnScaleTime)
                        .SetEase(Ease.OutBack)
                        .OnComplete(() => AudioManager.Instance.PlaySpawn())
                        .SetDelay((x * height + y) * spawnDelay);
                    tiles.Add(tile);
                }
            }
            transform.rotation = Quaternion.Euler(0f, 0f, 0f);
            transform.position = new Vector3(width - 1, height - 1, 0.5f);
        }

        public void ClearGrid(bool withFX = false)
        {
            if (!withFX)
            {
                for (int i = 0; i < tiles.Count; i++)
                    DestroyImmediate(tiles[i].gameObject);
                tiles.Clear();
                return;
            }

            foreach (var t in tiles)
            {
                if (t == null) continue;

                t.transform.DOScale(Vector3.zero, clearScaleTime)
                    .SetEase(Ease.InBack)
                    .OnComplete(() =>
                    {
#if UNITY_EDITOR
                        DestroyImmediate(t.gameObject);
#else
                        Destroy(t.gameObject);
#endif
                    });
            }

            tiles.Clear();
        }
    }
}
