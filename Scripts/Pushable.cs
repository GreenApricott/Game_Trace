using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Pushable : MonoBehaviour
{
    private Rigidbody2D rb;
    private Collider2D collider;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        collider = GetComponent<Collider2D>();
    }

    void Update()
    {
        
    }

    public void Move(Vector3 movement)
    {
        rb.velocity = movement;
    }

}
