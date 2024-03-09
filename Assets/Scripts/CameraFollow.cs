using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Camerafollow : MonoBehaviour
{
    [SerializeField] private Vector3 offset;
    [SerializeField] private float damping;

    public Transform Target;

    private Vector3 velocity = Vector3.zero;

    public void SetTarget(Transform target)
    {
        Target = target;
    }
    private void LateUpdate()
    {
        if (Target != null)
        {
            Vector3 targetPos = Target.position + offset;
            targetPos.z = transform.position.z;
            transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref velocity, damping);
            //transform.position = targetPos;
        }
    }
}
