using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    [SerializeField]
    private EnemyScriptableObject _enemyStats;

    public static event Action<Vector3> OnEnemyDeath; // Event to notify the spawner when an enemy dies

    public int spawnCost;
    public int maxHealth;
    public int damage;
    public float movementSpeed;

    int currentHealth;
    private void Awake()
    {
        if (_enemyStats == null)
        {
            Debug.LogWarning("EnemyScriptableObject not assigned to enemy prefab.");
            return;
        }
        spawnCost = _enemyStats.spawnCost;
        maxHealth = _enemyStats.health;
        damage = _enemyStats.damage;
        movementSpeed = _enemyStats.speed;
    }

    private void Start()
    {
        currentHealth = maxHealth;
    }

    public int GetEnemyCost()
    {
        return _enemyStats.spawnCost;
    }

    public void TakeDamage(int damage)
    {
        // damage enemy
        currentHealth -= damage;

        Debug.Log("Current Heath of  " + currentHealth + " Enemy: " + gameObject);

        // hurt animation 

        // check if enemy died
        if (currentHealth <= 0)
        {

            Die();

        }
    }

    public void Die()
    {
        // Notify listeners (e.g., spawner) that an enemy has died and pass its position
        Debug.Log("Notify spawner that enemy died:  ");
        OnEnemyDeath?.Invoke(transform.position);

        // die animation 
        /*
        GetComponent<Collider2D>().enabled = false;
        this.enabled = false;
        */
        Destroy(gameObject);

    }
}
