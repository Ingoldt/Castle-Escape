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
    private string _levelScene = "TrainingScene";

    public void PlayGame()
    {

        SceneManager.LoadSceneAsync(_levelScene);
    }

    public void QuitGame()
    {
        Debug.Log("Quit");
        Application.Quit();
    }


}
