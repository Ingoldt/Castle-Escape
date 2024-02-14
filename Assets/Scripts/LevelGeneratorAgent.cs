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

    private const float NullTileReward = 0.5f;
    private const float TilePlacementReward = 1f;
    private const float CloseDistancePenalty = -0.5f;
    private const float MediumDistanceReward = 0.1f;
    private const float FarDistanceReward = 0.5f;
    private const float NotPlayablePenalty = -15f;
    private const float PlayableReward = 15f;
    private const float ConstraintReward = 5f;
    private const float WallConstraintPenalty = -1f;
    private const float DoorAmmountPenalty = -1f;


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
        base.MaxStep = (int)(levelGeneration.GetLevelInfo.tileCount * 0.2f + 0.5f);
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

        // replace tile at the given coordinates
        if (levelGeneration.GetLevelInfo != null)
        {
            // check if coordinates are valid
            if (x >= 0 && x < width && y >= 0 && y < height)
            {
                levelGeneration.ReplaceTile(x, y, newTileValue);
                // update visuals
                levelGeneration.UpdateTilemap(x, y);

                //locate all tile types
                tileManager.LocateCategorizeTiles(currentState);
                //reward calculation
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
                    /*
                    //locate all tile types
                    tileManager.LocateCategorizeTiles(currentState);
                    */

                    float indirectReward = CalculateIndirectReward(); // Implement CalculateIndirectReward as needed
                    AddReward(indirectReward); // Add indirect reward during the episode
                    totalIndirectReward += indirectReward;

                    // Combine direct and indirect rewards to get the total episode reward
                    float totalReward = totalDirectReward + totalIndirectReward;

                    // Use totalReward as needed (e.g., for logging or training)

                    EndEpisode();
                    Debug.Log("Episodes completed: " + CompletedEpisodes);
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

    private float GetRemainingStepsRatio()
    {
        // Calculate the remaining steps ratio (between 0 and 1)
        float remainingStepsRatio = Mathf.Clamp01((float)StepCount / MaxStep);

        return remainingStepsRatio;
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

    private float CalculateExplorationReward(int x, int y, int prevX, int prevY, int newTileValue)
    {
        float distance = DistanceToPreviousTile(x, y, prevX, prevY);
        float reward = 0f;

        // reward for placing a tile new Tile
        if (levelGeneration.GetPreviousState[x, y] != tileManager.GetTileFromID(newTileValue))
        {
            if (newTileValue == 0)
            {
                // tile is null
                reward += NullTileReward;
                reward += TileContraints(x, y, newTileValue);
            }
            else
            {
                // tile is not null
                reward += TilePlacementReward;
                reward += TileContraints(x, y, newTileValue);
            }
            //exploring distance
            if (distance < 2)
            {
                reward += CloseDistancePenalty;
            }
            if (distance > 2 && distance < 4)
            {
                reward += MediumDistanceReward;
            }
            if (distance > 4)
            {
                reward += FarDistanceReward;
            }
        }

        else
        {
            reward = 0f;
        }

        return reward;
    }

    private int DistanceToPreviousTile(int x, int y, int prevX, int prevY)
    {
        // calculating the Manhattan distance to previous tile
        return Mathf.Abs(x - prevX) + (y - prevY);
    }

    private float CalculatingPlayabilityReward()
    {
        float reward = 0f;

        List<Vector2Int> doorLocations = tileManager.DoorLocations;
        List<Vector2Int> spawnLocations = tileManager.SpawnLocations;
        int doorCount = tileManager.DoorLocations.Count;
        int spawnCount = tileManager.SpawnLocations.Count;

        // Reward calculation for correct ammount of reachable doors from spawn
        if (doorCount != 1 && spawnCount != 1)
        {
            //IsReachable(doorLocations, spawnLocations);
            reward += NotPlayablePenalty;

        }
        else
        {
            IsReachable(doorLocations, spawnLocations);
            reward += PlayableReward;
        }

        return reward;
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

    private float TileContraints(int x, int y, int newTileValue)
    {
        float reward = 0f;
        int doorAmmount = tileManager.DoorLocations.Count;

        // Calculate Manhattan distances to each side of the grid
        int distanceTop = y;
        int distanceBottom = height - y;
        int distanceLeft = x;
        int distanceRight = width - x;

        // Determine the side with the minimum distance
        int minDistance = Mathf.Min(distanceTop, distanceBottom, distanceLeft, distanceRight);

        // empty tile
        if (newTileValue == 0)
        {
            // should be surroundet by 8 wall tiles
            int[] xOffset = { -1, 0, 1, -1, 1, -1, 0, 1 };
            int[] yOffset = { -1, -1, -1, 0, 0, 1, 1, 1 };
            int counter = CheckNeighborsForTile(x, y, xOffset, yOffset, tileManager.tileTypes.WallList);
            reward = ConstraintReward + (counter * WallConstraintPenalty);
            return reward;
        }

        // door tile top
        if (tileManager.GetTileFromID(newTileValue).name == "Door_Closed_1")
        {
            // should have 5 wall tiles surrounding the top half
            int[] xOffset = { -1, 0, 1, -1, 1, };
            int[] yOffset = { -1, -1, -1, 0, 0, };
            int counter = CheckNeighborsForTile(x, y, xOffset, yOffset, tileManager.tileTypes.WallList);
            reward = ConstraintReward + (counter * WallConstraintPenalty) + (doorAmmount * DoorAmmountPenalty);
            if (minDistance == distanceTop)
            {
                reward += 1f;
            }
            return reward;
        }

        // door tile right
        if (tileManager.GetTileFromID(newTileValue).name == "Door_Closed_2")
        {
            // should have 5 wall tiles surrounding the right half
            int[] xOffset = { 0, 1, 1, 0, 1 };
            int[] yOffset = { -1, -1, 0, 1, 1 };
            int counter = CheckNeighborsForTile(x, y, xOffset, yOffset, tileManager.tileTypes.WallList);
            reward = ConstraintReward + (counter * WallConstraintPenalty) + (doorAmmount * DoorAmmountPenalty);
            if (minDistance == distanceRight)
            {
                reward += 1f;
            }
            return reward;
        }

        // door tile bottom
        if (tileManager.GetTileFromID(newTileValue).name == "Door_Closed_3")
        {
            // should have 5 wall tiles surrounding the bottom half
            int[] xOffset = { -1, 1, -1, 0, 1 };
            int[] yOffset = { 0, 0, 1, 1, 1 };
            int counter = CheckNeighborsForTile(x, y, xOffset, yOffset, tileManager.tileTypes.WallList);
            reward = ConstraintReward + (counter * WallConstraintPenalty) + (doorAmmount * DoorAmmountPenalty);
            if (minDistance == distanceBottom)
            {
                reward += 1f;
            }
            return reward;
        }

        // door tile left
        if (tileManager.GetTileFromID(newTileValue).name == "Door_Closed_4")
        {
            // should have 5 wall tiles surrounding the left half
            int[] xOffset = { -1, 0, -1, -1, 0 };
            int[] yOffset = { -1, -1, 0, 1, 1 };
            int counter = CheckNeighborsForTile(x, y, xOffset, yOffset, tileManager.tileTypes.WallList);
            reward = ConstraintReward + (counter * WallConstraintPenalty) + (doorAmmount * DoorAmmountPenalty);
            if (minDistance == distanceLeft)
            {
                reward += 1f;
            }
            return reward;
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

    private float CalculatetileMatchingReward()
    {
        float reward = 0f;

        return reward;
    }

    // add remaining steps as opservation

    // direct
    // every empty tile needs to be surrounded by wall tiles  (checked)
    // Door rotation matching überprüfen    
    // doors must be surrounded by 3 wall tiles (checked)

    // im äußeren ring mit gewisser thickness dürfen nur walls und oder türen plaziert werden


}
