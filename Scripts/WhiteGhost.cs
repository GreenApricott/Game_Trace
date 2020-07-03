using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WhiteGhost : MonoBehaviour
{
 
    private Vector3 targetPoint;
    private bool follow=true;
    private bool canManipulate = false;
    private MoveableObject moveableobj;

    // Update is called once per frame
    void FixedUpdate()
    {
        if (follow)
        {
            Move();
            mouseCheck();
            manipulatObject();           
        }
        
    }

    public void Move()
    {
        // a 创建射线
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);// 从摄像机发射出一条经过鼠标当前位置的射线
        RaycastHit hitInfo = new RaycastHit();
        if (Physics.Raycast(ray, out hitInfo))
        {           
            targetPoint = hitInfo.point;
            targetPoint.z = hitInfo.point.z - 0.1f;       
            transform.position=Vector3.Lerp(transform.position, targetPoint, 0.5f);
            Debug.DrawRay(transform.position, transform.forward,Color.red);
        }
    }

    public void manipulatObject() {
        if (canManipulate)
        {
            moveableobj.Move(transform.position);
        }
    }

    private void mouseCheck() {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit2D hit = Physics2D.Raycast(transform.position, transform.forward, 20, 1 << LayerMask.NameToLayer("MoveableObject"));

        if (hit&&Input.GetMouseButtonDown(0))
        {
            moveableobj = hit.collider.GetComponent<MoveableObject>();
            if (moveableobj != null)
            {
                canManipulate = true;
                
            }
        }
        if (Input.GetMouseButtonUp(0))
            canManipulate = false;
    }

}
