using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Events;


public class Player2D : MonoBehaviour
{
    //固有属性
    private Vector2 capsuleColliderSize;
    private Rigidbody2D rb;
    private Collider2D cc;
    private Vector3 newVelocity;
    //PlayerSprite
    private SpriteRenderer playerSprite;
    private Animator playerAnim;

    private float xInput;
    [SerializeField]
    private bool isGrounded;
    [SerializeField]
    private bool isRun;
    [SerializeField]
    private bool isOnSlope;
    [SerializeField]
    private bool isCrouch;
    [SerializeField]
    private bool canPushing;
    private bool isPush;

    private bool canMove = true;
    [SerializeField]
    private bool canClimbLedge = false;
    private bool canWalkOnSlope;
    private bool ledgeDetected;
    //Player朝向
    private bool playerFaceLeft = false;
    //Enemy检查点
    private Transform enemyCheckPos;

    //private Pushable pushable = new Pushable();
    //public List<Pushable> pushables = new List<Pushable>();

    [Header("攀爬检测")]
    public Transform wallDetector;
    public Transform ledgeDetector;
    public float climbCheckDistance;

    [Header("攀爬检测")]
    private Vector2 LedgePosBottom, ledgePos1, ledgePos2;
    public Vector2 LedgeClimbOffset;
    private Vector2 slopeNormalPerp;
    private float wallcheckDistance;
    private float slopeSideAngle, slopeDownAngle, lastSlopeAngle;

    [Header("坡度检测距离")]
    public float slopeCheckDistance;
    public float slopeCheckOffset;
    [Tooltip("最大可走坡度")]
    public float maxSlopeAngle;

    [Header("移动速度")]
    public float movementSpeed = 6;
    [Tooltip("跑步速度修正")]
    [Range(1f, 3f)]
    public float runSpeedOffset;
    [Tooltip("推动速度修正")]
    [Range(0.1f,1)]
    public float pushSpeedOffset;
    [Tooltip("推动速度修正")]
    [Range(0.1f, 1)]
    public float crouchSpeedOffset;
    [Tooltip("玩家下落时空中位移修正")]
    [Range(0.1f,2)]
    public float movementAirOffset = 1;

    [Header("地板检测半径")]
    private Transform groundCheck;
    [Range(0.1f,0.5f)]
    public float groundCheckRadius;

    [Header("摄像机属性")]
    public Transform cameraTarget;
    [Range(0,5)]
    public float cameraHorizontalFacingOffset;
    [Range(0, 10)]
    public float cameraHorizontalSpeed;

    [Header("动画状态")] 
    [Range(0,1)]
    public float windSpeed = 0;

    [Header("推动检测")]
    public GameObject pushDetector;
    [Range(0,2)]
    public float pushDetectDistance;
    private Pushable pushTGT;

    //死亡相关
    public UnityEvent death;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        cc = GetComponent<Collider2D>();

        groundCheck = transform.Find("GroundCheckPos");
        enemyCheckPos = transform.Find("EnemyCheckPos");
        playerSprite = GetComponent<SpriteRenderer>();
        playerAnim = GetComponent<Animator>();
        playerAnim.SetFloat("WindSpeed", windSpeed);
        UnityAction action = new UnityAction(SavingManager.Runtime_LoadGame);
        death.AddListener(action);
      
