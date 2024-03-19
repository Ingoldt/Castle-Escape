using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpeedUp : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            // disable collider
            GetComponent<Collider2D>().enabled = false;

            // Attack Stat up
            GameObject player = collider.gameObject;
            PlayerController playerControllerScript = player.GetComponent<PlayerController>();

            // chnage UI
            playerControllerScript.SetMoveSpeed(0.2f);

            // destroy
            Destroy(gameObject);
        }
    }
}
