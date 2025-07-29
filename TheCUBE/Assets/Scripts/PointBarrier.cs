using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Turns the point charge around when it slips through the screen.
public class PointBarrier : MonoBehaviour
{
    private Vector3 normalDirection;
    
    void Start()
    {
        normalDirection = GetComponentInParent<Transform>().up;
    }

    private void OnTriggerEnter(Collider other)
    {
        Rigidbody rb = other.attachedRigidbody;
        Vector3 vel = rb.velocity;
        rb.velocity = Vector3.Reflect(vel, normalDirection);
    }
}
