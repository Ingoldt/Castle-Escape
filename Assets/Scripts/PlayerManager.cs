using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private void Awake()
    {
        cameraScript = FindObjectOfType<Camerafollow>();
    }

    // Start is called before the first frame update
    void Start()
    {
        
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
