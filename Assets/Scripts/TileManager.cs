using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class TileManager : MonoBehaviour
{
    public TileTypesScriptableObject tileTypes;
    [SerializeField]
    private Dictionary<TileBase, int> tileTypeToID = new Dictionary<TileBase, int>();
    private int nextTileID = 1;

    private List<Vector2Int> wallLocations = new List<Vector2Int>();
    public List<Vector2Int> WallLocations { get { return wallLocations; }}
    private List<Vector2Int> floorLocations = new List<Vector2Int>();
    public List<Vector2Int> FloorLocations { get { return floorLocations; }}
    private List<Vector2Int> indestructableLocations = new List<Vector2Int>();
    public List<Vector2Int> IndestructableLocations { get { return indestructableLocations; }}
    private List<Vector2Int> destructableLocations = new List<Vector2Int>();
    public List<Vector2Int> DestructableLocations { get { return destructableLocations; }}
    private List<Vector2Int> doorLocations = new List<Vector2Int>();
    public List<Vector2Int> DoorLocations { get { return doorLocations; }}
    private List<Vector2Int> spawnLocations = new List<Vector2Int>();
    public List<Vector2Int> SpawnLocations { get { return spawnLocations; }}

    public int GetTileValue(TileBase tile)
    {
        if (tile != null)
        {
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
                if (IsWallTile(tile))
                {
                    wallLocations.Add(new Vector2Int(x, y));
                }
                else if (IsFloorTile(tile))
                {
                    floorLocations.Add(new Vector2Int(x, y));
                }
                else if (IsIndestructibleTile(tile))
                {
                    indestructableLocations.Add(new Vector2Int(x, y));
                }
                else if (IsDestructibleTile(tile))
                {
                    destructableLocations.Add(new Vector2Int(x, y));
                }
                else if (IsDoorTile(tile))
                {
                    doorLocations.Add(new Vector2Int(x, y));
                }
                else if (IsSpawnTile(tile))
                {
                    spawnLocations.Add(new Vector2Int(x, y));
                }
            }
        }
    }

    // Reset the state, including the tileTypeToID dictionary
    public void ResetState()
    {
        tileTypeToID.Clear();
        nextTileID = 1;

        wallLocations.Clear();
        floorLocations.Clear();
        indestructableLocations.Clear();
        destructableLocations.Clear();
        doorLocations.Clear();
        spawnLocations.Clear();
    }

    public bool IsWallTile(TileBase tile)
    {
        return tileTypes.WallList.Contains(tile);
    }

    public bool IsFloorTile(TileBase tile)
    {
        return tileTypes.FloorList.Contains(tile);
    }

    public bool IsIndestructibleTile(TileBase tile)
    {
        return tileTypes.IndestructibleList.Contains(tile);
    }

    public bool IsDestructibleTile(TileBase tile)
    {
        return tileTypes.BarrelList.Contains(tile);
    }

    public bool IsDoorTile(TileBase tile)
    {
        return tileTypes.DoorList.Contains(tile);
    }

    public bool IsSpawnTile(TileBase tile)
    {
        return tileTypes.SpawnList.Contains(tile);
    }

    public bool IsTileInList(TileBase tile, List<TileBase> tileList)
    {
        return tileList.Contains(tile);
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
