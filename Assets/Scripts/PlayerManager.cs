using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour
{
    [SerializeField]
    private GameObject playerPrefab;
    private Transform playerTransform;

    public Vector3 playerPosition;
    public Camerafollow cameraScript;

    public Vector3 GetPlayerPosition()
    {
        return playerPosition;
    }

    // Subscribing to the LevelGenerated event
    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    // Unsubscribing from the LevelGenerated event
    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        cameraScript = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camerafollow>();
    }

    // Update is called once per frame
    void Update()
    {
        // Update the player's position in world coordinates
        if (playerTransform != null)
        {
            playerPosition = playerTransform.position;
        }
    }

    public GameObject SpawnPlayer(Vector3 spawnPosition)
    {
        // Instantiate player at the specified spawn position
        GameObject player = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);

        playerPosition = spawnPosition;
        // Get the Transform component from the spawned player
        playerTransform = player.GetComponent<Transform>();

        cameraScript.SetTarget(playerTransform);

        return player;
    }
}
