using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine.Tilemaps;
using Unity.Barracuda;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.MLAgents.Actuators;
using UnityEditor.Experimental.GraphView;
using Unity.MLAgents.Policies;

public class LevelGeneratorAgent : Agent
{

    private BehaviorParameters behaviorParameters;

    // Reference assigned in the Inspector
    [SerializeField]
    private LevelGeneration levelGeneration;
    [SerializeField]
    private TileManager tileManager;
    [SerializeField]
    private Pathfinding pathFinder;

    TileBase[,] previousState;
    TileBase[,] currentState;
    private int prevX;
    private int prevY;
    private int height;
    private int width;

    private float totalDirectReward;
    private float totalIndirectReward;

    public void UpdateBehaviorParameters()
    {
        behaviorParameters = GetComponent<BehaviorParameters>();
        if (behaviorParameters != null)
        {
            previousState = levelGeneration.GetPreviousState;
            currentState = levelGeneration.GetCurrentState;
            // Set Observation Space size
            int vectorObservationSize = currentState.Length + previousState.Length;
            behaviorParameters.BrainParameters.VectorObservationSize = vectorObservationSize;
            Debug.Log($"Vector Observation Size: {behaviorParameters.BrainParameters.VectorObservationSize}");

            // Set Branch size for each discreate branch
            // int numberOfBranches = behaviorParameters.
        }
    }

    public override void OnEpisodeBegin()
    {
        totalDirectReward = 0f;
        totalIndirectReward = 0f;
        // select a random level for a new episode
        levelGeneration.ResetLevel();
    }

    public override void CollectObservations(VectorSensor sensor)
    {

        previousState = levelGeneration.GetPreviousState;
        currentState = levelGeneration.GetCurrentState;

        // Flatten the 2d array into a 1D array and add it to the observation
        FlattenObservations(sensor, previousState);
        FlattenObservations(sensor, currentState);

    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // read action from action buffer
        int x = (int)actions.DiscreteActions[0];
        int y = (int)actions.DiscreteActions[1];
        int newTileValue = (int)actions.DiscreteActions[2];

        // replace tile at the given coordinates
        if (levelGeneration.GetLevelInfo != null)
        {
            // check if coordinates are valid
            if (x >= 0 && x < width && y >= 0 && y < height)
            {
                levelGeneration.ReplaceTile(x, y, newTileValue);
                // update visuals
                levelGeneration.UpdateTilemap();
                //reward calculation
                float directReward = CalculatingExplorationReward(x, y, prevX, prevY, newTileValue);
                AddReward(directReward);
                totalDirectReward += directReward;

                // Save current action's coordinates as previous action
                // needed for poximity check
                prevX = x;
                prevY = y;

                // Check if the episode has ended
                if (StepCount >= MaxStep - 1)
                {
                    float indirectReward = CalculateIndirectReward(); // Implement CalculateIndirectReward as needed
                    AddReward(indirectReward); // Add indirect reward during the episode
                    totalIndirectReward += indirectReward;

                    // Combine direct and indirect rewards to get the total episode reward
                    float totalReward = totalDirectReward + totalIndirectReward;

                    // Use totalReward as needed (e.g., for logging or training)

                    EndEpisode();
                }

            }
        }
    }

    //Flatten the Tilemap for the Agent's Observation
    private void FlattenObservations(VectorSensor sensor, TileBase[,] tilemap)
    {
        width = levelGeneration.GetLevelInfo.width;
        height = levelGeneration.GetLevelInfo.height;

        if (width != 0 && height != 0)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    TileBase tile = tilemap[x, y];
                    int tileValue = tileManager.GetTileValue(tile); // Convert TileBase to an integer value
                    sensor.AddObservation(tileValue);
                }
            }
        }
    }

    //reward calculation
    private float CalculateIndirectReward()
    {
        float totalReward = 0;

        // Reward calculation for Playabilty of the level and smooth tileing
        float playabilityReward = CalculatingPlayabilityReward();
        float tileMatchingReward = CalculatetileMatchingReward();

        totalReward += playabilityReward + tileMatchingReward;
        return totalReward;
    }

    private float CalculatingExplorationReward(int x, int y, int prevX, int prevY, int newTileValue)
    {
        float reward = 0f;

        // reward for placing a tile new Tile
        if (levelGeneration.GetPreviousState[x, y] != tileManager.GetTileFromID(newTileValue))
        {
            reward += 1f;
        }
        if (DistanceToPreviousTile(x, y, prevX, prevY) < 2)
        {
            reward += -0.5f;
        }
        if (DistanceToPreviousTile(x, y, prevX, prevY) > 2 && (DistanceToPreviousTile(x, y, prevX, prevY) < 4))
        {
            reward += 0.1f;
        }
        if (DistanceToPreviousTile(x, y, prevX, prevY) > 4)
        {
            reward += 0.5f;
        }

        return reward;
    }

    private int DistanceToPreviousTile(int x, int y, int prevX, int prevY)
    {
        // calculating the Manhattan distance
        return Mathf.Abs(x - prevX) + (y - prevY);
    }

    private float CalculatingPlayabilityReward()
    {
        float reward = 0f;

        List<Vector2Int> doorLocations = tileManager.DoorLocation;
        List<Vector2Int> spawnLocations = tileManager.SpawnLocation;
        int doorCount = tileManager.DoorLocation.Count;
        int spawnCount = tileManager.SpawnLocation.Count;

        // Reward calculation for correct ammount of reachable doors from spawn
        if (doorCount + spawnCount > 2)
        {
            //IsReachable(doorLocations, spawnLocations);
            reward += -15f;

        }
        else 
        {
            IsReachable(doorLocations, spawnLocations);
            reward += 15f;
        }

        return reward;
    }

    private bool IsReachable(List<Vector2Int> doorLocations, List<Vector2Int> spawnLocations)
    {
        //##### Implementation of is reachable and call of A* search
        for (int i = 0; i < doorLocations.Count; i++) 
        {   
            Vector2Int doorLocation = doorLocations[i];
            Vector2Int spawnLocation = spawnLocations[i];

            if (pathFinder.containsPath(doorLocation, spawnLocation, width, height, currentState))
            {
                return true;
            }
        }
        return false;
    }

    private float CalculatetileMatchingReward()
    {
        float reward = 0f;

        return reward;
    }
}
