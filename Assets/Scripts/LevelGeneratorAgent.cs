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
using System.Diagnostics.Tracing;
using System;

public class LevelGeneratorAgent : Agent
{

    private BehaviorParameters behaviorParameters;

    // Reference assigned in the Inspector
    [SerializeField]
    private LevelGeneration levelGeneration;
    [SerializeField]
    private TileManager tileManager;

    TileBase[,] previousState;
    TileBase[,] currentState;
    private int prevX;
    private int prevY;
    private int height;
    private int width;

    private float totalDirectReward;
    private float totalIndirectReward;

    private const float TilePlacementReward = 1f;
    private const float CloseDistancePenalty = -0.5f;
    private const float MediumDistanceReward = 0.1f;
    private const float FarDistanceReward = 0.5f;
    private const float NotPlayablePenalty = -5f;
    private const float PlayableReward = 50f;
    private const float ConstraintReward = 5f;
    private const float WallConstraintPenalty = -1,25f;
    private const float DoorAmountPenalty = -1f;
    private const float DoorRotationReward = 0.5f;
    private const float TileMatchingReward = 10f;


    /*
    public void UpdateBehaviorParameters()
    {
        behaviorParameters = GetComponent<BehaviorParameters>();
        if (behaviorParameters != null)
        {
            Debug.Log($"Vector Observation Size: {behaviorParameters.BrainParameters.VectorObservationSize}");

            // Set Branch size for each discreate branch
            // int numberOfBranches = behaviorParameters.
        }
    }
    */


    public override void OnEpisodeBegin()
    {
        
        totalDirectReward = 0f;
        totalIndirectReward = 0f;
        // reset the list of the Tilemanager
        tileManager.ResetState();
        // select a random level for a new episode
        levelGeneration.ResetLevel();
        // 20% of the tiles in the level can be altered by the agend rounding it up to the nearest integer
        base.MaxStep = (int)(levelGeneration.GetLevelInfo.tileCount * 0.15f + 0.5f);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        if (levelGeneration == null)
        {
            Debug.LogError("LevelGeneration is not assigned.");
            return;
        }

        if (tileManager == null)
        {
            Debug.LogError("TileManager is not assigned.");
            return;
        }

        previousState = levelGeneration.GetPreviousState;
        currentState = levelGeneration.GetCurrentState;

        // Flatten the 2d array into a 1D array and add it to the observation
        FlattenObservations(sensor, previousState);
        FlattenObservations(sensor, currentState);

        sensor.AddObservation(GetRemainingStepsRatio());
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // read action from action buffer
        int x = (int)actions.DiscreteActions[0];
        int y = (int)actions.DiscreteActions[1];
        int newTileValue = (int)actions.DiscreteActions[2];

        /*
        Debug.Log("Agent action: " + newTileValue);
        TileBase tile = tileManager.GetTileFromID(newTileValue);
        Debug.Log("Retrieved Tile: " + (tile != null ? tile.name : "null"));
        */

        // replace tile at the given coordinates
        if (levelGeneration.GetLevelInfo != null)
        {
            // check if coordinates are valid
            if (x >= 0 && x < width && y >= 0 && y < height)
            {
                levelGeneration.ReplaceTile(x, y, newTileValue);
                // update visuals
                levelGeneration.UpdateTilemap(x, y);

                //�locate all tile types
                tileManager.LocateCategorizeTiles(currentState);

                // direct reward calculation
                float directReward = CalculateExplorationReward(x, y, prevX, prevY, newTileValue);
                AddReward(directReward);
                totalDirectReward += directReward;

                // Save current action's coordinates as previous action
                // needed for poximity check
                prevX = x;
                prevY = y;

                // Check if the episode is about to end
                if (StepCount >= MaxStep - 1)
                {
                    float indirectReward = CalculateIndirectReward(); // Implement CalculateIndirectReward as needed
                    AddReward(indirectReward); // Add indirect reward during the episode
                    totalIndirectReward += indirectReward;

                    EndEpisode();
                }
            }
        }
    }

    // Flatten the Tilemap for the Agent's Observation
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

    private float GetRemainingStepsRatio()
    {
        // Calculate the remaining steps ratio (between 0 and 1)
        float remainingStepsRatio = Mathf.Clamp01((float)StepCount / MaxStep);

        return remainingStepsRatio;
    }


