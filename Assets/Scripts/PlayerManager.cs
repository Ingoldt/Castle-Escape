using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerManager : MonoBehaviour
{
    [SerializeField]
    private GameObject playerPrefab;
    private GameObject _playerInstance;
    private Transform playerTransform;
    private bool _playerHasKey = false;

    public Vector3 playerPosition;
    public Camerafollow cameraScript;

    public Vector3 GetPlayerPosition()
    {
        return playerPosition;
    }
    public bool GetPlayerHasKey()
    {
        return _playerHasKey;
    }

    public void SetPlayerHasKey(bool value)
    {
        _playerHasKey = value;
    }

    public GameObject GetPlayerInstance()
    {
        return _playerInstance;
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
        _playerInstance = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);

        playerPosition = spawnPosition;
        // Get the Transform component from the spawned player
        playerTransform = _playerInstance.GetComponent<Transform>();

        cameraScript.SetTarget(playerTransform);

        return _playerInstance;
    }
}
