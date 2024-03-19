using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthUp : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.CompareTag("Player"))
        {
            // disable collider
            GetComponent<Collider2D>().enabled = false;

            // Attack Stat up
            GameObject player = collider.gameObject;
            PlayerCombat playerCombatScript = player.GetComponent<PlayerCombat>();

            // chnage UI
            playerCombatScript.SetCurrentHealth(10);

            // destroy
            Destroy(gameObject);
        }
    }
}
