using JetBrains.Rider.Unity.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private MenuManager _menuManager;

    private void Start()
    {
        _menuManager = GameObject.FindGameObjectWithTag("MenuManager").GetComponent<MenuManager>();
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
        _menuManager.SetLoadingState(true);
        _menuManager.LoadNextScene();

    }

    public void QuitGame()
    {
        Debug.Log("Quit");
        Application.Quit();
    }
}
