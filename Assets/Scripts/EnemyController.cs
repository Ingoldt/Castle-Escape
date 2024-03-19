using UnityEngine;
using UnityEngine.Tilemaps;


public class EnemyController : MonoBehaviour
{
    public enum EnemyState
    {
        Idle,
        Chase,
        Attack
    }

    public Animator animator;
    public EnemyScript enemyScript;
    public GameObject player;
    public Tilemap tilemap;
    public LayerMask obstacleLayer;
    public LayerMask playerObstacleLayerLayer;
    public LayerMask playerLayer;
    public Transform attackPoint;

    private EnemyState currentState;

    // Target position for movement
    private Vector3 targetPosition;
    private Vector3 previousPosition;
    private float idleTimer;
    private float nextAttackTime;
    private bool _facingRight;

    public float idleDuration = 3f;
    public float wanderRadius = 5f;

    // Stats
    public float movementSpeed = 1f;


    float raycastOffset = 0.1f;
    public float steerForce = 0.2f;

    void Start()
    {
        player = GameController.instance.playerManagerScript.GetPlayerInstance();
        tilemap = GameController.instance.levelGenerationScript.GetComponentInChildren<Tilemap>();
        enemyScript = GetComponent<EnemyScript>();

        // Start in the idle state
        idleTimer = idleDuration;
        currentState = EnemyState.Idle;

        previousPosition = transform.position;

        _facingRight = true;
        movementSpeed = 1f;
        idleDuration = 3f;
        wanderRadius = 5f;
    }

    void Update()
    {
        // Perform actions based on the current state
        switch (currentState)
        {
            case EnemyState.Idle:
                IdleState();
                break;
            case EnemyState.Chase:
                ChaseState();
                break;
            case EnemyState.Attack:
                AttackState();
                break;
        }
        //AvoidObstacles();
    }

    void IdleState()
    {

        // Reduce idle timer
        idleTimer -= Time.deltaTime;

        if (idleTimer <= 0f || Vector3.Distance(transform.position, targetPosition) < 0.4f)
        {
            // Generate a new random target position
            targetPosition = GetRandomPositionWithinBounds();

            // Reset idle timer
            idleTimer = idleDuration;
        }

        // Draw a line from previous position to current position
        Debug.DrawLine(previousPosition, transform.position, Color.magenta);
        MoveTowards(targetPosition, movementSpeed);

        // Update the previous position
        previousPosition = transform.position;


        // Fire a ray towards the player's position
        Vector3 playerDirection = (player.transform.position - transform.position).normalized;
        RaycastHit2D playerHit = Physics2D.Raycast(transform.position, playerDirection, 20f, playerObstacleLayerLayer);

        // Draw the ray
        Debug.DrawRay(transform.position, playerDirection * playerHit.distance, Color.blue);



        // Check if the ray hits the player
        if (playerHit.collider != null && playerHit.collider.gameObject == player)
        {
            ChangeState(EnemyState.Chase);
        }

        // Draw a target position marker
        Debug.DrawLine(transform.position + Vector3.left * 0.5f, transform.position + Vector3.right * 0.5f, Color.red);
        Debug.DrawLine(transform.position + Vector3.up * 0.5f, transform.position + Vector3.down * 0.5f, Color.red);
    }



    void ChaseState()
    {
        // Implement chase state behavior here
        // Chase the player

        // Calculate the direction from the enemy to the player
        Vector3 playerDirection = (player.transform.position - transform.position).normalized;

        // Move the enemy towards the player's position
        MoveTowards(transform.position + playerDirection, enemyScript.movementSpeed);

        // Check if the enemy is within attack range of the player
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        if (distanceToPlayer <= enemyScript.attackRange + 2)
        {
            // Transition to the attack state
            ChangeState(EnemyState.Attack);
        }

    }

    void AttackState()
    {
        // Implement attack state behavior here
        // Attack the player if close enough

        // Calculate the direction from the enemy to the player
        Vector3 playerDirection = (player.transform.position - transform.position).normalized;

        // Move the enemy towards the player's position while maintaining some distance
        Vector3 desiredPosition = player.transform.position - playerDirection * (enemyScript.attackRange - 1f);
        MoveTowards(desiredPosition, enemyScript.movementSpeed);

        // Check if the enemy is within attack range of the player
        float distanceToPlayer = Vector3.Distance(transform.position, player.transform.position);
        if (distanceToPlayer <= enemyScript.attackRange)
        {
            // Check if it's time for the next attack
            if (Time.time >= nextAttackTime)
            {
                Attack();
                nextAttackTime = Time.time + 1f / enemyScript.attackSpeed;
            }
        }

        // Check if the enemy is too far away from the player
        float maxDistance = enemyScript.attackRange * 2f; // Adjust this value as needed
        if (distanceToPlayer > maxDistance)
        {
            // Transition back to the chase state
            ChangeState(EnemyState.Chase);
        }
    }


    // Method to switch states
    void ChangeState(EnemyState newState)
    {
        currentState = newState;
    }

    public void MoveTowards(Vector3 targetPosition, float speed)
    {
        // Calculate the direction to the target position
        Vector3 direction = (targetPosition - transform.position).normalized;

        // Move the enemy towards the target position with the specified speed
        transform.position += direction * speed * Time.deltaTime;

        // Check if the enemy is moving right and facing left, or moving left and facing right
        if (direction.x > 0 && !_facingRight || direction.x < 0 && _facingRight)
        {
            // Flip the enemy
            Flip();
        }
    }

