using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TileData
{
    public int x;
    public int y;
    public bool isDot;
    public bool isObstacle;
    public Color color;
}

[Serializable]
public class LevelData
{
    public int width;
    public int height;
    public float camSize;
    public List<TileData> tiles = new List<TileData>();
}
