using UnityEngine.Tilemaps;
using UnityEngine;
using System.Collections.Generic;
using System;

public class ChangeSprite : MonoBehaviour
{
    private TileManager _tilemanager;
    private List<TileBase> _closedDoors;
    private List<TileBase> _barrelTiles;
    private List<TileBase> _floorTiles;
    private Tilemap _tilemap;
    private List<Vector3> _doorPos = new List<Vector3>();
    private TileBase _doorTile = null;
    // Map to store current health for each barrel position
    private Dictionary<Vector3Int, int> barrelHealthMap = new Dictionary<Vector3Int, int>();

    public static event Action OnKeyUse;

    public List<TileBase> openDoors;
    public int maxBarrelHealth = 15;

    private void OnEnable()
    {
        KeyScript.OnPlayerHasKey += HandlePlayerHasKey;
        GameController.OnLevelGenrationCompleted += HandleLevelGenerated;
    }

    private void OnDisable()
    {
        KeyScript.OnPlayerHasKey -= HandlePlayerHasKey;
        GameController.OnLevelGenrationCompleted -= HandleLevelGenerated;
    }

    void Start()
    {
        _tilemanager = GameController.instance.GetTileManager();
    }

    void Update()
    {
        if (GameController.instance.playerManagerScript.GetPlayerHasKey())
        {
            Vector3 playerPos = GameController.instance.playerManagerScript.GetPlayerPosition();

            foreach (Vector3 doorPos in _doorPos)
            {
                // Calculate Manhattan distance between player and door
                float distance = Mathf.Abs(playerPos.x - doorPos.x) + Mathf.Abs(playerPos.y - doorPos.y);

                if (distance <= 2)
                {
                    if (_doorTile != null)
                    {
                        switch (_doorTile.name)
                        {
                            case "Door_Closed_1":
                                _tilemap.SetTile(_tilemap.WorldToCell(doorPos), openDoors[0]);
                                break;
                            case "Door_Closed_2":
                                _tilemap.SetTile(_tilemap.WorldToCell(doorPos), openDoors[1]);
                                break;
                            case "Door_Closed_3":
                                _tilemap.SetTile(_tilemap.WorldToCell(doorPos), openDoors[2]);
                                break;
                            case "Door_Closed_4":
                                _tilemap.SetTile(_tilemap.WorldToCell(doorPos), openDoors[3]);
                                break;
                        }

                        // Disable collider for this tile
                        _tilemap.SetColliderType(_tilemap.WorldToCell(doorPos), Tile.ColliderType.None);

                        // invoke event stat key is used 
                        OnKeyUse?.Invoke();
                        // Reset _playerHasKey to false
                        GameController.instance.playerManagerScript.SetPlayerHasKey(false);
                    }
                }
            }
        }
    }

    private void HandleLevelGenerated()
    {
        _tilemap = GetComponent<Tilemap>();

        // get cloased door sprites
        _closedDoors = _tilemanager.tileTypes.DoorList;
        // get barrel sprites 
        _barrelTiles = _tilemanager.tileTypes.BarrelList;
        // get floor sprites
        _floorTiles = _tilemanager.tileTypes.FloorList;

        // Get the bounds of the tilemap
        BoundsInt bounds = _tilemap.cellBounds;

        // Iterate over all positions in the tilemap
        foreach (var position in bounds.allPositionsWithin)
        {
            Vector3Int tilePosition = new Vector3Int(position.x, position.y, 0);
            TileBase tile = _tilemap.GetTile(tilePosition);

            // Check if the tile is a door tile
            if (tile != null && _closedDoors.Contains(tile))
            {
                // Add the center position of the door tile to the list
                _doorPos.Add(_tilemap.GetCellCenterWorld(tilePosition));
                _doorTile = tile;
            }
            else if (tile != null && _barrelTiles.Contains(tile))
            {
                barrelHealthMap[tilePosition] = maxBarrelHealth;
            }
            else
            {

                //Debug.LogWarning("Pos: " + tilePosition + ", Tile: " + tile);
            }
        }
    }

    private void HandlePlayerHasKey()
    {
        Debug.Log("Player has optained the key");
        GameController.instance.playerManagerScript.SetPlayerHasKey(true);
    }

    public void BarrelTakeDamage(int damage, Vector3Int tilePosition, string tileName)
    {
        if (barrelHealthMap.ContainsKey(tilePosition))
        {
            barrelHealthMap[tilePosition] -= damage;

            if (barrelHealthMap[tilePosition] <= 0)
            {
                switch (tileName)
                {
                    case "Barrel_1":
                        _tilemap.SetTile(_tilemap.WorldToCell(tilePosition), _floorTiles[0]);
                        break;
                    case "Barrel_2":
                        _tilemap.SetTile(_tilemap.WorldToCell(tilePosition), _floorTiles[1]);
                        break;
                    case "Barrel_3":
                        _tilemap.SetTile(_tilemap.WorldToCell(tilePosition), _floorTiles[2]);
                        break;
                    case "Barrel_4":
                        _tilemap.SetTile(_tilemap.WorldToCell(tilePosition), _floorTiles[3]);
                        break;
                }

                // disable collider 
                _tilemap.SetColliderType(_tilemap.WorldToCell(tilePosition), Tile.ColliderType.None);

                // Remove the barrel from the health map
                barrelHealthMap.Remove(tilePosition);
            }
        }
    }
}