using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine.Tilemaps;
using Unity.Barracuda;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public class LevelGeneratorAgent : Agent
{
    // Reference assigned in the Inspector
    [SerializeField]
    private LevelGeneration levelGeneration;
    private Dictionary<TileBase, int> tileTypeToID = new Dictionary<TileBase, int>();
    private int nextTileID = 1; // empty tile (ID = 0)


    public override void OnEpisodeBegin()
    {
        // Reset the environment for a new episode
        levelGeneration.ResetEnvironment();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Access levelInstance
        GameObject currentLevel = levelGeneration.GetLevelInstance;

        TileBase[,] previousState = currentLevel.GetComponent<LevelInfoScriptableObject>().PreviousState;
        TileBase[,] currentState = currentLevel.GetComponent<LevelInfoScriptableObject>().CurrentState;
        TileBase[,] futureState = currentLevel.GetComponent<LevelInfoScriptableObject>().FutureState;

        // Flatten the observations into a 1D array
        FlattenObservations(sensor, previousState);
        FlattenObservations(sensor, currentState);
        FlattenObservations(sensor, futureState);

    }

    private void FlattenObservations(VectorSensor sensor, TileBase[,] tilemap)
    {
        int width = tilemap.GetLength(0);
        int height = tilemap.GetLength(1);

        if (width != 0 && height != 0)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    TileBase tile = tilemap[x, y];
                    int tileValue = GetTileValue(tile); // Convert TileBase to an integer value
                    sensor.AddObservation(tileValue);
                }
            }
        }
    }


    // Convert a TileBase to an integer value (you might need to adjust this based on your tiles)
    private int GetTileValue(TileBase tile)
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
}
