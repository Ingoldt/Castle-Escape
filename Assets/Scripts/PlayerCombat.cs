using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    public Animator animator;
    [SerializeField]
    private PlayerScriptableObject _playerStats;
    private float nexAttackTime;

    public Transform attackPoint;
    public LayerMask enemyLayer;
    public float attackSpeed;
    public float meleeRange;
    public float maxHealth;
    public int damage;

    private void Awake()
    {
        if (_playerStats == null)
        {
            Debug.LogWarning("PlayerScriptableObject not assigned to player prefab.");
            return;
        }
        meleeRange = _playerStats.meleeRange;
        attackSpeed = _playerStats.attackSpeed;
        damage = _playerStats.damage;
        maxHealth = _playerStats.health;
    }

    // Update is called once per frame
    void Update()
    {

        if (Time.time >= nexAttackTime) 
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Attack();
                nexAttackTime = Time.time + 1f / attackSpeed;
            }
        }
    }

    void Attack()
    {
        // player attack animation
        animator.SetTrigger("Attack");

        // detect all enemies in attack range
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, meleeRange, enemyLayer);

        //Damage Enemies
        foreach (Collider2D enemy in hitEnemies)
        {
            enemy.GetComponent<EnemyScript>().TakeDamage(damage);
        }
    }

    /*
    void OnDrawGizmosSelected()
    {
        if (attackPoint == null)
        { 
            Debug.Log("Attack Poit is not assigned to the player");
            return;
        }
        Gizmos.DrawWireSphere(attackPoint.position, meleeRange);
    }
    */
}
