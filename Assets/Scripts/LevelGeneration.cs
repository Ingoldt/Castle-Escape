using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using UnityEngine;
using UnityEngine.Tilemaps;

public class LevelGeneration : MonoBehaviour
{
    
    public LevelGeneratorAgent agent;
    [SerializeField]
    private List<GameObject> levelPrefabs;
    [SerializeField]
    private GameObject parent;
    [SerializeField]
    private TileManager tileManager;

    private GameObject levelInstance;
    private LevelInfoScriptableObject levelInfo;

    private TileBase[,] previousState;
    private TileBase[,] currentState;

    public LevelInfoScriptableObject GetLevelInfo
    { get { return levelInfo;  } }
    public TileBase[,] GetPreviousState
    { get { return previousState; } }
    public TileBase[,] GetCurrentState
    { get { return currentState; } }

    // Reset the environment and chooses a random initialization
    public void ResetLevel()
    {
        // clean up left over instances 
        if (levelInstance != null)
        {
            Destroy(levelInstance);
        }

        if (levelPrefabs != null && levelPrefabs.Count > 0) 
        {
            levelInstance = ChooseRandomLevel();
            //set parent structure
            levelInstance.transform.SetParent(parent.transform);
            // Apply the initial state
            InitializeTilemap();
            Debug.Log("Initialized both states");
            agent.UpdateBehaviorParameters();
        }
    }

    private GameObject ChooseRandomLevel()
    {
        // Randomly choose an prebuild Level from the list
        int randomIndex = Mathf.FloorToInt(Random.Range(0, levelPrefabs.Count));
        
        // Instantiate the selected prebuilt level
        levelInstance = Instantiate(levelPrefabs[randomIndex], transform.localPosition, Quaternion.identity);

        // Get LevelInfoScriptableObject from the prefab directly
        levelInfo = levelInstance.GetComponent<Level>().levelInfo;

        return levelInstance;
    }

    void InitializeTilemap()
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
        /*
        string directory = @"D:\Downloads";
        string filePath = Path.Combine(directory, "yourFileName.csv");
        SaveCurrentStateToCSV(filePath);
        */
    }

    // update the tilemap by replacing a tile in the current state and update the previous state
    public void ReplaceTile(int x, int y, int newTileValue)
    {
        previousState[x, y] = currentState[x, y];
        currentState[x, y] = tileManager.GetTileFromID(newTileValue);
    }

    public void UpdateTilemap()
    {
        Tilemap tilemap = levelInstance.GetComponentInChildren<Tilemap>();

        // Ensure the dimensions match
        int width = currentState.GetLength(0);
        int height = currentState.GetLength(1);

        BoundsInt bounds = new BoundsInt(0, 0, 0, width, height, 0);

        for (int x = 0; x < bounds.size.x; x++)
        {
            for (int y = 0; y < bounds.size.y; y++)
            {
                TileBase tile = currentState[x, y];
                Vector3Int position = new Vector3Int(x, y, 0);
                tilemap.SetTile(position, tile);
            }
        }
    }

    //Just for debugging
    void SaveCurrentStateToCSV(string filePath)
    {
        if (currentState == null)
        {
            Debug.LogError("Current state is not initialized.");
            return;
        }

        int width = currentState.GetLength(0);
        int height = currentState.GetLength(1);

        // Create or overwrite the CSV file
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            for (int y = height - 1; y >= 0; y--)
            {
                string rowString = "";

                for (int x = 0; x < width; x++)
                {
                    TileBase tile = currentState[x, y];

                    if (tile != null)
                    {
                        // Assuming you are using Unity's default Tile component
                        if (tile is Tile unityTile)
                        {
                            // Assuming that your tiles have a sprite assigned
                            rowString += unityTile.sprite.name + ",";
                        }
                        else
                        {
                            rowString += "UnknownTileType,";
                        }
                    }
                    else
                    {
                        rowString += "Empty,";
                    }
                }

                // Remove the trailing comma and write the row to the file
                writer.WriteLine(rowString.TrimEnd(','));
            }
        }

        Debug.Log("Current state saved to: " + filePath);
    }

}
