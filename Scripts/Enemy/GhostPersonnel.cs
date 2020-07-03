using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GhostPersonnel : MonoBehaviour
{
    
    public enum GhostMode { wandering, pursuing };

    public GhostMode ghostMode = GhostMode.wandering;

    private Player2D player;

    private MovingGhost movingGhost;

    public float distance;

    public float angle;

    public float PursuingSpeed;
    
    private bool faceRight = true;

    private Vector3 curCheckDir;
    private Vector3 lastPos;

    void Start()
    {
        player = GameObject.FindObjectOfType<Player2D>();
        movingGhost = GetComponent<MovingGhost>();
        lastPos = movingGhost.getNextNodePos();
        curCheckDir = lastPos - transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        lastPos = movingGhost.getNextNodePos();
        Vector3 playerDir = player.getEnemyCheckPos().position - transform.position;
        Vector3 checkDir = Vector3.Lerp(curCheckDir, lastPos - transform.position, Time.deltaTime);
        curCheckDir = checkDir;

        float currentAngle = Vector3.Angle(playerDir, checkDir);
            
        if (ghostMode == GhostMode.wandering) {
            Debug.DrawRay(transform.position, Quaternion.AngleAxis( angle / 2, Vector3.forward) * checkDir.normalized * distance, Color.yellow);
            Debug.DrawRay(transform.position, Quaternion.AngleAxis(-angle / 2, Vector3.forward) * checkDir.normalized * distance, Color.yellow);
            if (currentAngle < angle/2 && playerDir.magnitude <= distance)
            {
                RaycastHit2D hit = Physics2D.Raycast(transform.position, playerDir, distance, ~(1 << LayerMask.NameToLayer("Ghost")));
                Debug.DrawRay(transform.position, playerDir, Color.red);

                if (hit.transform == player.transform)
                {
                    Invoke("Caught", 0);
                }
            }
        }
        else {
            Pursue();
        }
        if (Vector3.Distance(transform.position, player.transform.position) <= 1)
            deathTrigger();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.tag == "player")
        {
            Caught();
        }
    }

    private void Caught()
    {
        Debug.Log("发现玩家");
        ghostMode = GhostMode.pursuing;
        
    }

    private void Pursue() {
        Vector3 dir = player.transform.position - transform.position;
        if (dir.magnitude>0.3f) {
            movingGhost.enabled = false;
            dir.Normalize();
            transform.position += dir * PursuingSpeed * Time.deltaTime;
        }
    }

    private void deathTrigger() {
        player.death.Invoke();
    }
}
