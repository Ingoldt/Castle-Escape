using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.Tilemaps;


public class LevelGeneration : MonoBehaviour
{
    [SerializeField]
    private List<GameObject> smallLevels;
    [SerializeField]
    private List<GameObject> mediumLevels;
    [SerializeField]
    private List<GameObject> largeLevels;
    [SerializeField]
    private GameObject parent;
    [SerializeField]
    private TileManager tileManager;
    private GameObject levelInstance;
    private LevelInfoScriptableObject levelInfo;
    private TileBase[,] previousState;
    private TileBase[,] currentState;
    private Vector3Int tilemapOrigin;

    public Vector3Int GetTilemapOrigin
    { get { return tilemapOrigin; } }
    public LevelInfoScriptableObject GetLevelInfo
    { get { return levelInfo;  } }
    public TileBase[,] GetPreviousState
    { get { return previousState; } }
    public TileBase[,] GetCurrentState
    { get { return currentState; } }



    //Debugging
    /*
    private void OnDrawGizmos()
    {
        Tilemap tilemap = levelInstance.GetComponentInChildren<Tilemap>();

        if (tilemap != null)
        {
            DrawTilemapOriginGizmo(tilemap);
        }
    }

    private void DrawTilemapOriginGizmo(Tilemap tilemap)
    {
        Vector3Int originCell = tilemap.origin;
        Vector3 originWorldPosition = tilemap.GetCellCenterWorld(originCell);

        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(originWorldPosition, new Vector3(tilemap.cellSize.x, tilemap.cellSize.y, 0));
    }
    */

    private void Awake()
    {
        // Access the TileManager component on the same GameObject
        tileManager = GetComponent<TileManager>();

        if (tileManager == null)
        {
            Debug.LogError("TileManager component not found on the same GameObject as LevelGeneration.");
        }
    }

    // Reset the environment and chooses a random initialization for traing 
    /*
    public void ResetLevelTraining()
    {
        // clean up left over instances 
        if (levelInstance != null)
        {
            Destroy(levelInstance);
        }

        if (smallLevels != null && smallLevels.Count > 0)
        {
            levelInstance = ChooseRandomLevel();
            //set parent structure
            levelInstance.transform.SetParent(parent.transform);
            // Apply the initial state
            InitializeTilemap();
        }
    }
    */

    public void CreateLevel(LevelInfoScriptableObject.BaseType baseType)
    {
        if (levelInfo == null || !levelInfo.playability)
        {
            levelInstance = ChooseRandomLevel(baseType);
            //set parent structure
            levelInstance.transform.SetParent(parent.transform);
            // Apply the initial state
            InitializeTilemap();
        }
        else
        {
            Debug.LogError("Cannot create level: LevelInfo is null or playability is false.");
        }
    }

    private GameObject ChooseRandomLevel(LevelInfoScriptableObject.BaseType baseType)
    {
        switch (baseType)
        {
            case LevelInfoScriptableObject.BaseType.Small:
                return InstanceRandomLevel(smallLevels);
            case LevelInfoScriptableObject.BaseType.Medium:
                return InstanceRandomLevel(mediumLevels);
            case LevelInfoScriptableObject.BaseType.Large:
                return InstanceRandomLevel(largeLevels);
            default:
                Debug.LogError("Unsupported BaseType");
                return null;
        }
    }

    private GameObject InstanceRandomLevel(List<GameObject> levelList)
    {
        if (levelList.Count > 0)
        {
            // Instantiate the selected prebuilt level
            levelInstance = Instantiate(levelList[Random.Range(0, levelList.Count)], parent.transform.position, Quaternion.identity);
            // Get LevelInfoScriptableObject from the prefab directly
            levelInfo = levelInstance.GetComponent<Level>().levelInfo;
            return levelInstance;
        }
        // Handle the case when the level list is empty
        Debug.LogError("No levels available in the specified list");
        return null;
    }

    private void InitializeTilemap()
    {
        
        TileBase[,] initialState = levelInfo.InitialState;

        int width = levelInfo.width;
        int height = levelInfo.height;

        // Create a new array with the same dimensions as initialState
        currentState = new TileBase[width, height];
        previousState = new TileBase[width,height];

        // Copy elements from initialState to currentState
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                currentState[x, y] = initialState[x, y];
                previousState[x,y] = initialState[x, y];
            }
        }
    }

    // update the tilemap by replacing a tile in the current state and update the previous state
    public void ReplaceTile(int x, int y, int newTileValue)
    {
        previousState[x, y] = currentState[x, y];
        currentState[x, y] = tileManager.GetTileFromID(newTileValue);
    }

    public void UpdateTilemapTraining(int x, int y)
    {
        Tilemap tilemap = levelInstance.GetComponentInChildren<Tilemap>();

        // Get the bottom-left corner of the tilemap in world coordinates
        Vector3Int parentCellPosition = tilemap.origin;

        // Calculate the cell position using the bottom-left corner and the given x, y
        Vector3Int cellPosition = new Vector3Int(parentCellPosition.x + x, parentCellPosition.y + y, 0);

        // Set the tile in the tilemap
        tilemap.SetTile(cellPosition, currentState[x, y]);

        // Update the collider
        UpdateCollider2D();
    }

    public void UpdateTilemap(TileBase[,] endState)
    {
        Tilemap tilemap = levelInstance.GetComponentInChildren<Tilemap>();

        // Ensure the dimensions match
        if (endState.GetLength(0) != currentState.GetLength(0) || endState.GetLength(1) != currentState.GetLength(1))
        {
            Debug.LogError("Mismatched dimensions between current state and new state arrays.");
            return;
        }

        // Iterate through the entire state array and update tiles
        for (int x = 0; x < endState.GetLength(0); x++)
        {
            for (int y = 0; y < endState.GetLength(1); y++)
            {
                // Get the bottom-left corner of the tilemap in world coordinates
                tilemapOrigin = tilemap.origin;

                // Calculate the cell position using the bottom-left corner and the current x, y
                Vector3Int cellPosition = new Vector3Int(tilemapOrigin.x + x, tilemapOrigin.y + y, 0);

                // Set the tile in the tilemap
                tilemap.SetTile(cellPosition, endState[x, y]);
            }
        }

        // Update the collider
        UpdateCollider2D();
    }

    public void UpdateCollider2D()
    {
        // in unity editor for tilemap collider 2d component "max tile change counterr" set dynamically for more freedome  
        TilemapCollider2D tilemapCollider = levelInstance.GetComponentInChildren<TilemapCollider2D>();
        if (tilemapCollider != null)
        {
            tilemapCollider.enabled = false;
            tilemapCollider.enabled = true;
        }
    }
}
