using System.Collections;
using System.Collections.Generic;
using Unity.Barracuda;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PathfindingTest : MonoBehaviour
{
    [SerializeField]
    private LevelGeneration levelGen;
    [SerializeField]
    private TileManager manager;
    [SerializeField]
    private Pathfinding pathfinding;
    [SerializeField]
    GameObject prefab;

    private TileBase[,] previousState;
    private TileBase[,] currentState;
    // Start is called before the first frame update
    void Start()
    {
        GameObject level = Instantiate(prefab);
        LevelInfoScriptableObject levelInfo = levelGen.GetLevelInfo;
        InitializeTilemap();

        Vector2Int doorLocation = manager.DoorLocations[0];
        Vector2Int spawnLocation = manager.SpawnLocations[0];

        if (pathfinding.ContainsPath(spawnLocation, doorLocation, levelInfo.width, levelInfo.height, levelGen.GetCurrentState))
        {
            Debug.Log("Level is Playable");

        }

    }

    private void InitializeTilemap()
    {
        LevelInfoScriptableObject levelInfo = levelGen.GetLevelInfo;
        TileBase[,] initialState = levelInfo.InitialState;

        int width = levelInfo.width;
        int height = levelInfo.height;

        // Create a new array with the same dimensions as initialState
        currentState = new TileBase[width, height];
        previousState = new TileBase[width, height];

        // Copy elements from initialState to currentState
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                currentState[x, y] = initialState[x, y];
                previousState[x, y] = initialState[x, y];
            }
        }

        /*
        string directory = @"D:\Downloads";
        string filePath = Path.Combine(directory, "yourFileName.csv");
        SaveCurrentStateToCSV(filePath);
        */
    }
}
