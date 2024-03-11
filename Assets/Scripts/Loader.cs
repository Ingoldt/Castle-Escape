using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loader : MonoBehaviour
{
    public GameObject gameControllerPrefab;
    public GameObject menuMenagerPrefab;


    // Used for initialization
    void Awake()
    {
        EnsureSingleInstance(gameControllerPrefab);
        EnsureSingleInstance(menuMenagerPrefab);
    }

    private void EnsureSingleInstance(GameObject prefab)
    {
        if (prefab != null)
        {
            GameObject existingInstance = GameObject.Find(prefab.name + "(Clone)");
            if (existingInstance == null)
            {
                Instantiate(prefab);
            }
            else
            {
                Destroy(gameObject);
            }
        }
        else
        {
            Debug.LogError("Prefab is not assigned in the Loader script.");
        }
    }
}