    // Reward calculation
    private float CalculateExplorationReward(int x, int y, int prevX, int prevY, int newTileValue)
    {
        float distance = DistanceToPreviousTile(x, y, prevX, prevY);
        float reward = 0f;

        if (levelGeneration.GetPreviousState[x, y] != tileManager.GetTileFromID(newTileValue))
        {
            // reward for replacing a tile with a new Tile
            reward += TilePlacementReward;

            if (newTileValue <= 3)
            {
                // Reserved tiles (Door Tiles)
                return reward += TileConstraints(x, y, newTileValue);
            }
            else if (newTileValue == 4)
            {
                // wall tiles
                // more close together placement of wall tiles
                return reward += TileConstraints(x, y, newTileValue);
            }
            else
            {
                // all other tiles within the current level
                // exploring distance
                if (distance < 2)
                {
                    return reward += CloseDistancePenalty;
                }
                if (distance > 2 && distance < 4)
                {
                    return reward += MediumDistanceReward;
                }
                if (distance > 4)
                {
                    return reward += FarDistanceReward;
                }
            }
        }

        return reward;
    }

    private int DistanceToPreviousTile(int x, int y, int prevX, int prevY)
    {
        // calculating the Manhattan distance to previous tile
        return Mathf.Abs(x - prevX) + (y - prevY);
    }

    private float TileConstraints(int x, int y, int newTileValue)
    {
        float reward = 0f;

        /*
        Debug.Log("Tile ID: " + newTileValue);
        Debug.Log("coordinates " + " x " + x + " y " + y);
        TileBase tile = tileManager.GetTileFromID(newTileValue);
        Debug.Log("Retrieved Tile: " + (tile != null ? tile.name : "null"));
        */

        int doorAmount = tileManager.DoorLocations.Count;
        string tileName = tileManager.GetTileFromID(newTileValue)?.name;

        if (newTileValue == 4) 
        {
            Debug.Log("Tile ID: " + newTileValue);
            Debug.Log("coordinates " + " x " + x + " y " + y);
            TileBase tile = tileManager.GetTileFromID(newTileValue);
            Debug.Log("Retrieved Tile: " + (tile != null ? tile.name : "null"));

        }

        // Calculate Manhattan distances to each side of the grid
        int distanceTop = y;
        int distanceBottom = height - y;
        int distanceLeft = x;
        int distanceRight = width - x;

        // Determine the side with the minimum distance
        int minDistance = Mathf.Min(distanceTop, distanceBottom, distanceLeft, distanceRight);
        // Adjust offsets based on door orientation
        int[] xOffset;
        int[] yOffset;

        // door tile top, right, bottom, left
        if (newTileValue <= 3)
        {
            if (newTileValue == 0) // Door_Closed_1 (top)
            {
                xOffset = new int[] { -1, 0, 1, -1, 1, };
                yOffset = new int[] { -1, -1, -1, 0, 0, };
            }
            else if (newTileValue == 1) // Door_Closed_2 (right)
            {
                xOffset = new int[] { 0, 1, 1, 0, 1 };
                yOffset = new int[] { -1, -1, 0, 1, 1 };
            }
            else if (newTileValue == 2) // Door_Closed_3 (bottom)
            {
                xOffset = new int[] { -1, 1, -1, 0, 1 };
                yOffset = new int[] { 0, 0, 1, 1, 1 };
            }
            else // Door_Closed_4 (left)
            {
                xOffset = new int[] { -1, 0, -1, -1, 0 };
                yOffset = new int[] { -1, -1, 0, 1, 1 };
            }

            int counter = CheckNeighborsForTile(x, y, xOffset, yOffset, tileManager.tileTypes.WallList);
            reward = ConstraintReward + (counter * WallConstraintPenalty) + (doorAmount * DoorAmountPenalty);

            // Check if the door is placed on the side with the minimum distance
            if (minDistance == (newTileValue == 0 ? distanceTop : newTileValue == 1 ? distanceRight :
                               newTileValue == 2 ? distanceBottom : distanceLeft))
            {
                // reward for placing the correct door variation based of the rotation
                reward += DoorRotationReward;
            }
            return reward;
        }

        else if (tileName == "Wall")
        {

            Debug.Log("Tile ID: " + newTileValue);
            Debug.Log("coordinates " + " x " + x + " y " + y);
            TileBase tile = tileManager.GetTileFromID(newTileValue);
            Debug.Log("Retrieved Tile: " + (tile != null ? tile.name : "null"));

            xOffset = new int[] { 0, -1, 1, 0 };
            yOffset = new int[] { -1, 0, 0, 1 };

            int counter = CheckNeighborsForTile(x, y, xOffset, yOffset, tileManager.tileTypes.WallList);
            if (counter <= 6)
            {
                return reward += ConstraintReward;
            }
            else
            {
                reward = ConstraintReward + (counter * WallConstraintPenalty);
            }
        }
        return reward;
    }

