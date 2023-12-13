using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelToArray : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Tilemap tilemap = GetComponent<Tilemap>();

        if (tilemap != null)
        {
            ExtractLevel(tilemap);
        }
        else
        {
            Debug.LogError("Tilemap component not found on the GameObject.");
        }
    }

    void ExtractLevel(Tilemap tilemap)
    {
        BoundsInt bounds = tilemap.cellBounds;
        TileBase[,] levelArray = new TileBase[bounds.size.x, bounds.size.y];

        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                levelArray[x, y] = tilemap.GetTile(new Vector3Int(bounds.x + x, bounds.y + y, 0));
            }
        }

        //Save level as array in a txt file for debugging
        // Create or overwrite the text file
        string filePath = "A:/Unity/Repos/Level.txt";
        using (StreamWriter writer = new StreamWriter(filePath, false))
        {
            // Write the array structure to the file
            writer.WriteLine("levelArray:");
            for (int y = bounds.size.y - 1; y >= 0; y--)
            {
                string row = "";
                for (int x = 0; x < bounds.size.x; x++)
                {
                    row += (levelArray[x, y] != null ? levelArray[x, y].name : "null") + "\t";
                }
                writer.WriteLine(row);
            }
        }

        Debug.Log($"Level information saved to: {filePath}");
    }
}
