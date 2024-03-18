using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loader : MonoBehaviour
{
    public GameObject gameControllerPrefab;
    public GameObject menuManagerPrefab;

    // Used for initialization
    void Awake()
    {
        // Check if the prefabs already exist in the scene
        GameObject existingGameController = GameObject.Find(gameControllerPrefab.name + "(Clone)");
        GameObject existingMenuManager = GameObject.Find(menuManagerPrefab.name + "(Clone)");

        // If both prefabs exist, we don't need to instantiate new ones
        if (existingGameController != null && existingMenuManager != null)
        {
            GameController gameController = existingGameController.GetComponent<GameController>();

            if (gameController != null)
            {
                gameController.Initialize();
            }
        }
        else
        {
            // If any of the prefabs don't exist, instantiate them
            if (existingGameController == null && gameControllerPrefab != null)
            {
                Instantiate(gameControllerPrefab);
            }

            if (existingMenuManager == null && menuManagerPrefab != null)
            {
                Instantiate(menuManagerPrefab);
            }
        }
    }
}