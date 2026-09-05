using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
using System;
using Unity.VectorGraphics;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    //Editiable
    public float airModifier;
    public float crouchFactor;
    public bool limitedMovement;
    public float airresistance;
    public CameraMovement cam;
    public PlayerInput playerInput;
    public Transform orientation;
    public Rigidbody rb;
    public float groundDrag;
    public LayerMask whatIsGround;
    public float jumpCooldown;
    public float jumpPower;
    public float maxslopeangle;
    public float slideforce;
    public float walkspeed;
    public float runspeed;
    public float crouchspeed;
    public Transform playerObject;
    //Misc
    [HideInInspector] public Vector3 localVel;
    [HideInInspector] public Vector3 flatVel;
    public RaycastHit groundHit;
    [HideInInspector] public Vector3 smoothVel;
    private Vector3 smoothVelVelocity;
    //Crouching/Sliding
    [HideInInspector] public bool sliding;
    private float crouchvelocity;
    [HideInInspector] public bool slideFrictionIsOn;
    [HideInInspector] public float crouchheight;
    [HideInInspector] public bool crouched;
    private bool wasCrouchPressed;
    private bool objectabove;
    //Movement
    private float movespeed;
    [HideInInspector] public bool isMoving;
    [HideInInspector] public bool grounded;
    private float jumpForce;
    private bool jumpReady;
    [HideInInspector] public float horizontalInput;
    private float verticalInput;
    private Vector3 moveDirection;
    //Slope
    private bool exitslope;
    float slopeangle;
    public MovementState state;
    public enum MovementState
    {
        standing,
        walking,
        running,
        crouching,
        sliding,
        air
    }
    private void Start()
    {
        jumpReady = true;
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }
    void OnDrawGizmos()
    {
        Gizmos.DrawWireMesh(Resources.GetBuiltinResource<Mesh>("Capsule.fbx"), transform.position, 
            transform.rotation, new Vector3(0.25f, 0.05f, 0.25f));

    }
    private void FixedUpdate()
    {
        if (transform.position.y < -20)
        {
            transform.position = new Vector3(0, 20, 0);
            rb.linearVelocity = Vector3.zero;
        }
        if(SceneManager.GetActiveScene().name == "Infinite")
        {

            if(transform.position.x < -98)
            {
                transform.position = new Vector3(98, transform.position.y, transform.position.z);
            }
            if(transform.position.x > 98)
            {
                transform.position = new Vector3(-98, transform.position.y, transform.position.z);
            }
        }


        grounded = Physics.CheckCapsule(
            transform.position + Vector3.up * 0.05f, 
            transform.position + Vector3.down * 0.05f, 
            0.25f, 
            whatIsGround);
        Physics.Raycast(transform.position, Vector3.down, out groundHit, 0.25f, whatIsGround);

        bool onSlope = OnSlope();

        //Movement
        Movement(onSlope);
        MovementLimiting(onSlope);
        //Slope
        //Sliding
        SlideMovement(onSlope);
    }
    private void Update()
    {
        if (!grounded)
        {
            smoothVel = Vector3.SmoothDamp(smoothVel, rb.linearVelocity, ref smoothVelVelocity, 0.2f);
        }
        //MovementState
        States(OnSlope());
        //Inputs
        Input();
    }
    private void States(bool onSlope)
    {
        if (crouched && (grounded || onSlope) && isMoving && slideFrictionIsOn)
        {
            state = MovementState.sliding;
            movespeed = crouchspeed;
        }
        else if (crouched && grounded)
        {
            state = MovementState.crouching;
            movespeed = crouchspeed;
        }
        else if (grounded && playerInput.sprintPressed && (verticalInput > 0 || horizontalInput != 0))
        {
            state = MovementState.running;
            movespeed = runspeed;
        }
        else if (grounded && isMoving)
        {
            state = MovementState.walking;
            movespeed = walkspeed;
        }
        else if (grounded)
        {
            movespeed = 1;
            state = MovementState.standing;
        }
        else
        {
            movespeed = 1;
            state = MovementState.air;
        }
    }
    private void Movement(bool onSlope)
    {
        Physics.gravity= new Vector3(0,-20,0);

        transform.rotation = Quaternion.Euler(0, cam.yRotation, 0);
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        flatVel = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        localVel = orientation.InverseTransformDirection(rb.linearVelocity);

        if (onSlope && !exitslope && !sliding)
        {
            Vector3 slopeMove = GetSlopeMoveDirection(onSlope);
            rb.AddForce(slopeMove * movespeed * 10, ForceMode.Force);

            if(!playerInput.jumpPressed && rb.linearVelocity.y > 0.1f)
                rb.AddForce(Vector3.down * 5, ForceMode.Force);
        }
        else if (grounded && !sliding)
        {
            rb.AddForce(moveDirection.normalized * movespeed * 10, ForceMode.Force);
        }
        else if (!grounded || sliding)
        {
            rb.AddForce(moveDirection * airModifier * Mathf.Max(rb.linearVelocity.magnitude, 1) * 0.1f, ForceMode.Force);
        }

        rb.useGravity = !OnSlope() || exitslope;
        rb.linearDamping = (grounded || onSlope) && !sliding ? groundDrag : airresistance;

        playerObject.transform.localScale = new Vector3(
            playerObject.transform.localScale.x, 
            crouchheight, 
            playerObject.transform.localScale.z);

        objectabove = Physics.Raycast(transform.position, Vector3.up, 2, whatIsGround);
        if (objectabove && crouched)
        {
            playerObject.transform.localScale = new Vector3(
                playerObject.transform.localScale.x, 
                crouchFactor, 
                playerObject.transform.localScale.z);
        }
    }
    private void MovementLimiting(bool onSlope)
    {
        if (rb.linearVelocity.magnitude > 50 || sliding) return;
        if(limitedMovement && (grounded || onSlope) && !exitslope)
        {
            if (OnSlope() && !exitslope)
            {
                if (rb.linearVelocity.magnitude > movespeed)
                    rb.linearVelocity = rb.linearVelocity.normalized * movespeed;
            }
            else
            {
                if (flatVel.magnitude > movespeed)
                {
                    Vector3 limitedVel = flatVel.normalized * movespeed;
                    rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
                }
            }
        }
    }
    private void Input()
    {
        horizontalInput = playerInput.moveInput.x;
        verticalInput = playerInput.moveInput.y;

        isMoving = verticalInput != 0 || horizontalInput != 0;

        jumpForce = crouched ? jumpPower * 0.75f : jumpPower;

        if (playerInput.jumpPressed && jumpReady && grounded && !crouched)
        {
            exitslope = true;
            rb.linearVelocity = flatVel;
            rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
            jumpReady = false;
            Invoke(nameof(ResetJump), jumpCooldown);
        }
        if (playerInput.crouchPressed)
        {
            crouched = true;
            crouchheight = Mathf.SmoothDamp(crouchheight, crouchFactor, ref crouchvelocity, 0.1f);

            if(!wasCrouchPressed)
            {
                slideFrictionIsOn = rb.linearVelocity.magnitude > crouchspeed + 2f;
            }
            wasCrouchPressed = true;
        }
        else if (!objectabove)
        {
            crouched = false;
            wasCrouchPressed = false;
            slideFrictionIsOn = false;
            crouchheight = Mathf.SmoothDamp(crouchheight, 1f, ref crouchvelocity, 0.1f);
        }
    }
    private void ResetJump()
    {
        jumpReady = true;
        exitslope = false;
    }
    private bool OnSlope()
    {
        if(!grounded) return false;
        slopeangle = Vector3.Angle(Vector3.up, groundHit.normal);
        return slopeangle < maxslopeangle && slopeangle > 0.5f;
    }
    private Vector3 GetSlopeMoveDirection(bool onSlope)
    {
        if(!onSlope) return moveDirection.normalized;
        return Vector3.ProjectOnPlane(moveDirection, groundHit.normal).normalized;
    }
    private void SlideMovement(bool onSlope)
    {
        bool canSlide = playerInput.crouchPressed && slideFrictionIsOn && grounded && (isMoving || onSlope);
        if(!canSlide)
        {
            sliding = false;
            return;
        }

        if(!sliding && !onSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(onSlope) * slideforce, ForceMode.Impulse);
        }

        sliding = true;
        
        if (onSlope && !exitslope)
        {
            rb.AddForce(Vector3.down * 10, ForceMode.Force);
        }
        else
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, 0, rb.linearVelocity.z);
        }
    }
    public void Recoil(float recoilForce)
    {
        if (!grounded) rb.AddForce(-cam.transform.forward.normalized * recoilForce, ForceMode.Impulse);
    }
}