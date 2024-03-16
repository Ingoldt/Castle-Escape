using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [SerializeField] private PlayerScriptableObject _playerStats;
    public Animator animator;

    [Header("Attacking")]
    private float nexAttackTime;
    public Transform attackPoint;
    public LayerMask enemyLayer;

    [Header("Player Stats")]
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

    public void OnAttack(InputAction.CallbackContext ctxt)
    {
        // Check if it's time for the next attack
        if (Time.time >= nexAttackTime)
        {
            Attack();
            nexAttackTime = Time.time + 1f / attackSpeed;
        }
    }

    void Attack()
    {
        // Player attack animation
        animator.SetTrigger("Attack");

        // Detect all enemies and objects in attack range
        Collider2D[] hitObjects = Physics2D.OverlapCircleAll(attackPoint.position, meleeRange);

        // Damage Enemies and Barrels
        foreach (Collider2D hitObject in hitObjects)
        {
            int layer = hitObject.gameObject.layer;
            string layerName = LayerMask.LayerToName(layer);

            if (layerName == "Enemy")
            {
                hitObject.GetComponent<EnemyScript>().TakeDamage(damage);
            }
            else
            {
                // Check if the collided object is a barrel
                Tilemap tilemap = hitObject.GetComponent<Tilemap>();
                if (tilemap != null)
                {
                    // Get the position of the tile
                    Vector3 hitPosition = hitObject.ClosestPoint(attackPoint.position);
                    Vector3Int tilePosition = tilemap.WorldToCell(hitPosition);

                    // Get the tile at the position
                    TileBase tile = tilemap.GetTile(tilePosition);

                    // Check if the tile exists and its name starts with "barrel"
                    if (tile != null && tile.name.ToLower().Contains("barrel"))
                    {
                        // Barrel is hit
                        hitObject.GetComponent<ChangeSprite>().BarrelTakeDamage(damage, tilePosition, tile.name);
                    }
                }
            }
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
