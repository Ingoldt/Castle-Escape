using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileManager : MonoBehaviour
{
    public TileTypesScriptableObject tileTypes;
    [SerializeField]
    private TileBase[] essentialTiles;
    private Dictionary<TileBase, int> tileTypeToID = new Dictionary<TileBase, int>();
    private int nextTileID = 6;

    private List<Vector2Int> wallLocations = new List<Vector2Int>();
    public List<Vector2Int> WallLocations { get; private set; }
    private List<Vector2Int> floorLocations = new List<Vector2Int>();
    public List<Vector2Int> FloorLocations { get; private set; }
    private List<Vector2Int> indestructableLocation = new List<Vector2Int>();
    public List<Vector2Int> IndestructableLocation { get; private set; }
    private List<Vector2Int> destructableLocation = new List<Vector2Int>();
    public List<Vector2Int> DestructableLocation { get; private set; }
    private List<Vector2Int> doorLocation = new List<Vector2Int>();
    public List<Vector2Int> DoorLocation { get; private set; }
    private List<Vector2Int> spawnLocation = new List<Vector2Int>();
    public List<Vector2Int> SpawnLocation { get; private set; }

    public int GetTileValue(TileBase tile)
    {
        if (tile != null)
        {
            // Check for essential tiles and assigning a specific id 

            if (essentialTiles != null && ContainsTile(essentialTiles,tile))
            {
                // Check if the tile is already in the dictionary
                if (!tileTypeToID.ContainsKey(tile))
                {
                    // Assign specific ID according to the array position
                    int arrayIndex = Array.IndexOf(essentialTiles, tile);
                    tileTypeToID[tile] = arrayIndex + 1; // Adding 1 to avoid 0 as ID
                }
                return tileTypeToID[tile];
            }
            // Check if the tile is already in the dictionary
            if (!tileTypeToID.ContainsKey(tile))
            {
                tileTypeToID[tile] = nextTileID++;
            }
            return tileTypeToID[tile];
        }

        return 0; // Default to 0 if the tile is not recognized or empty
    }

    public TileBase GetTileFromID(int tileID)
    {
        // Find Tile associated with the given ID
        foreach (var kvp in tileTypeToID)
        {
            if (kvp.Value == tileID)
            {
                return kvp.Key;
            }
        }
        // Return null if the ID is not found or there is no tile at this location
        return null;

    }

    public void LocateCategorizeTiles(TileBase[,] tilemap)
    {
        int width = tilemap.GetLength(0);
        int height = tilemap.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                TileBase tile = tilemap[x, y];
                if (isWallTile(tile))
                {
                    wallLocations.Add(new Vector2Int(x, y));
                }
                if (isFloorTile(tile))
                {
                    floorLocations.Add(new Vector2Int(x, y));
                }
                if (isIndestructableTile(tile))
                {
                    indestructableLocation.Add(new Vector2Int(x, y));
                }
                if (isDestructableTile(tile))
                {
                    destructableLocation.Add(new Vector2Int(x, y));
                }
                if (isDoorTile(tile))
                {
                    doorLocation.Add(new Vector2Int(x, y));
                }
                if (isSpawnTile(tile))
                {
                    spawnLocation.Add(new Vector2Int(x, y));
                }
            }
        }
    }

    public bool isWallTile(TileBase tile) 
    {
        if(tileTypes.WallList.Contains(tile))
        {
            return true;
        }
        return false;
    }

    public bool isFloorTile(TileBase tile)
    {
        if (tileTypes.FloorList.Contains(tile))
        {
            return true;
        }
        return false;
    }

    public bool isIndestructableTile(TileBase tile)
    {
        if (tileTypes.IndestructibleList.Contains(tile))
        {
            return true;
        }
        return false;
    }

    public bool isDestructableTile(TileBase tile)
    {
        if (tileTypes.BarrelList.Contains(tile))
        {
            return true;
        }
        return false;
    }

    public bool isDoorTile(TileBase tile)
    {
        if (tileTypes.DoorList.Contains(tile))
        {
            return true;
        }
        return false;
    }

    public bool isSpawnTile(TileBase tile)
    {
        if (tileTypes.SpawnList.Contains(tile))
        {
            return true;
        }
        return false;
    }


    // Checking if a certain tile type is within the array
    private bool ContainsTile(TileBase[] targetTiles, TileBase tile)
    {
        foreach (var t in targetTiles) 
        { 
            if(t == tile)
            {
                return true;
            }
        }

        return false;
    }
}
