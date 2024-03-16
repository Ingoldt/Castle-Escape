using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using Unity.MLAgents.Actuators;
using static LevelInfoScriptableObject;

public class LevelGeneratorAgent : Agent
{

    // Reference assigned in the Inspector
    [SerializeField]
    private LevelGeneration _levelGeneration;
    [SerializeField]
    private TileManager _tileManager;

    TileBase[,] previousState;
    TileBase[,] currentState;
    private int prevX;
    private int prevY;
    private int height;
    private int width;
    private const float epsilon = 0.3f;
    private float totalDirectReward;

    private const float TilePlacementReward = 1f;
    private const float CloseDistancePenalty = -0.5f;
    private const float MediumDistanceReward = 0.1f;
    private const float FarDistanceReward = 0.5f;
    private const float NotPlayablePenalty = -35f;
    private const float DistrucableReward = 0.6f;
    private const float PlayableReward = 50f;
    private const float ConstraintReward = 5f;
    private const float WallConstraintPenalty = -1.6f;
    private const float DoorAmountPenalty = -1f;
    private const float DoorRotationReward = 0.5f;
    private const float TileMatchingReward = 20f;
    private const float SpawnTilePenalty = -5f;

    private static bool generateNewLevel = true;

    // Define a simple event to notify when a level is generated
    public delegate void LevelGeneratedEventHandler();
    public static event LevelGeneratedEventHandler OnLevelGenerated;

    public static bool ShouldGenerateNewLevel
    {
        get { return generateNewLevel; }
        set { generateNewLevel = value; }
    }

    private void Awake()
    {
        Debug.Log("AGent Genrate level bool: " + generateNewLevel);

        // Access the LevelGeneration and TileManager components on the same GameObject
        _levelGeneration = GetComponent<LevelGeneration>();
        _tileManager = GetComponent<TileManager>();

        if (_levelGeneration == null)
        {
            Debug.LogError("LevelGeneration component not found on the same GameObject as Agent.");
        }

        if (_tileManager == null)
        {
            Debug.LogError("TileManager component not found on the same GameObject as Agent.");
        }
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        GameController.OnGenerateLevel += HandleGenerateLevel;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        GameController.OnGenerateLevel -= HandleGenerateLevel;
    }

    private void HandleGenerateLevel(LevelInfoScriptableObject.BaseType baseType)
    {
        // Handle the event and generate the level with the specified baseType
        Debug.Log("Level Agent handles Level Generation");

        totalDirectReward = 0f;
        _tileManager.ResetState();
        _levelGeneration.CreateLevel(baseType);
        _tileManager.InitializeTileLists(_levelGeneration.GetLevelInfo.InitialState);

        if (_levelGeneration.GetLevelInfo.PublicBaseType == LevelInfoScriptableObject.BaseType.Medium)
        {
            base.MaxStep = Mathf.FloorToInt(_levelGeneration.GetLevelInfo.tileCount * 0.12f);
        }
        else
        {
            base.MaxStep = Mathf.FloorToInt(_levelGeneration.GetLevelInfo.tileCount * 0.10f);
        }
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        previousState = _levelGeneration.GetPreviousState;
        currentState = _levelGeneration.GetCurrentState;

        // Flatten the 2d array into a 1D array and add it to the observation
        FlattenObservations(sensor, previousState);
        FlattenObservations(sensor, currentState);

        sensor.AddObservation(GetRemainingStepsRatio());
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        if (generateNewLevel)
        {
            // read action from action buffer
            int x;
            int y;
            int newTileValue;

            // Exploration-exploitation strategy
            if (Random.value < epsilon)
            {
                // Exploration: Choose a random action excluding a 2x2 thick layer from the borders (walls)
                x = Random.Range(3, width - 2);
                y = Random.Range(3, height - 2);
                // excluding spawn and door tiles
                newTileValue = Random.Range(6, _tileManager.GetTileTypeDictionaryCount());
            }
            else
            {
                // Exploitation: Choose the action with the highest estimated value
                x = (int)actions.DiscreteActions[0];
                y = (int)actions.DiscreteActions[1];
                newTileValue = (int)actions.DiscreteActions[2];
            }

            // replace tile at the given coordinates
            if (_levelGeneration.GetLevelInfo != null)
            {
                // check if coordinates are valid
                if (x >= 0 && x < width && y >= 0 && y < height)
                {
                    _levelGeneration.ReplaceTile(x, y, newTileValue);
                    // update visuals
                    //_levelGeneration.UpdateTilemapTraining(x, y);

                    // locate all tile types and update tile lists
                    _tileManager.UpdateTileLists(x, y, _levelGeneration.GetCurrentState, _levelGeneration.GetPreviousState);

                    // direct reward calculation
                    float directReward = CalculateExplorationReward(x, y, prevX, prevY, newTileValue);
                    AddReward(directReward);
                    totalDirectReward += directReward;

                    // Save current action's coordinates as previous action
                    // needed for poximity check
                    prevX = x;
                    prevY = y;

                    // Check if the episode is about to end
                    if (StepCount == MaxStep)
                    {
                        float indirectReward = CalculateIndirectReward();
                        AddReward(indirectReward); // Add indirect reward during the episode
                        // build level after agent is finished
                        _levelGeneration.UpdateTilemap(currentState);

                        Debug.Log("generateNewLevel flag: " + generateNewLevel);
                        EndEpisode();
                        // Trigger a event when the level is generated
                        OnLevelGenerated?.Invoke();
                    }
                }
            }
        }      
    }

