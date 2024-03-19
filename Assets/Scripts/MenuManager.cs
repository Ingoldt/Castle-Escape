using System.Collections;
using TMPro;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public static MenuManager instance = null;
    public GameController gameControllerScript;
    public GameObject eventSystemPrefab;
    public GameObject playerUIPrefab;
    public GameObject mainMenuPrefab;
    public GameObject pauseMenuPrefab;
    public GameObject deathScreenPrefab;
    public GameObject loadingScreenPrefab;
    public TextMeshProUGUI loadingText;


    private bool _isLoading = false;
    private GameObject _loadingScreenInstance;
    private GameObject _eventSystemInstance;
    private GameObject _mainMenuInstance;
    private GameObject _pauseMenuInstance;
    private GameObject _deathScreenInstance;
    private GameObject _playerUIInstance;

    private bool _isPauseMenuActive = false;
    private bool _isDeathScreenActive = false;
    private string _currentScene = "";
    public GameObject GetPlayerUIInstance()
    {
        return _playerUIInstance;
    }
    private void Awake()
    {
        if (instance == null) { instance = this; }
        else if (instance != this) { Destroy(instance); }

        // keep gamemanager for all scenes
        DontDestroyOnLoad(gameObject);
        EnsureEventSystem();

        // instanciate loading screen

        if (loadingScreenPrefab != null && _loadingScreenInstance == null)
        {
            _loadingScreenInstance = Instantiate(loadingScreenPrefab);
            Transform loadScreenTransform = _loadingScreenInstance.transform.Find("LoadScreen");
            loadingText = loadScreenTransform.GetComponentInChildren<TextMeshProUGUI>();
            DontDestroyOnLoad(_loadingScreenInstance);
        }

        // Instantiate player UI
        if (playerUIPrefab != null && _playerUIInstance == null)
        {
            _playerUIInstance = Instantiate(playerUIPrefab);
            DontDestroyOnLoad(_playerUIInstance);
        }
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

    private void Start()
    {
        gameControllerScript = GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();
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

    public void SetLoadingState(bool isLoading)
    {
        _isLoading = isLoading;
    }

    public void LoadNextScene()
    {
        string _nextScene = "";

        if (_isPauseMenuActive)
        {
            // If pause menu is active, return to main menu
            _nextScene = "MainMenuScene";
            TogglePauseMenu();
        }
        else if(_isDeathScreenActive)
        {
            // If pause menu is active, return to main menu
            _nextScene = "MainMenuScene";
            ToggleDeathScreen();

        }
        else
        {
            // Otherwise, determine the next scene based on the current scene
            switch (_currentScene)
            {
                case "MainMenuScene":
                    _nextScene = "LevelScene";
                    break;
                case "LevelScene":
                    _nextScene = "NextLevelScene";
                    break;
                case "NextLevelScene":
                    _nextScene = "LevelScene";
                    break;
            }
        }

        StartCoroutine(LoadSceneAsync(_nextScene));
    }

    IEnumerator LoadSceneAsync(string _nextScene)
    {
        SceneManager.LoadSceneAsync(_nextScene);

        // disable playerUI
        TogglePlayerUI(false);

        // enable loading screen
        _loadingScreenInstance.SetActive(true);

        while (_isLoading)
        {
            for (int i = 0; i < 3; i++) // Adjust the number of dots as needed
            {
                loadingText.text = "Loading" + new string('.', i + 1);
                yield return new WaitForSeconds(0.5f); // Adjust the time between dots as needed
            }
            loadingText.text = "Loading";
        }

        // enable playerUI
        if (_nextScene != "MainMenuScene")
        {
            TogglePlayerUI(true);
        }

        // Hide the loading screen after the loop
        _loadingScreenInstance.SetActive(false);
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _currentScene = scene.name;
        if (_currentScene == "MainMenuScene" && _mainMenuInstance == null)
        {
            _mainMenuInstance = Instantiate(mainMenuPrefab);
        }
    }

    private void TogglePlayerUI(bool enable)
    {
        if (_playerUIInstance == null)
        {
            Instantiate(_playerUIInstance);
            return;
        }
        _playerUIInstance.SetActive(enable);
    }

    public void TogglePauseMenu()
    {
        if (_pauseMenuInstance == null)
        {
            // Pause menu not instantiated, instantiate it
            _pauseMenuInstance = Instantiate(pauseMenuPrefab);
            _isPauseMenuActive = true;
            Time.timeScale = 0;
            Debug.Log("Pause Menu Instantiated");
        }
        else
        {
            // Pause menu already instantiated, toggle its active state
            _isPauseMenuActive = !_isPauseMenuActive;
            _pauseMenuInstance.SetActive(_isPauseMenuActive);

            // Set game state when continuing
            Time.timeScale = _isPauseMenuActive ? 0 : 1;

            Debug.Log("Pause Menu Toggled");
        }
    }

    public void ToggleDeathScreen()
    {
        if (_deathScreenInstance == null)
        {
            // Instantiate the death screen prefab if it's not instantiated already
            _deathScreenInstance = Instantiate(deathScreenPrefab);
            _isDeathScreenActive = true;
            Time.timeScale = 0;
        }
        else
        {
            // Toggle the visibility of the death screen
            _isDeathScreenActive = !_isDeathScreenActive;
            _deathScreenInstance.SetActive(_isDeathScreenActive);
            Time.timeScale = _isDeathScreenActive ? 0 : 1;
        }
    }
}
