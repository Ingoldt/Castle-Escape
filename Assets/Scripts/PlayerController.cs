using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Range(0, .3f)][SerializeField] private float m_MovementSmoothing = .05f;

    private float _horizontalMove = 0f;
    private float _verticalMove = 0f;
    private Vector2 _velocity = Vector2.zero;
    private bool _facingRight = true;

    [SerializeField]
    private PlayerScriptableObject _playerStats;
    public Animator animator;
    public Rigidbody2D rb2D;
    public float moveSpeed;

    float totalMovement;

    private void Awake()
    {
        moveSpeed = _playerStats.movementSpeed;
    }

    private void Update()
    {
        // Processing Inputs
        _horizontalMove = Input.GetAxisRaw("Horizontal") * moveSpeed;
        _verticalMove = Input.GetAxisRaw("Vertical") * moveSpeed;

        // Calculate the total movement magnitude
        totalMovement = Mathf.Max(Mathf.Abs(_horizontalMove), Mathf.Abs(_verticalMove));

        animator.SetFloat("Speed", Mathf.Abs(totalMovement));

    }

    private void FixedUpdate()
    {
        Move(_horizontalMove, _verticalMove);
    }

    public void Move(float horizontalMove, float verticalMove)
    {
        // Normalize the movement vector
        Vector2 movementDirection = new Vector2(horizontalMove, verticalMove).normalized;

        // Move the character by finding the target velocity
        Vector2 targetVelocity = movementDirection * moveSpeed;

        // Smoothly adjust the character's velocity
        rb2D.velocity = Vector2.SmoothDamp(rb2D.velocity, targetVelocity, ref _velocity, m_MovementSmoothing);


        // If the input is moving the player right and the player is facing left...
        if (horizontalMove > 0 && !_facingRight)
        {
            // ... flip the player.
            Flip();
        }
        // Otherwise if the input is moving the player left and the player is facing right...
        else if (horizontalMove < 0 && _facingRight)
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

}