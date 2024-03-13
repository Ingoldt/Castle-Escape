using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyScript : MonoBehaviour
{
    private GameObject _player;
    private bool isFollowing = false;

    public Vector3 offset;
    public float dampingFactor = 4;

    private void Start()
    {
        // Get the player instance from the PlayerManager
        _player = GameController.instance.playerManagerScript.GetPlayerInstance();

        if (_player != null)
        {
            // Associate the key with the player
            SetPlayer(_player);
        }
        else
        {
            Debug.LogError("Player instance is not set!");
        }
    }

    private void Update()
    {
        if (isFollowing && _player != null) 
        {
            // Update the key's position to follow the player with damping
            Vector3 targetPosition = _player.transform.position + offset;
            transform.position = Vector3.Lerp(transform.position, targetPosition, dampingFactor * Time.deltaTime);
        }
    }

    // Method to associate the key with the player
    private void SetPlayer(GameObject player)
    {

    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            // Start following the player
            isFollowing = true;

            // Disable the key's collider so it doesn't interfere with the player
            GetComponent<Collider2D>().enabled = false;
        }
    }
}
