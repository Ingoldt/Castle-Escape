using JetBrains.Rider.Unity.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [Header("Main Menu Objects")]
    [SerializeField]
    private GameObject[] _hideObjects;

    [Header("Scenes to Load")]
    [SerializeField]
    private string _nextScene = "";

    public void PlayGame()
    {

        SceneManager.LoadSceneAsync(_nextScene);
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
