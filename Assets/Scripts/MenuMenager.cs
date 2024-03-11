using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuMenager : MonoBehaviour
{
    public GameObject eventSystemPrefab;
    public GameObject mainMenuPrefab;
    public GameObject pauseMenuPrefab;
    public GameObject EndScreen;
    private GameObject _eventSystemInstance;
    private GameObject _mainMenuInstance;
    private GameObject _pauseMenuInstance;
    private GameObject _EndScreenInstance;
    public static MenuMenager instance = null;

    private string _currentScene = "";

    private void Awake()
    {
        if (instance == null) { instance = this; }
        else if (instance != this) { Destroy(instance); }

        // keep gamemanager for all scenes
        DontDestroyOnLoad(gameObject);
        EnsureEventSystem();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    // Unsubscribing from the LevelGenerated event
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void Update()
    {
        // Check for ESC key press outside the MainMenu scene
        if (_currentScene != "MainMenuScene" && Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }
    }

    private void EnsureEventSystem()
    {
        // Check if there's an existing Event System
        if (_eventSystemInstance == null)
        {
            // Instantiate Event System prefab dynamically
            _eventSystemInstance = Instantiate(eventSystemPrefab);
            DontDestroyOnLoad(_eventSystemInstance);
        }
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _currentScene = scene.name;
    }

    private void TogglePauseMenu()
    {
        if (_pauseMenuInstance == null)
        {
            // Pause menu not instantiated, instantiate it
            _pauseMenuInstance = Instantiate(pauseMenuPrefab);
        }
        else
        {
            // Pause menu already instantiated, toggle its active state
            _pauseMenuInstance.SetActive(!_pauseMenuInstance.activeSelf);
        }
    }
}
