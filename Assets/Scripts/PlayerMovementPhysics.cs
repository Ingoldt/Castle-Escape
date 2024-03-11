using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class PlayerMovementPhysics : MonoBehaviour
{
    public float moveSpeed;
    public Rigidbody2D rb2D;

    private bool _isMoving;
    private Vector2 _input;
    private Vector2 _direction;

    private void Update()
    {
        //Processing Inputs
        ProcessInputs();
    }

    private void FixedUpdate()
    {
        //Charcter move Physics
        Move();
    }

    void ProcessInputs()
    {
        _input.x = Input.GetAxis("Horizontal");
        _input.y = Input.GetAxis("Vertical");

        // Normalize the input vector
        _direction = _input.normalized;
        //Debug.Log("Input: " + _input + " Direction: " + _direction);
    }

    void Move()
    {
        //does't need Time.deltaTime because its called in Fixed update
        rb2D.velocity = _direction * moveSpeed;
        //Debug.Log(rb2D.velocity.magnitude);
    }
}