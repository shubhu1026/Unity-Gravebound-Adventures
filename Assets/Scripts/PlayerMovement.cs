using System;
using System.Collections;
using System.Runtime.InteropServices;
using TMPro;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] float moveSpeed = 5f;

    [Header("Jump System")]
    [SerializeField] float jumpForce = 5f;
    [SerializeField] float fallMultiplier = 2.5f;
    [SerializeField] float lowJumpMultiplier = 2f;

    [Header("Dash")]
    [SerializeField] float dashingVelocity = 14f;
    [SerializeField] float dashingTime = 0.5f;
    [SerializeField] float dashCooldown = 1f;

    [Header("Ground Check")]
    [SerializeField] Transform groundCheck;
    [SerializeField] LayerMask groundLayer;

    [Header("Wall Sliding")]
    [SerializeField] float wallSlidingSpeed = 4f;
    [SerializeField] Transform wallCheck;
    [SerializeField] LayerMask wallLayer;

    [Header("Wall Jumping")]
    [SerializeField] float wallJumpingTime = 0.2f;
    [SerializeField] float wallJumpingDuration = 0.4f;
    [SerializeField] Vector2 wallJumpingPower = new Vector2(8f, 16f);

    [Header("Trail")]
    [SerializeField] TrailRenderer tr;

    [Header("SFX")]
    [SerializeField] AudioClip sfxJump;
    [SerializeField] AudioClip sfxDash;
    [SerializeField] AudioClip sfxWin;
    [SerializeField] AudioClip sfxDie;

    //Audio
    AudioSource src;

    private Rigidbody2D rb;

    //Move
    public bool canMove = true;

    //Dimension
    public bool isUpsideDown = false;

    //Ground Check
    private float groundCheckRadius = 0.1f;

    //Jump Variables
    private bool isJumping;
    private float jumpTimeCounter;

    //Coyote Time Variables
    private float coyoteTime = 0.2f;
    private float coyoteTimeCounter;

    //Jump Buffer Variables
    private float jumpBufferTime = 0.2f;
    private float jumpBufferCounter;

    //Dash Variables
    private Vector2 dashingDir;
    private bool isDashing;
    private bool canDash = true;

    //Wall Sliding
    private bool isWallSliding;

    //Wall Jumping
    private bool isWallJumping;
    private float wallJumpingDirection;
    private float wallJumpingCounter;

    //Animator
    private Animator animator;

    float moveInput;

    Vector3 currScale;
    Vector2 velocityVector;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        src = GetComponent<AudioSource>();
    }

    private void Start()
    {
        canMove = true;
        isUpsideDown = false;
        SetGravity();
    }

    private void Update()
    {
        if(canMove)
        {
            ProcessInput();
            MovePlayer();
        }
        Animate();
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    private bool IsWalled()
    {
        return Physics2D.OverlapCircle(wallCheck.position, 0.2f, wallLayer);
    }

    private void ProcessInput()
    {
        // Read input for movement
        moveInput = Input.GetAxis("Horizontal");

        // Check for input to switch between worlds
        if (Input.GetMouseButtonDown(1))
        {
            ChangeDimension();
        }
        
        if (Input.GetKeyDown(KeyCode.LeftShift) && canDash)
        {
            Dash();
        }
    }

    private void MovePlayer()
    {
        //If Player is Dashing
        if (isDashing)
        {
            rb.velocity = dashingDir.normalized * dashingVelocity;
            return;
        }

        if (!isWallJumping)
        { 
            // Apply horizontal movement
            rb.velocity = new Vector2(moveInput * moveSpeed, rb.velocity.y);

            //Rotate Sprite According to the direction the player moves in
            RotatePlayerSprite();
        }

        //Jump
        ProcessJump();
        WallSlide();
        WallJump();
    }

    private void RotatePlayerSprite()
    {
        currScale = transform.localScale;

        if (moveInput < 0 && currScale.x > 0)
        {
            transform.localScale = new Vector3(-1 * currScale.x, currScale.y, currScale.z);
        }
        else if (moveInput > 0 && currScale.x < 0)
        {
            transform.localScale = new Vector3(-1 * currScale.x, currScale.y, currScale.z);
        }
    }

    void ProcessJump()
    {
        if(isDashing)
        {
            return;
        }

        //Coyote Time
        if (IsGrounded())
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        //Jump Buffer
        if(Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        if (!isUpsideDown)
        {
            NormalJump();
        }
        else
        {
            UpsideDownJump();
        }
    }

    private void UpsideDownJump()
    {
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            //rb.velocity = Vector2.up * -jumpForce;
            rb.velocity = new Vector2(rb.velocity.x, -jumpForce);
            jumpBufferCounter = 0f;
        }

        if (rb.velocity.y > 0)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if (rb.velocity.y < 0 && !Input.GetButton("Jump"))
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
            coyoteTimeCounter = 0;
        }
    }

    private void NormalJump()
    {
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            //rb.velocity = Vector2.up * jumpForce;
            PlaySfx(sfxJump);
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
            jumpBufferCounter = 0f;
        }

        if (rb.velocity.y < 0)
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
        }
        else if (rb.velocity.y > 0 && !Input.GetButton("Jump"))
        {
            rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
            coyoteTimeCounter = 0;
        }
    }

    private void ChangeDimension()
    {
        if (isDashing) return;

        velocityVector = rb.velocity;
        transform.position = new Vector3(transform.position.x, -1 * transform.position.y, transform.position.z);
        rb.velocity = velocityVector;
        isUpsideDown = !isUpsideDown;
        SetGravity();
    }

    public void SetGravity()
    {
        if (!isUpsideDown)
        {
            if (currScale.y < 0)
            {
                transform.localScale = new Vector3(currScale.x, -1 * currScale.y, currScale.z);
            }
            Physics2D.gravity = new Vector2(0f, -9.81f); // Set gravity to point downward in upright world                                                  
        }
        else
        {
            if (currScale.y > 0)
            {
                transform.localScale = new Vector3(currScale.x, -1 * currScale.y, currScale.z);
            }
            Physics2D.gravity = new Vector2(0f, 9.81f); // Set gravity to point upward in upside-down world
        }
    }

    private void WallSlide()
    {
        if (IsWalled() && !IsGrounded() && moveInput != 0f)
        {
            isWallSliding = true;
            if (!isUpsideDown)
                rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, -wallSlidingSpeed, float.MaxValue));
            else
               rb.velocity = new Vector2(rb.velocity.x, Mathf.Clamp(rb.velocity.y, wallSlidingSpeed, float.MinValue));
        }
        else
        {
            isWallSliding = false;
        }
    }

    private void WallJump()
    {
        if(isWallSliding)
        {
            isWallJumping = false;
            wallJumpingDirection = -transform.localScale.x;
            wallJumpingCounter = wallJumpingTime;

            CancelInvoke(nameof(StopWallJumping));
        }
        else
        {
            wallJumpingCounter -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump") && wallJumpingCounter > 0f)
        {
            isWallJumping = true;
            PlaySfx(sfxJump);
            if(!isUpsideDown)
                rb.velocity = new Vector2(wallJumpingDirection * wallJumpingPower.x, wallJumpingPower.y);
            else
                rb.velocity = new Vector2(wallJumpingDirection * wallJumpingPower.x, -wallJumpingPower.y);

            /*
            //if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
            //{
                //rb.velocity = Vector2.up * jumpForce;
                rb.velocity = new Vector2(rb.velocity.x, jumpForce);
                jumpBufferCounter = 0f;
            //}

            if (rb.velocity.y < 0)
            {
                rb.velocity += Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * Time.deltaTime;
            }
            else if (rb.velocity.y > 0 && !Input.GetButton("Jump"))
            {
                rb.velocity += Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * Time.deltaTime;
                coyoteTimeCounter = 0;
            }
            */

            wallJumpingCounter = 0f;

            if(transform.localScale.x != wallJumpingDirection)
            {
                Vector3 localScale = transform.localScale;
                localScale.x *= -1f;
                transform.localScale = localScale;
            }

            Invoke(nameof(StopWallJumping), wallJumpingDuration);
        }
    }

    private void StopWallJumping()
    {
        isWallJumping = false;
    }

    private void Dash()
    {
        PlaySfx(sfxDash);
        isDashing = true;
        canDash = false;
        tr.emitting = true;
        dashingDir = new Vector2(moveInput, Input.GetAxisRaw("Vertical"));

        if (dashingDir == Vector2.zero || dashingDir == Vector2.up || dashingDir == Vector2.down)
        {
            dashingDir = new Vector2(transform.localScale.x, 0f);
        }
        StartCoroutine(StopDashing());
    }

    public void StopPlayer()
    {
        canMove = false;
        rb.velocity = new Vector2(0f, 0f);
    }

    private IEnumerator StopDashing()
    {
        yield return new WaitForSeconds(dashingTime);
        tr.emitting = false;
        isDashing = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void Animate()
    {
        animator.SetBool("AnimIsDashing", isDashing);

        if(moveInput == 0)
        {
            animator.SetBool("AnimIsIdle", true);
        }
        else
        {
            animator.SetBool("AnimIsIdle", true);
        }
    }

    public void PlaySfx(AudioClip clip)
    {
        src.PlayOneShot(clip, 0.3f);
    }
}
