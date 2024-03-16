using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Range(0, .3f)][SerializeField] private float m_MovementSmoothing = .05f;
    public Animator animator;
    public Rigidbody2D rb2D;
    public TrailRenderer trailRenderer;

    [Header("Player Stats")]
    [SerializeField] private PlayerScriptableObject _playerStats;
    public float moveSpeed;
    public bool isInvincible;

    [Header("Dashing")]
    [SerializeField] private float dashDeceleration = 1f;
    [SerializeField] private float _dashingVelocity;
    [SerializeField] private float _dashDuration;
    [SerializeField] private float _dashCooldown;
    private bool _isDashing;
    private bool _canDash = true;

    [Header("Movement")]
    private bool _facingRight = true;
    private Vector2 _velocity = Vector2.zero;
    Vector3 moveDir = Vector3.zero;
    float totalMovement;

    private void Start()
    {
        trailRenderer = GetComponent<TrailRenderer>();
        moveSpeed = _playerStats.movementSpeed;
        moveDir = Vector2.zero;
        isInvincible = _playerStats.isInvincible;
        _dashingVelocity = 45f;
        _dashDuration = 0.4f;
        _dashCooldown = 5f;
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
        if (_isDashing)
        {
            // Calculate the deceleration force
            Vector2 decelerationForce = -rb2D.velocity.normalized * dashDeceleration;

            // Apply the deceleration force
            rb2D.AddForce(decelerationForce, ForceMode2D.Force);
        }
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
        if (ctxt.started && _canDash)
        {
            Dash();
        }
    }

    void Dash()
    {
        if (!_isDashing && _canDash)
        {
            // Player dash animation
            animator.SetTrigger("Dash");
            trailRenderer.emitting = true;
            _isDashing = true;
            _canDash = false;
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


            rb2D.velocity = dashDirection.normalized * _dashingVelocity;
            Debug.Log("Dash Velocity: " + rb2D.velocity);
            StartCoroutine(StopDashing());
        }
    }

    private IEnumerator StopDashing()
    {
        yield return new WaitForSeconds(_dashDuration);
        Debug.Log("Dash ended");
        _isDashing = false;
        trailRenderer.emitting = false;
        rb2D.velocity = Vector2.zero;

        Physics2D.IgnoreLayerCollision(LayerMask.NameToLayer("Player"), LayerMask.NameToLayer("Enemy"), false);

        // Player is invincible during dash
        _playerStats.SetInvincible(false);

        StartCoroutine(DashCooldown());
    }
    private IEnumerator DashCooldown()
    {
        yield return new WaitForSeconds(_dashCooldown);
        Debug.Log("Dash cooldown ended");
        _canDash = true;
    }


}