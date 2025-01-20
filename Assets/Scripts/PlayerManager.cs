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
        if (playerTransform != null)
            playerPosition = playerTransform.position;
    }

    public GameObject SpawnPlayer(Vector3 spawnPosition)
    {
        if (_playerInstance == null)
        {
            _playerInstance = Instantiate(playerPrefab, spawnPosition, Quaternion.identity);
            DontDestroyOnLoad(_playerInstance);

            // Start invincibility after spawning
            StartCoroutine(GrantInvincibilityAfterSpawn());
        }
        else
        {
            playerTransform.position = spawnPosition;
        }

        playerPosition = spawnPosition;
        playerTransform = _playerInstance.transform;
        cameraScript.SetTarget(playerTransform);

        return _playerInstance;
    }
    private IEnumerator GrantInvincibilityAfterSpawn()
    {
        // Ensure the player script is accessible
        PlayerController playerScript = _playerInstance.GetComponent<PlayerController>();
        if (playerScript != null)
        {
            playerScript.SetInvincible(true); 
            yield return new WaitForSeconds(4f); 
            playerScript.SetInvincible(false); 
        }
    }
}
