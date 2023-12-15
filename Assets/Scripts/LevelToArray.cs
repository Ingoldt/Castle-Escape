using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelToArray : MonoBehaviour
{
    [SerializeField]
    private LevelInfoScriptableObject levelInfo;
    // Start is called before the first frame update
    void Awake()
    {
        Tilemap tilemap = GetComponent<Tilemap>();
        if (tilemap != null)
        {
            ExtractAndSaveInitialState(tilemap);
        }
        else
        {
            Debug.LogError("Tilemap or LevelInfoScriptableObject is not assigned to the Gameobject.");
        }
    }
    public void ExtractAndSaveInitialState(Tilemap tilemap)
    {
        BoundsInt bounds = tilemap.cellBounds;
        levelInfo.initialState = new TileBase[bounds.size.x, bounds.size.y];

        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                levelInfo.initialState[x, y] = tilemap.GetTile(new Vector3Int(bounds.x + x, bounds.y + y, 0));
            }
        }
        Debug.Log("Base Type: " + levelInfo.PublicBaseType);
        Debug.Log("Variation: " + levelInfo.PublicVariation);
        Debug.Log("Initial State Dimensions: " + levelInfo.initialState.GetLength(0) + " x " + levelInfo.initialState.GetLength(1));
    }
}
