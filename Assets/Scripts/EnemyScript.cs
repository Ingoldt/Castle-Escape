using System;
using UnityEngine;

public class EnemyScript : MonoBehaviour
{
    public static event Action<Vector3> OnEnemyDeath;
    public Animator animator;
    [SerializeField] private EnemyScriptableObject _enemyStats;
    [SerializeField] private GameObject[] itemPrefabs;
    private DropableItem[] dropableItems;
    public Transform firerballPrrefab;

    public EnemyScriptableObject.EnemyType myEnemyType;
    public int spawnCost;
    public int maxHealth;
    public int damage;
    public float attackSpeed;
    public float attackRange;
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
        attackSpeed = _enemyStats.attackSpeed;
        attackRange = _enemyStats.attackkRange;
        movementSpeed = _enemyStats.speed;
        myEnemyType = _enemyStats.enemyType;
    }

    private void Start()
    {
        currentHealth = maxHealth;

        dropableItems = new DropableItem[]
        {
            new DropableItem(itemPrefabs[0], 0.3f), // prefab1 with a 30% chance AttackUp
            new DropableItem(itemPrefabs[1], 0.2f), // prefab2 with a 20% chance SpeedUp
            new DropableItem(itemPrefabs[2], 0.5f)  // prefab2 with a 50% chance HealthUp
        };
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
        //play hurt animation
        animator.SetTrigger("Hurt");

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

        // item drops
        // Check if an item should be dropped
        if (UnityEngine.Random.Range(0f, 1f) <= 0.9f) // 20% chance
        {
            // Determine which item to drop based on predefined chances
            float dropChanceSum = 0f;
            foreach (var item in dropableItems)
            {
                dropChanceSum += item.dropChance;
                if (UnityEngine.Random.Range(0f, 1f) <= dropChanceSum)
                {
                    // Instantiate the dropable item prefab at the current position
                    Instantiate(item.itemPrefab, transform.position, Quaternion.identity);
                    break;
                }
            }

        }
        // die animation 
        Destroy(gameObject);

    }

}
