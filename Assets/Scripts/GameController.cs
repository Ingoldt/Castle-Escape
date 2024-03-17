using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameController : MonoBehaviour
{
    private LevelGeneration _levelGenerationScript;
    private TileManager _tileManagerScript;
    private GameObject _agent;
    private Vector3Int _tilemapOrigin;
    private string _currentScene = "";

    [Header("Script Reverences")]
    public GameObject smallAgentPrefab;
    public GameObject mediumAgentPrefab;
    public GameObject largeAgentPrefab;
    public PlayerManager playerManagerScript;
    public MenuManager menuMenagerScript;
    public static GameController instance = null;

    public bool shouldGenerateLevel;

    [Header("Game Data")]
    public int completedLevels;
    public float smallLevelBaseChance = 0.5f;
    public float mediumLevelBaseChance = 0.3f;
    public float largeLevelBaseChance = 0.15f;
    public float chestLevelBaseChance = 0.05f;

    private bool? _lastLevelWasChest = null;
    private float _smallLevelChance;
    private float _mediumLevelChance;
    private float _largeLevelChance;
    private float _chestLevelChance;
    private float _smallLevelChanceMin;
    private float _smallLevelChanceMax;
    private float _mediumLevelChanceMin;
    private float _mediumLevelChanceMax;
    private float _largeLevelChanceMin;
    private float _largeLevelChanceMax;


    // EVENTS
    // Signaling a new level should be generated
    public static event Action<LevelInfoScriptableObject.BaseType> OnGenerateLevel;
    public static event Action<List<Vector3>, Vector3> OnSpawnEnemies;
    public static event Action OnLevelGenrationCompleted;
    public TileManager GetTileManager()
    {
        return _tileManagerScript;
    }

    private void Awake()
    {
        if (instance == null) { instance = this; }
        else if (instance != this) { Destroy(instance); }

        // keep gamemanager for all scenes
        DontDestroyOnLoad(gameObject);

        playerManagerScript = GetComponent<PlayerManager>();
    }

    // Subscribing to the LevelGenerated event
    private void OnEnable()
    {
        LevelGeneratorAgent.OnLevelGenerated += HandleLevelGenerated;
        SceneManager.sceneLoaded += HandleSceneLoaded;
        ChangeSprite.OnKeyUse += HandleKeyUsed;
    }

    // Unsubscribing from the LevelGenerated event
    private void OnDisable()
    {
        LevelGeneratorAgent.OnLevelGenerated -= HandleLevelGenerated;
        SceneManager.sceneLoaded -= HandleSceneLoaded;
        ChangeSprite.OnKeyUse += HandleKeyUsed;
    }

    private void Start()
    {
        menuMenagerScript = GameObject.FindGameObjectWithTag("MenuManager").GetComponent<MenuManager>();

        // Initialize chances based on base chances
        _smallLevelChance = smallLevelBaseChance;
        _mediumLevelChance = mediumLevelBaseChance;
        _largeLevelChance = largeLevelBaseChance;
        _chestLevelChance = chestLevelBaseChance;

    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _currentScene = scene.name;
        // Check if it's a scene where you want to initialize the level
        if (!_currentScene.Equals("MainMenuScene"))
        {
            initializeLevel();
        }
    }

    private void initializeLevel()
    {
        // Calculate the desired base type based on probabilities
        //LevelInfoScriptableObject.BaseType desiredBaseType = CalculateDesiredBaseType();

        // Adjust level probabilities
        //AdjustLevelProbabilities();

        LevelInfoScriptableObject.BaseType desiredBaseType = LevelInfoScriptableObject.BaseType.Small;
        // LevelInfoScriptableObject.BaseType desiredBaseType = LevelInfoScriptableObject.BaseType.Medium;
        // LevelInfoScriptableObject.BaseType desiredBaseType = LevelInfoScriptableObject.BaseType.Large;

        // instanciate agent according to the baseType
        GameObject agentPrefab = null;
        _agent = null;
        switch (desiredBaseType)
        {
            case LevelInfoScriptableObject.BaseType.Small:
                agentPrefab = smallAgentPrefab;
                break;
            case LevelInfoScriptableObject.BaseType.Medium:
                agentPrefab = mediumAgentPrefab;
                break;
            case LevelInfoScriptableObject.BaseType.Large:
                agentPrefab = largeAgentPrefab;
                break;
            default:
                Debug.LogError("Unsupported BaseType" + desiredBaseType);
                return;
        }

        if (_agent == null)
        {
            _agent = Instantiate(agentPrefab);
            LevelGeneratorAgent.ShouldGenerateNewLevel = true;
        }
        else
        {
            Debug.LogWarning("There is allready a agent in the scene");
        }

        _levelGenerationScript = _agent.GetComponent<LevelGeneration>();
        _tileManagerScript = _agent.GetComponent<TileManager>();

        UpdateShouldGenerateLevel();

        Debug.Log("GAME CONTROLLER: shouldGenerateLevel " + shouldGenerateLevel);

        if (shouldGenerateLevel)
        {
            GenerateLevel(desiredBaseType);
            UpdateShouldGenerateLevel();
        }
    }
    // Handle the event when a level is generated
    private void HandleLevelGenerated()
    {
        Debug.Log("Level is generated. Proceed with the game flow.");
        UpdateShouldGenerateLevel();
        if (!shouldGenerateLevel)
        {

            // convert Tilemap positions into World positions
            List<Vector3> spawnPositions = TileMapToWorldPosition(_tileManagerScript.SpawnLocations);

            // spawn player inside the level
            playerManagerScript.SpawnPlayer(spawnPositions[0]);

            List<Vector3> floorPositions = TileMapToWorldPosition(_tileManagerScript.FloorLocations);
            SpawnEnemies(floorPositions);

            // Disable loading screen
            menuMenagerScript.SetLoadingState(false);

            // enable player enemy interaction
            OnLevelGenrationCompleted?.Invoke();
        }
        else
        {
            Debug.LogWarning("GAME CONTROLLER: Generated Level was not playable! Regenerating Level");
            Destroy(_agent);
            Debug.LogWarning("GAME CONTROLLER: leftover agent was destroyed");
            initializeLevel();
        }
    }

    private void GenerateLevel(LevelInfoScriptableObject.BaseType baseType)
    {
        // Trigger the event with the desired base type
        Debug.Log("GAME CONTROLLER: Level should get generated by the Agent");
        OnGenerateLevel?.Invoke(baseType);
    }

    private void SpawnEnemies(List<Vector3> locations)
    {
        Debug.Log("GAME CONTROLLER: Enemies shoud get spawned");
        Vector3 playerPos = playerManagerScript.GetPlayerPosition();
        OnSpawnEnemies?.Invoke(locations, playerPos);
    }

    public List<Vector3> TileMapToWorldPosition(List<Vector2Int> locations)
    {
        List<Vector3> worldPositions = new List<Vector3>();
        _tilemapOrigin = _levelGenerationScript.GetTilemapOrigin;

        foreach (var position in locations)
        {
            worldPositions.Add(new Vector3(_tilemapOrigin.x + 0.5f + position.x, _tilemapOrigin.y + 0.5f + position.y, 0));
        }

        return worldPositions;
    }

    private void HandleKeyUsed()
    {
        Debug.Log("Key was used" + completedLevels);
        // track game specific information (e.g completed levels...)
        completedLevels += 1;

        StartCoroutine(DelayedLoadNextScene());
    }

    private IEnumerator DelayedLoadNextScene()
    {
        // Wait for 2 seconds
        yield return new WaitForSeconds(1f);

        // Load the next scene here
        menuMenagerScript.SetLoadingState(true);
        menuMenagerScript.LoadNextScene();
    }

    // Method to adjust chances based on completed levels
    private void AdjustLevelProbabilities()
    {
        float levelMultiplier = CalculateLevelMultiplier(completedLevels);
        float increaseFactor = Mathf.Abs(levelMultiplier != 0 ? levelMultiplier / 3f : 1f);

        // Decrease Small Level Chance to min of 0.15 over time
        _smallLevelChanceMin = 0;
        _smallLevelChanceMax = Mathf.Max(smallLevelBaseChance - (smallLevelBaseChance * levelMultiplier), 0.15f);
        // Calculate new small level chance
        _smallLevelChance = Mathf.Clamp(smallLevelBaseChance - (smallLevelBaseChance * levelMultiplier), _smallLevelChanceMin, _smallLevelChanceMax);

        _mediumLevelChanceMin = _smallLevelChanceMax + Mathf.Epsilon;
        _mediumLevelChanceMax = Mathf.Min(_mediumLevelChanceMin + (mediumLevelBaseChance * increaseFactor), _mediumLevelChanceMin + 0.45f);
        // Calculate new medium level chance
        _mediumLevelChance = Mathf.Clamp(_mediumLevelChance + (mediumLevelBaseChance * increaseFactor), _mediumLevelChanceMin, _mediumLevelChanceMax + Mathf.Epsilon);

        _largeLevelChanceMin = _mediumLevelChanceMax + Mathf.Epsilon;
        _largeLevelChanceMax = Mathf.Min(_largeLevelChanceMin + (largeLevelBaseChance * increaseFactor), _largeLevelChanceMin + 0.35f);
        // Calculate new large level chance
        _largeLevelChance = Mathf.Clamp(_largeLevelChance + (largeLevelBaseChance * increaseFactor), _largeLevelChanceMin, _largeLevelChanceMax);

        // Adjust cumulative chances for chest 
        if (_lastLevelWasChest == false || _lastLevelWasChest == null)
        {
            // Calculate remaining probability after subtracting the chances of other level types
            float remainingProbability = Mathf.Abs(1.0f - (_smallLevelChance + (1.0f - _mediumLevelChance) + (1.0f - _largeLevelChance)));


            // Increase the chest level chance by 0.05 each time it triggers, up to a maximum of 0.2
            _chestLevelChance = Mathf.Clamp(_chestLevelChance + 0.05f, chestLevelBaseChance, Mathf.Min(chestLevelBaseChance + 0.2f, chestLevelBaseChance + remainingProbability));
        }
        else
        {
            // Reset to base chance
            _chestLevelChance = chestLevelBaseChance;
        }

        // Log the adjusted probabilities
        Debug.Log("Small Level Chance: " + _smallLevelChance);
        Debug.Log("Medium Level Chance: " + _mediumLevelChance);
        Debug.Log("Large Level Chance: " + _largeLevelChance);
        Debug.Log("Chest Level Chance: " + _chestLevelChance);
    }

    // Method to calculate the level multiplier based on completed levels
    private float CalculateLevelMultiplier(int completedLevels)
    {
        // Decrease factor
        return 1.0f - Mathf.Pow(0.1f, completedLevels);
    }

    LevelInfoScriptableObject.BaseType CalculateDesiredBaseType()
    {
        if(completedLevels != 0 && completedLevels % 10 == 0)
        {
            // Return the Boss basetype
            return LevelInfoScriptableObject.BaseType.Boss;
        }


        // Calculate total probability
        float totalProbability = _smallLevelChance + _mediumLevelChance + _largeLevelChance + _chestLevelChance;

        // Generate a random value between 0 and totalProbability
        float randomValue = UnityEngine.Random.Range(0f, totalProbability);

        // Create an array to store the cumulative probabilities
        float[] cumulativeProbabilities = new float[]
        {
        _smallLevelChance,
        _smallLevelChance + _mediumLevelChance,
        _smallLevelChance + _mediumLevelChance + _largeLevelChance,
        totalProbability // Chest level
        };

        // Perform binary search to find the desired base type
        int left = 0;
        int right = cumulativeProbabilities.Length - 1;

        while (left <= right)
        {
            int mid = left + (right - left) / 2;

            if (randomValue < cumulativeProbabilities[mid])
            {
                right = mid - 1;
            }
            else
            {
                left = mid + 1;
            }
        }

        // Return the corresponding base type
        return (LevelInfoScriptableObject.BaseType)left;
    }

    private void UpdateShouldGenerateLevel()
    {
        shouldGenerateLevel = LevelGeneratorAgent.ShouldGenerateNewLevel;
    }


}
