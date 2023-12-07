using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed;
    private bool _isMoving;
    private Vector2 _input;

    private void Update()
    {
        if (!_isMoving)
        {
            _input.x = Input.GetAxisRaw("Horizontal");
            _input.y = Input.GetAxisRaw("Vertical");


            //disable diagonal movement
            if (_input.x != 0) _input.y = 0;

            if (_input != Vector2.zero)
            {
                var targetPos = transform.position;
                targetPos.x += _input.x;
                targetPos.y += _input.y;

                StartCoroutine(Move(targetPos));
            }
        }
    }

    IEnumerator Move(Vector3 targetPos)
    {
        _isMoving = true;
        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;

        // Add a small delay after movement to prevent rapid coroutine starts
        yield return new WaitForSeconds(0.001f);

        _isMoving = false;
    }
}