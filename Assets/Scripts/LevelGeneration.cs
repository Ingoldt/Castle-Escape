using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelGeneration : MonoBehaviour
{
    [SerializeField]
    private List<LevelInfoScriptableObject> prebuiltLevels;

    //Debugging
    /*
    void OnDrawGizmos()
    {
        Tilemap tilemap = GetComponent<Tilemap>();

        if (tilemap != null)
        {
            // Draw the tilemap bounds using Gizmos
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(tilemap.localBounds.center, tilemap.localBounds.size);
        }
    }
    */

    // Start is called before the first frame update
    void Start()
    {
        if (prebuiltLevels != null && prebuiltLevels.Count > 0) 
        {
            LevelInfoScriptableObject selectedLevel = ChooseRandomLevel();
            ApplyToTilemap(selectedLevel.initialState);
        }
    }

    private LevelInfoScriptableObject ChooseRandomLevel()
    {
        // Randomly choose an prebuild Level from the list
        int randomIndex = Mathf.FloorToInt(Random.Range(0, prebuiltLevels.Count));

        // Return the selected prebuilt level
        return prebuiltLevels[randomIndex];
    }

    void ApplyToTilemap(TileBase[,] initialState)
    {
        Tilemap tilemap = GetComponent<Tilemap>();

        int width = initialState.GetLength(0);
        int height = initialState.GetLength(1);

        BoundsInt bounds = new BoundsInt(0, 0, 0, width, height, 0);

        for(int x = 0; x < bounds.size.x;x++)
        {
            for(int y = 0; y < bounds.size.y;y++)
            {
                TileBase tile = initialState[x, y];
                Vector3Int position = new Vector3Int(x, y, 0);
                tilemap.SetTile(position, tile);

                // Print out the tile information
                if (tile != null)
                {
                    Debug.Log($"Set tile at ({position.x}, {position.y}): {tile.name}");
                }
                else
                {
                    Debug.Log($"No tile set at ({position.x}, {position.y})");
                }
            }
        }
        
    }
}