    void Flip()
    {
        // Switch the way the enemy is labelled as facing.
        _facingRight = !_facingRight;

        // Multiply the enemy's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }

    Vector3 GetRandomPositionWithinBounds()
    {
        BoundsInt bounds = tilemap.cellBounds;
        Vector3 minWorldPos = tilemap.CellToWorld(bounds.min);
        Vector3 maxWorldPos = tilemap.CellToWorld(bounds.max);

        Vector2 randomDir = Random.insideUnitCircle.normalized;
        float randomDistance = Random.Range(1f, wanderRadius);

        // Calculate the random position within the wander radius
        Vector3 randomPosition = transform.position + new Vector3(randomDir.x, randomDir.y, 0f) * randomDistance;

        // Clamp the random position to ensure it stays within the bounds of the Tilemap
        randomPosition.x = Mathf.Clamp(randomPosition.x, minWorldPos.x, maxWorldPos.x);
        randomPosition.y = Mathf.Clamp(randomPosition.y, minWorldPos.y, maxWorldPos.y);

        return randomPosition;
    }

    void AvoidObstacles()
    {

        Vector3 accumulatedSteerDirection = Vector3.zero;

        // Get raycast directions
        Vector2[] directions = GetRaycastDirections();

        // Fire rays in all directions to detect obstacles
        foreach (Vector2 direction in directions)
        {
            // Calculate raycast origin
            Vector2 rayOrigin = CalculateRaycastOrigin(direction, raycastOffset);

            // Cast a ray to check for obstacles
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, direction, 0.5f, obstacleLayer);
            if (hit.collider != null)
            {
                // Obstacle detected, accumulate steering force
                Debug.Log("Obstacle detected! Collider: " + hit.collider.gameObject.name + ", Distance: " + hit.distance);

                // Visualize the ray and hit point
                Debug.DrawRay(rayOrigin, direction * hit.distance, Color.red);
                Gizmos.color = Color.red;

                Vector2 normal = hit.normal;

                // Calculate the steering force to guide the enemy away from the obstacle
                Vector3 steerDirection = Vector3.Cross(normal, Vector3.forward).normalized;

                // Accumulate the steering force
                accumulatedSteerDirection += steerDirection;
            }
            else
            {
                // No obstacle detected, visualize the ray
                Debug.DrawRay(rayOrigin, direction * 0.5f, Color.green);
            }
        }

        // Smoothly blend between the original direction and accumulated steering force
        Vector3 newDirection = Vector3.Lerp(Vector3.zero, accumulatedSteerDirection, steerForce * Time.deltaTime);

        // Apply the new movement direction to the enemy's movement
        MoveTowards(transform.position + newDirection, movementSpeed);
    }

    private Vector2 CalculateRaycastOrigin(Vector2 direction, float offset)
    {
        CapsuleCollider2D capsuleCollider = GetComponent<CapsuleCollider2D>();
        float capsuleRadiusX = capsuleCollider.size.x * 0.5f; // Half of the width
        float capsuleRadiusY = capsuleCollider.size.y * 0.5f; // Half of the height
        Vector2 position2D = new Vector2(transform.position.x, transform.position.y);

        // Calculate the offset in the direction perpendicular to the ray
        Vector2 perpendicularOffset = Vector2.Perpendicular(direction).normalized * capsuleRadiusX;

        // Calculate parallel lines at the farthest points of the collider on each side
        Vector2 farthestPoint1 = position2D + perpendicularOffset + direction * capsuleRadiusY;
        Vector2 farthestPoint2 = position2D - perpendicularOffset + direction * capsuleRadiusY;

        // Calculate the middle point between the parallel lines
        Vector2 middlePoint = (farthestPoint1 + farthestPoint2) * 0.5f;

        return middlePoint + direction * offset;
    }

    void Attack()
    {
        // attack animation
        animator.SetTrigger("Attack");

        if (enemyScript.myEnemyType == EnemyScriptableObject.EnemyType.Knight)
        {
            // Detect player in attack range
            Collider2D[] hitPlayer = Physics2D.OverlapCircleAll(attackPoint.position, enemyScript.attackRange, playerLayer);

            // Damage Enemies and Barrels
            foreach (Collider2D hit in hitPlayer)
            {
                Debug.Log("Enemy hit: " + hit.name);
                hit.GetComponent<PlayerCombat>().TakeDamage(enemyScript.damage);
            }
        }

        else if (enemyScript.myEnemyType == EnemyScriptableObject.EnemyType.Mage)
        {
            // instantiate project
            Transform projectileTransform = Instantiate(enemyScript.firerballPrrefab, attackPoint.position, Quaternion.identity);
            // shoot projectile in direction of player 
            Vector3 shootDir = (player.transform.position - attackPoint.position).normalized;
            projectileTransform.GetComponent<Fireball>().Setup(shootDir);
        }

    }


    private Vector2[] GetRaycastDirections()
    {
        return new Vector2[]
        {
        Vector2.up,
        Vector2.right,
        Vector2.down,
        Vector2.left,
        new Vector2(1, 1).normalized,    // Up-right diagonal
        new Vector2(-1, 1).normalized,   // Up-left diagonal
        new Vector2(1, -1).normalized,   // Down-right diagonal
        new Vector2(-1, -1).normalized   // Down-left diagonal
        };
    }

    void OnDrawGizmosSelected()
    {
        if (tilemap != null)
        {
            // Get the bounds of the Tilemap in grid coordinates
            BoundsInt bounds = tilemap.cellBounds;

            // Convert grid coordinates to world coordinates
            Vector3 minWorldPos = tilemap.CellToWorld(bounds.min);
            Vector3 maxWorldPos = tilemap.CellToWorld(bounds.max);

            // Draw wireframe bounds
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube((maxWorldPos + minWorldPos) / 2f, maxWorldPos - minWorldPos);
        }

        if (attackPoint == null)
        {
            Debug.Log("Attack Poit is not assigned to the player");
            return;
        }
        Gizmos.DrawWireSphere(attackPoint.position, 0.6f);


    }


}