    private int CheckNeighborsForTile(int x, int y, int[] xOffset, int[] yOffset, List<TileBase> tileList)
    {
        int counter = 0;

        for (int i = 0; i < xOffset.Length; i++)
        {
            int neighborX = x + xOffset[i];
            int neighborY = y + yOffset[i];

            // Check if the neighbor coordinates are within the array bounds
            if (neighborX >= 0 && neighborX < width && neighborY >= 0 && neighborY < height)
            {
                // check for tile to not be in a certain list / not from a certain type
                if (!tileManager.IsTileInList(currentState[neighborX, neighborY], tileList))
                {
                    // counts how many tiles are falsely placed given the constraint
                    counter += 1;
                }
            }
        }
        return counter;
    }

    private float CalculateIndirectReward()
    {
        float totalReward = 0;

        // Reward calculation for Playabilty of the level and smooth tileing
        float playabilityReward = CalculatingPlayabilityReward();
        float tileMatchingReward = CalculatetileMatchingReward();

        totalReward += playabilityReward + tileMatchingReward;
        return totalReward;
    }

    private float CalculatingPlayabilityReward()
    {
        float reward = 0f;

        List<Vector2Int> doorLocations = tileManager.DoorLocations;
        List<Vector2Int> spawnLocations = tileManager.SpawnLocations;
        int doorCount = doorLocations.Count;
        int spawnCount = spawnLocations.Count;

        // Reward calculation for correct ammount of reachable doors from spawn
        if (doorCount == 1 && spawnCount == 1)
        {
            IsReachable(doorLocations, spawnLocations);
            return reward += PlayableReward;
        }
        else
        {
            return reward += NotPlayablePenalty * (Mathf.Abs(doorCount - 1) + Mathf.Abs(spawnCount - 1));
        }
    }

    public bool IsReachable(List<Vector2Int> spawnLocations, List<Vector2Int> doorLocations)
    {
        Pathfinding pathF = new Pathfinding(); 
        //##### Implementation of is reachable and call of A* search
        for (int i = 0; i < doorLocations.Count; i++)
        {
            Vector2Int doorLocation = doorLocations[i];
            Vector2Int spawnLocation = spawnLocations[i];

            if (pathF.ContainsPath(spawnLocation, doorLocation, width, height, currentState))
            {
                return true;
            }
        }
        return false;
    }

    private float CalculatetileMatchingReward()
    {
        float reward = 0f;

        // Define the thickness of the border to check
        int borderThickness = 2;

        // Check the top and bottom borders
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < borderThickness; y++)
            {
                // top
                if (!tileManager.IsWallTile(currentState[x, y]) || !tileManager.IsDoorTile(currentState[x, y]))
                {
                    // Penalize for non-wall tiles in the border
                    reward += WallConstraintPenalty;
                }
                // bottom
                if (!tileManager.IsWallTile(currentState[x, height - 1 - y]) || !tileManager.IsDoorTile(currentState[x, height - 1 - y]))
                {
                    // Penalize for non-wall tiles in the border
                    reward += WallConstraintPenalty;
                }
            }
        }

        // Check the left and right borders (excluding corners to avoid duplicate checks)
        for (int y = borderThickness; y < height - borderThickness; y++)
        {
            for (int x = 0; x < borderThickness; x++)
            {
                // left
                if (!tileManager.IsWallTile(currentState[x, y]) || !tileManager.IsDoorTile(currentState[x, y]))
                {
                    // Penalize for non-wall tiles in the border
                    reward += WallConstraintPenalty;
                }
                // right
                if (tileManager.IsWallTile(currentState[width - 1 - x, y]) || !tileManager.IsDoorTile(currentState[width - 1 - x, y]))
                {
                    // Penalize for non-wall tiles in the border
                    reward += WallConstraintPenalty;
                }
            }
        }

        return reward + TileMatchingReward;
    }

    //reward tuneing for certain tiles


}