    // Flatten the Tilemap for the Agent's Observation
    private void FlattenObservations(VectorSensor sensor, TileBase[,] tilemap)
    {
        width = _levelGeneration.GetLevelInfo.width;
        height = _levelGeneration.GetLevelInfo.height;
        //Debug.Log("Level: "+ _levelGenerationScript.GetLevelInfo.name + "height: " + height + "Width: " + width + "Variation: " + _levelGenerationScript.GetLevelInfo.PublicVariation);
        
        
        if (width != 0 && height != 0)
        {
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    TileBase tile = tilemap[x, y];
                    int tileValue = _tileManager.GetTileValue(tile); // Convert TileBase to an integer value
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

        if (_levelGeneration.GetPreviousState[x, y] != _tileManager.GetTileFromID(newTileValue))
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
                // Spawn Tile
                return reward += TileConstraints(x, y, newTileValue);
            }
            else if (newTileValue == 5)
            {
                // wall tiles
                // more close together placement of wall tiles
                return reward += TileConstraints(x, y, newTileValue);
            }
            else if (_tileManager.IsDestructibleTile(_tileManager.GetTileFromID(newTileValue)))
            {
                // barrel
                return reward += DistrucableReward;
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
        TileBase tile = _tileManagerScript.GetTileFromID(newTileValue);
        Debug.Log("Tile ID: " + newTileValue + " Tile: " + tile);
        Debug.Log("coordinates " + " x " + x + " y " + y);
        */

        int doorAmount = _tileManager.DoorLocations.Count;
        int SpawnAmount = _tileManager.SpawnLocations.Count;

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

        // want to set spawn tile while spawn tiles are at max amount
        if (newTileValue == 4)
        {
            if (SpawnAmount >= 1)
            {
                return reward = SpawnAmount * SpawnTilePenalty;
            }
        }

        // direct not playable penalty
        else if (doorAmount > 1 || SpawnAmount > 1)
        {
            reward += doorAmount * DoorAmountPenalty + SpawnAmount * SpawnTilePenalty;
        }

        // door tile top, right, bottom, left
        else if (newTileValue <= 3)
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

            int counter = CheckNeighborsForTile(x, y, xOffset, yOffset, _tileManager.tileTypes.WallList);
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

        else if (newTileValue == 5)
        {
            xOffset = new int[] { 0, -1, 1, 0 };
            yOffset = new int[] { -1, 0, 0, 1 };

            if ((x == 2 || x == width - 3) && (y >= 2 && y <= height - 3) || (x >= 2 && x <= width - 3) && (y == 2 || y == height - 3))
            {                           
                return reward = ConstraintReward * -0.5f;
            }
            else
            {            
                int counter = CheckNeighborsForTile(x, y, xOffset, yOffset, _tileManager.tileTypes.WallList);
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
                if (!_tileManager.IsTileInList(currentState[neighborX, neighborY], tileList))
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

        List<Vector2Int> doorLocations = _tileManager.DoorLocations;
        List<Vector2Int> spawnLocations = _tileManager.SpawnLocations;
        int doorCount = doorLocations.Count;
        int spawnCount = spawnLocations.Count;
        //Debug.Log("Door Amount: " + doorCount + ", Spawn AMount: " + spawnCount);

        // Reward calculation for correct ammount of reachable doors from spawn
        if (doorCount == 1 && spawnCount == 1)
        {
            if(IsReachable(doorLocations, spawnLocations))
            {
                _levelGeneration.GetLevelInfo.playability = true;
                generateNewLevel = false;
                Debug.Log("Level is playable Agent achieve it's goal");
                Debug.Log("Agent Script shouldGenerateLevel: " + generateNewLevel);
                return reward += PlayableReward;
            }
            return reward;
        }
        else
        {
            _levelGeneration.GetLevelInfo.playability = false;
            generateNewLevel = true;
            Debug.Log("Level is not playable, Reward: " + reward + NotPlayablePenalty);
            Debug.Log("Agent Script shouldGenerateLevel: " + generateNewLevel);
            return reward += NotPlayablePenalty;
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
                if (!_tileManager.IsWallTile(currentState[x, y]) && !_tileManager.IsDoorTile(currentState[x, y]))
                {
                    // Penalize for non-wall tiles in the border
                    reward += WallConstraintPenalty * 2;
                }
                // bottom
                if (!_tileManager.IsWallTile(currentState[x, height - 1 - y]) && !_tileManager.IsDoorTile(currentState[x, height - 1 - y]))
                {
                    // Penalize for non-wall tiles in the border
                    reward += WallConstraintPenalty * 2;
                }
            }
        }

        // Check the left and right borders (excluding corners to avoid duplicate checks)
        for (int y = borderThickness; y < height - borderThickness; y++)
        {
            for (int x = 0; x < borderThickness; x++)
            {
                // left
                if (!_tileManager.IsWallTile(currentState[x, y]) && !_tileManager.IsDoorTile(currentState[x, y]))
                {
                    // Penalize for non-wall tiles in the border
                    reward += WallConstraintPenalty * 2;
                }
                // right
                if (_tileManager.IsWallTile(currentState[width - 1 - x, y]) && !_tileManager.IsDoorTile(currentState[width - 1 - x, y]))
                {
                    // Penalize for non-wall tiles in the border
                    reward += WallConstraintPenalty * 2;
                }
            }
        }

        return reward + TileMatchingReward;
    }
}
