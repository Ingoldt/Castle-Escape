using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Range(0, .3f)][SerializeField] private float m_MovementSmoothing = .05f;
    public Animator animator;
    public Rigidbody2D rb2D;
    public PlayerUI playerUI;
    public TrailRenderer trailRenderer;

    [Header("Player Stats")]
    [SerializeField] private PlayerScriptableObject _playerStats;
    public float moveSpeed;
    public bool isInvincible;

    [Header("Dashing")]
    [SerializeField] private Cooldown cooldown;
    public float dashingVelocity = 45f;
    public float dashDuration = 0.4f;

    [Header("Movement")]
    private bool _facingRight = true;
    private Vector2 _velocity = Vector2.zero;
    Vector3 moveDir = Vector3.zero;
    float totalMovement;

    private void Start()
    {
        // get playerui element from ui manager
        playerUI = GameController.instance.menuMenagerScript.GetPlayerUIInstance().GetComponentInChildren<PlayerUI>();

        trailRenderer = GetComponent<TrailRenderer>();
        moveSpeed = _playerStats.movementSpeed;
        moveDir = Vector2.zero;
        isInvincible = _playerStats.isInvincible;
        playerUI.SetMaxStamina(1f);
        playerUI.SetSpeedValue(moveSpeed);
    }

    private void Update()
    {
        // Calculate the total movement magnitude
        totalMovement = moveDir.magnitude;

        animator.SetFloat("Speed", Mathf.Abs(totalMovement));
    }

    private void FixedUpdate()
    {
        Move();
    }

    public void SetMoveSpeed(float value)
    {
        moveSpeed += value;
        playerUI.SetSpeedValue(moveSpeed);
    }

    public void OnMove(InputAction.CallbackContext ctxt)
    {
        moveDir = ctxt.ReadValue<Vector2>().normalized;
    }

    public void Move()
    {
        // Move the character by finding the target velocity
        Vector2 targetVelocity = moveDir * moveSpeed;

        // Smoothly adjust the character's velocity
        rb2D.velocity = Vector2.SmoothDamp(rb2D.velocity, targetVelocity, ref _velocity, m_MovementSmoothing);

        // If the input is moving the player right and the player is facing left...
        if (moveDir.x > 0 && !_facingRight)
        {
            // ... flip the player.
            Flip();
        }
        // Otherwise if the input is moving the player left and the player is facing right...
        else if (moveDir.x < 0 && _facingRight)
        {
            // ... flip the player.
            Flip();
        }
    }

    void Flip()
    {
        // Switch the way the player is labelled as facing.
        _facingRight = !_facingRight;

        // Multiply the player's x local scale by -1.
        Vector3 theScale = transform.localScale;
        theScale.x *= -1;
        transform.localScale = theScale;
    }


    public void OnDash(InputAction.CallbackContext ctxt)
    {
        if (cooldown.IsCoolingDown) return;

        Dash();
        StartCoroutine(DashCooldown());
    }

    void Dash()
    {
        // Player dash animation
        animator.SetTrigger("Dash");
        trailRenderer.emitting = true;

        // Set stamina to 0 when dashing
        playerUI.SetStamina(0f);

        // Player is invincible during dash
        _playerStats.SetInvincible(true);

        // Ignore collisions between Player layer and Enemy layer
        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Enemy"), true);

        Vector2 dashDirection = moveDir;

        // If not moving, dash in the direction the player is facing
        if (dashDirection == Vector2.zero)
        {
            dashDirection.x = _facingRight ? 1 : -1;
        }
        Debug.Log("Dash Direction: " + dashDirection);

        rb2D.velocity = dashDirection.normalized * dashingVelocity;
        Debug.Log("Dash Velocity: " + rb2D.velocity);

        StartCoroutine(StopDashing());
    }

    private IEnumerator StopDashing()
    {
        yield return new WaitForSeconds(dashDuration);
        Debug.Log("Dash ended");
        trailRenderer.emitting = false;
        rb2D.velocity = Vector2.zero;

        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Enemy"), false);

        // Player is invincible during dash
        _playerStats.SetInvincible(false);
    }
    private IEnumerator DashCooldown()
    {
        // Start cooldown timer
        cooldown.StartCooldown();

        float initialMaxStamina = playerUI.staminaSlider.maxValue;
        playerUI.SetMaxStamina(initialMaxStamina);

        float startTime = Time.time; // Record the start time of the cooldown

        while (cooldown.IsCoolingDown)
        {
            float elapsedTime = Time.time - startTime; // Calculate the elapsed time since the cooldown started
            float remainingCooldown = Mathf.Max(0f, cooldown.CooldownTime - elapsedTime); // Calculate remaining cooldown time
            float currentStaminaValue = Mathf.Lerp(1f, 0f, remainingCooldown / cooldown.CooldownTime); // Adjust Lerp parameters to reflect the correct direction
            playerUI.SetStamina(currentStaminaValue);

            yield return null;
        }

        playerUI.SetStamina(1f); // Ensure stamina bar is completely filled at the end of cooldown
        Debug.Log("Dash cooldown ended");
    }
}