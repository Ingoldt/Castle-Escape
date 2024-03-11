using JetBrains.Rider.Unity.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private MenuMenager _menuManager;

    [Header("Scenes to Load")]
    [SerializeField]
    private string _nextScene = "";

    private void Start()
    {
        _menuManager = GameObject.FindGameObjectWithTag("MenuManager").GetComponent<MenuMenager>();
    }

    public void PlayGame()
    {
        _menuManager.SetLoadingState(true);
        _menuManager.LoadNextScene();
    }

    public void ReturnToMainMenu()
    {
        // Re-enable the main menu objects when returning to the main menu
        // load scene
        SceneManager.LoadScene(_nextScene);
     
    }

    public void QuitGame()
    {
        Debug.Log("Quit");
        Application.Quit();
    }
}
