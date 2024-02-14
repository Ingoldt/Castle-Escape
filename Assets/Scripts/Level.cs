using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class Level : MonoBehaviour
{
    private Tilemap tilemap;
    private TilemapRenderer tilemapRenderer;
    private TilemapCollider2D tilemapCollider;

    
    public LevelInfoScriptableObject levelInfo;
    /*
    public LevelInfoScriptableObject GetLevelInfo
    {  get { return levelInfo; } }
    */
    void Awake()
    {
        tilemap = GetComponent<Tilemap>();
        if (tilemap != null)
        {
            SaveInitialState(tilemap);
        }
        else
        {
            Debug.LogError("Tilemap or LevelInfoScriptableObject is not assigned to the Gameobject.");
        }
    }
    public void SaveInitialState(Tilemap tilemap)
    {
        BoundsInt bounds = tilemap.cellBounds;
        levelInfo.InitialState = new TileBase[bounds.size.x, bounds.size.y];
        levelInfo.tileCount = 0;

        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                levelInfo.InitialState[x, y] = tilemap.GetTile(new Vector3Int(bounds.x + x, bounds.y + y, 0));

                if (levelInfo.InitialState[x, y] != null)
                {
                    levelInfo.tileCount++;
                }
            }
        }

        levelInfo.width = bounds.size.x;
        levelInfo.height = bounds.size.y;
        Debug.Log("Save Inital State of: " + $"({levelInfo.PublicBaseType} , {levelInfo.PublicVariation})");
        Debug.Log("Base Type: " + levelInfo.PublicBaseType);
        Debug.Log("Variation: " + levelInfo.PublicVariation);
        Debug.Log("Initial State Dimensions: " + levelInfo.width + " x " + levelInfo.height);
    }
}
