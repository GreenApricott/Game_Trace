using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveableObject : MonoBehaviour
{
    public float velocity;

    private Rigidbody2D rb;
    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void Move(Vector3 target)
    {
        var temp = target - transform.position;
        temp.y = 0;
        rb.velocity = temp.normalized * velocity;
    }
    // Update is called once per frame

}
