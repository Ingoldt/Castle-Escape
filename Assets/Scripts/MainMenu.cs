using UnityEngine;

public class MainMenu : MonoBehaviour
{
    private MenuManager _menuManager;

    private void Start()
    {
        _menuManager = GameController.instance.menuMenagerScript;
    }

    public void PlayGame()
    {
        _menuManager.SetLoadingState(true);
        _menuManager.LoadNextScene();
    }

    public void ResumeGame()
    {
        _menuManager.TogglePauseMenu();
    }

    public void ReturnToMainMenu()
    {
        // make everything destroyable that is DontDestroyOnLoad

        _menuManager.SetLoadingState(true);
        _menuManager.LoadNextScene();
    }

    public void QuitGame()
    {
        Debug.Log("Quit");
        Application.Quit();
    }
}
