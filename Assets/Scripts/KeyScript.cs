using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeyScript : MonoBehaviour
{
    private GameObject _player;
    private bool isFollowing = false;

    public static event Action OnPlayerHasKey;
    public Vector3 offset;
    public float dampingFactor = 4;

    private void OnEnable()
    {
        ChangeSprite.OnKeyUse += HandleKeyUsed;
    }

    private void OnDisable()
    {
        ChangeSprite.OnKeyUse -= HandleKeyUsed;
    }

    private void Start()
    {
        // Get the player instance from the PlayerManager
        _player = GameController.instance.playerManagerScript.GetPlayerInstance();
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

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            // Start following the player
            GameController.instance.playerManagerScript.SetPlayerHasKey(true);
            isFollowing = true;

            // Disable the key's collider so it doesn't interfere with the player
            GetComponent<Collider2D>().enabled = false;

            OnPlayerHasKey?.Invoke();
        }
    }

    private void HandleKeyUsed()
    {
        // key was used to open door 
        Destroy(gameObject);
    }
}
