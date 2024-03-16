using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class GameController : MonoBehaviour
{
    private LevelGeneration _levelGenerationScript;
    private TileManager _tileManagerScript;
    private GameObject _agent;
    private Vector3Int _tilemapOrigin;
    private string _currentScene = "";

    public GameObject smallAgentPrefab;
    public GameObject mediumAgentPrefab;
    public GameObject largeAgentPrefab;
    public PlayerManager playerManagerScript;
    public MenuManager menuMenagerScript;
    public static GameController instance = null;

    public bool shouldGenerateLevel;

    [Header("Game Data")]
    public int completedLevels;



    // Signaling a new level should be generated
    public static event Action<LevelInfoScriptableObject.BaseType> OnGenerateLevel;
    public static event Action<List<Vector3>, Vector3> OnSpawnEnemies;
    public static event Action OnLevelGenrationCompleted;

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
    }

    public TileManager GetTileManager()
    {
        return _tileManagerScript;
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
        /// Logic to vary level sizes depending of how many levels the player completed
        //LevelInfoScriptableObject.BaseType desiredBaseType = LevelInfoScriptableObject.BaseType.Large;
        //LevelInfoScriptableObject.BaseType desiredBaseType = LevelInfoScriptableObject.BaseType.Medium;
        LevelInfoScriptableObject.BaseType desiredBaseType = LevelInfoScriptableObject.BaseType.Small;

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
                Debug.LogError("Unsupported BaseType");
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

    private void UpdateShouldGenerateLevel()
    {
        shouldGenerateLevel = LevelGeneratorAgent.ShouldGenerateNewLevel;
    }
}
