using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DeathObject : MonoBehaviour
{
 
    private void OnCollisionEnter2D(Collision2D collision)
    {

        if (collision.gameObject.GetComponent<Player2D>()!=null)
        {
            collision.gameObject.GetComponent<Player2D>().death.Invoke();
        }
    }
}
