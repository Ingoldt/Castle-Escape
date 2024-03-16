using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public static MenuManager instance = null;
    public GameController gameControllerScript;
    public GameObject eventSystemPrefab;
    public GameObject mainMenuPrefab;
    public GameObject pauseMenuPrefab;
    public GameObject endScreenPrefab;
    public GameObject loadingScreenPrefab;
    public TextMeshProUGUI loadingText;


    private bool _isLoading = false;
    private GameObject _loadingScreenInstance;
    private GameObject _eventSystemInstance;
    private GameObject _mainMenuInstance;
    private GameObject _pauseMenuInstance;
    private GameObject _EndScreenInstance;

    private bool _isPauseMenuActive = false;
    private string _currentScene = "";

    private void Awake()
    {
        if (instance == null) { instance = this; }
        else if (instance != this) { Destroy(instance); }

        // keep gamemanager for all scenes
        DontDestroyOnLoad(gameObject);
        EnsureEventSystem();

        // instanciate loading screen

        if(loadingScreenPrefab != null && _loadingScreenInstance == null)
        {
            _loadingScreenInstance = Instantiate(loadingScreenPrefab);
            Transform loadScreenTransform = _loadingScreenInstance.transform.Find("LoadScreen");
            loadingText = loadScreenTransform.GetComponentInChildren<TextMeshProUGUI>();
            DontDestroyOnLoad(_loadingScreenInstance);
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
        // Hide the loading screen after the loop
        _loadingScreenInstance.SetActive(false);
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _currentScene = scene.name;
        if (_currentScene == "MainMenuScene" && _mainMenuInstance == null)
        {
            _mainMenuInstance = Instantiate(mainMenuPrefab);
            //_mainMenuInstance.transform.SetParent(transform);
        }
    }

    private void TogglePauseMenu()
    {
        if (_pauseMenuInstance == null)
        {
            // Pause menu not instantiated, instantiate it
            _pauseMenuInstance = Instantiate(pauseMenuPrefab);
            _isPauseMenuActive = true;
        }
        else
        {
            // Pause menu already instantiated, toggle its active state
            _pauseMenuInstance.SetActive(!_pauseMenuInstance.activeSelf);
            _isPauseMenuActive = !_pauseMenuInstance.activeSelf;
        }
    }
}
