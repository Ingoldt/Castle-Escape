using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loader : MonoBehaviour
{
    public GameObject gameControllerPrefab; 

    // Used for initialization
    void Awake()
    {
        if (gameControllerPrefab != null)
        {
            Instantiate(gameControllerPrefab);
        }
        else
        {
            Debug.LogError("GameController prefab is not assigned in the Loader script.");
        }
    }
}