        capsuleColliderSize =  new Vector2(0, 1.8f);
    }

    // Update is called once per frame
    public void Update()
    {
        if (canMove)
            CheckInput();

        playerAnim.SetFloat("WindSpeed", windSpeed);
    }

    public void FixedUpdate()
    {
        PushDetect();
        CheckSorroundings();
        SlopeCheck();
        ApplyMovement();
        UpdateCameraTargetPos();
    }

    private void CheckInput()
    {
        //检测X轴输入 & 下蹲
        if (isGrounded)
        {
            xInput = Input.GetAxisRaw("Horizontal");
            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                isCrouch = !isCrouch;
                playerAnim.SetBool("IsCrouch", isCrouch);
            }   
            playerAnim.SetFloat("HorzontalSpeed", xInput);    
        }

        //推动 & 跑步
        if (Input.GetKey(KeyCode.LeftShift) && isGrounded)
        {
            if (!canPushing && !isCrouch)
            {
                isPush = false;
                if (xInput != 0)
                {
                    isRun = true;
                }                    
                else
                    isRun = false;
            }
            else if (canPushing && !isCrouch)
            {
                isPush = true;
                isRun = false;
            }
        }
        else
        {
            isPush = false;
            isRun = false; 
        }
        playerAnim.SetBool("IsPush",isPush);
        playerAnim.SetBool("IsRun", isRun);

        //走路 
        if (xInput != 0 && isGrounded)
        { 
            if(!isPush)
                playerFaceLeft = xInput > 0 ? false : true;
            playerAnim.SetBool("IsWalk", true);
        }
        else
        {
            playerAnim.SetBool("IsWalk", false);
        }
       
        playerSprite.flipX = playerFaceLeft;

        //攀爬
        if (canClimbLedge && !playerAnim.GetBool("IsPush") && Input.GetKeyDown(KeyCode.Space))
        {
            playerAnim.SetTrigger("IsClimb");
            canPushing = false;
            canMove = false;
        }

    }

    public void UpdateCameraTargetPos()
    {
        float newLocalPosX;

        float desiredLocalPosX = (playerFaceLeft ? -1f : 1f) * cameraHorizontalFacingOffset;
        desiredLocalPosX += transform.position.x;
        if (Mathf.Approximately(cameraHorizontalSpeed, 0f))
        {
            newLocalPosX = desiredLocalPosX;
        }
        else
        {
            newLocalPosX = Mathf.Lerp(cameraTarget.position.x, desiredLocalPosX, cameraHorizontalSpeed * Time.deltaTime);
        }
        cameraTarget.position = new Vector2(newLocalPosX, cameraTarget.position.y);
    }

    private void SlopeCheckHorizontal(Vector2 checkPos)
    {
        
        RaycastHit2D slopeHitFront = Physics2D.Raycast(checkPos,  transform.right, slopeCheckDistance,1<< LayerMask.NameToLayer("ground"));
        RaycastHit2D slopeHitBack  = Physics2D.Raycast(checkPos, -transform.right, slopeCheckDistance,1<< LayerMask.NameToLayer("ground"));
        Debug.DrawRay(checkPos, transform.right, Color.red);
        if (slopeHitFront)
        {
            isOnSlope = true;
            slopeSideAngle = Vector2.Angle(slopeHitFront.normal, Vector2.up);
        }
        else if (slopeHitBack)
        {
            isOnSlope = true;

            slopeSideAngle = Vector2.Angle(slopeHitBack.normal, Vector2.up);
        }
        else
        {
            slopeSideAngle = 0.0f;
            isOnSlope = false;
        }

    }

    private void SlopeCheckVertical(Vector2 checkPos)
    {
        
        RaycastHit2D hitinfo = Physics2D.Raycast(checkPos, Vector2.down, slopeCheckDistance,1<< LayerMask.NameToLayer("ground"));

        if (hitinfo)
        {

            slopeNormalPerp = Vector2.Perpendicular(hitinfo.normal).normalized;

            slopeDownAngle = Vector2.Angle(hitinfo.normal, Vector2.up);

            if (slopeDownAngle != lastSlopeAngle)
            {
                isOnSlope = true;
            }

            lastSlopeAngle = slopeDownAngle;

            Debug.DrawRay(hitinfo.point, slopeNormalPerp, Color.blue);
            Debug.DrawRay(hitinfo.point, hitinfo.normal, Color.green);

        }

        if (slopeDownAngle > maxSlopeAngle || slopeSideAngle > maxSlopeAngle)
        {
            canWalkOnSlope = false;
        }
        else
        {
            canWalkOnSlope = true;
        }

    }

    private void SlopeCheck()
    {
        Vector3 checkPos = transform.position - (Vector3)(new Vector2(0.0f, capsuleColliderSize.y / 2 + slopeCheckOffset));//得到角色底部的点的坐标

        SlopeCheckHorizontal(checkPos);
        SlopeCheckVertical(checkPos);
    }

    private void ApplyMovement()
    {
        Vector3 checkPos = transform.position - (Vector3)(new Vector2(0.0f, capsuleColliderSize.y / 2 + slopeCheckOffset));
        if (isGrounded && !isOnSlope ) //if not on slope
        {
            float speedOffset = 1f;
            if (isRun)
            {
                speedOffset = runSpeedOffset;
            }
            else if (isCrouch)
            {
                speedOffset = crouchSpeedOffset;  
            }
            else if(canPushing)
            {
                speedOffset = pushSpeedOffset;
            }
            Debug.Log("speedOffset"+ speedOffset);
 
            newVelocity = new Vector3(movementSpeed * xInput * speedOffset, 0f, 0);

            if (playerAnim.GetBool("IsPush") && playerAnim.GetBool("IsWalk"))
            {
                MoveBox(newVelocity);
            }
            rb.velocity = newVelocity;
        }

        else if (isGrounded && isOnSlope ) //If on slope
        {
            if (canWalkOnSlope)
            {
                newVelocity = new Vector3(movementSpeed * slopeNormalPerp.x * -xInput, movementSpeed * slopeNormalPerp.y * -xInput, 0);
                rb.velocity = newVelocity;
            }
            else
            {
               
                RaycastHit2D slopeHitFront = Physics2D.Raycast(checkPos, transform.right,slopeCheckDistance, 1 << LayerMask.NameToLayer("ground"));
                RaycastHit2D slopeHitBack = Physics2D.Raycast(checkPos, -transform.right,slopeCheckDistance, 1 << LayerMask.NameToLayer("ground"));
                if (slopeHitFront.distance > slopeHitBack.distance && xInput == -1)
                {
                    newVelocity = new Vector3(movementSpeed * slopeNormalPerp.x * -xInput, movementSpeed * slopeNormalPerp.y * -xInput, 0);
                    rb.velocity = newVelocity;
                }
                else if (slopeHitFront.distance < slopeHitBack.distance && xInput == 1)
                {
                    newVelocity = new Vector3(movementSpeed * slopeNormalPerp.x * -xInput, movementSpeed * slopeNormalPerp.y * -xInput, 0);
                    rb.velocity = newVelocity;
                }
            }
        }
        else if (!isGrounded) //If in air
        {
            isRun = false;
            isCrouch = false;
            playerAnim.SetBool("IsRun", false);
            playerAnim.SetBool("IsCrouch", false);
            newVelocity=new Vector2(movementSpeed * movementAirOffset * xInput, rb.velocity.y );
            rb.velocity = newVelocity;
        }

    }

    private void CheckSorroundings()
    {
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, 1 << LayerMask.NameToLayer("ground"));
        playerAnim.SetBool("IsGround", isGrounded);

        RaycastHit2D isTouchingWall, isTouchingLedge;
        int isRight = playerFaceLeft ? -1 : 1;
        //Debug.DrawRay(wallDetector.position + new Vector3(isRight * 0.2f, 0, 0), transform.right * isRight,Color.blue);
        //Debug.DrawRay(ledgeDetector.position + new Vector3(isRight * 0.2f, 0, 0), transform.right * isRight, Color.yellow);
        isTouchingWall = Physics2D.Raycast(wallDetector.position + new Vector3(isRight*0.2f,0,0), transform.right * isRight, climbCheckDistance, 1 << LayerMask.NameToLayer("ground"));
        isTouchingLedge = Physics2D.Raycast(ledgeDetector.position + new Vector3(isRight * 0.2f, 0, 0), transform.right * isRight, climbCheckDistance, 1 << LayerMask.NameToLayer("ground"));

        if (isTouchingWall && !isTouchingLedge)
        {
            canClimbLedge = true;
            ledgePos2 = new Vector3(isTouchingWall.point.x + LedgeClimbOffset.x * isRight, isTouchingWall.point.y + LedgeClimbOffset.y,transform.position.z);
        }
        else
        {
            canClimbLedge = false;
        }
    }

    public void FinishLedgeClimb()
    {
        canClimbLedge = false;
        transform.position = ledgePos2;
        canMove = true;
        canPushing = false;
        ledgeDetected = false;
    }

    public Transform getEnemyCheckPos()
    {
        return enemyCheckPos;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("MoveableObject"))
        {
            if (xInput * (collision.transform.position.x - transform.position.x) > 0)
                pushTGT = collision.gameObject.GetComponent<Pushable>();
          
        }
    }

    public void PushDetect() {
        RaycastHit2D isFacingBox;
        int isRight = playerFaceLeft ? -1 : 1;
        isFacingBox = Physics2D.Raycast(pushDetector.transform.position, transform.right * isRight, pushDetectDistance, 1 << LayerMask.NameToLayer("MoveableObject"));
        Debug.DrawRay(pushDetector.transform.position, transform.right * isRight,Color.red);
        if (isFacingBox)
            canPushing = true;
        else
            canPushing = false;
    }

    private void MoveBox(Vector3 movement) {
        int isRight = playerFaceLeft ? -1 : 1;
        Vector3 tgtVec = movement;
        try
        {
            pushTGT.Move(tgtVec);
        }
        catch (Exception E) { }
        }
    }
