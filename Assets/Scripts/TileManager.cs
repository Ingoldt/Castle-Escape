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
    private int nextTileID;

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

    public int GetTileTypeDictionaryCount()
    {
        return tileTypeToID.Count;
    }

    public void InitializeReservedTiles()
    {
        // Assign Door tiles to reserved spots (0, 1, 2, 3)
        // Assign Wall tiles to (4)
        // Assign Spawn tiles to (5)
        AssignReservedTile(tileTypes.DoorList[0], 0);
        AssignReservedTile(tileTypes.DoorList[1], 1);
        AssignReservedTile(tileTypes.DoorList[2], 2);
        AssignReservedTile(tileTypes.DoorList[3], 3);
        AssignReservedTile(tileTypes.SpawnList[0], 4);
        AssignReservedTile(tileTypes.WallList[0], 5);
    }
    private void AssignReservedTile(TileBase tile, int reservedID)
    {
        tileTypeToID[tile] = reservedID;
    }

    public int GetTileValue(TileBase tile)
    {
        if (tile == null)
        {
            // Log the error and throw an exception
            Debug.LogError("Tile cannot be null");
            throw new ArgumentNullException(nameof(tile), "Tile cannot be null");
        }
        else
        {
            // Check if the tile is already in the dictionary
            if (!tileTypeToID.ContainsKey(tile))
            {
                tileTypeToID[tile] = nextTileID++;
            }
            return tileTypeToID[tile];
        }
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
        // Throw an InvalidOperationException if the ID is not found
        throw new InvalidOperationException($"Tile with ID {tileID} not found in level instance:");

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
        nextTileID = 6;
        InitializeReservedTiles();
        ResetLists();
    }
    public void ResetLists()
    {
        wallLocations.Clear();
        floorLocations.Clear();
        indestructableLocations.Clear();
        destructableLocations.Clear();
        doorLocations.Clear();
        spawnLocations.Clear();
    }

    public bool IsWallTile(TileBase tile)
    {
        return IsTileInList(tile, tileTypes.WallList);
    }

    public bool IsFloorTile(TileBase tile)
    {
        return IsTileInList(tile, tileTypes.FloorList);
    }

    public bool IsIndestructibleTile(TileBase tile)
    {
        return IsTileInList(tile, tileTypes.IndestructibleList);
    }

    public bool IsDestructibleTile(TileBase tile)
    {
        return IsTileInList(tile, tileTypes.BarrelList);
    }

    public bool IsDoorTile(TileBase tile)
    {
        return IsTileInList(tile, tileTypes.DoorList);
    }

    public bool IsSpawnTile(TileBase tile)
    {
        return IsTileInList(tile, tileTypes.SpawnList);
    }

    public bool IsTileInList(TileBase tile, List<TileBase> tileList)
    {
        bool status = tileList.Contains(tile);
        return status;
    }
}
