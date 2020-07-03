using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class SavingPoint : MonoBehaviour
{
    public Sprite shiny;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.GetComponent<Player2D>() != null)
        {
            Debug.Log("saved");
            GetComponent<SpriteRenderer>().sprite = shiny;
            SavingManager.Runtime_SaveGame();
            this.enabled = false;
        }
    }
}
