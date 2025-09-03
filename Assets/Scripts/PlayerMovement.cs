
using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CoinManager cm;

    // --- Movement Input ---
    private float horizontal;
    private float vertical;

    // --- Jump Timing ---
    private float coyoteTime = 0.2f;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private float jumpBufferTime = 0.2f;

    // --- Movement Settings ---
    [SerializeField] private float maxSpeed = 8f;
    [SerializeField] private float acceleration = 60f;
    [SerializeField] private float deceleration = 30f;
    [SerializeField] private float airControlMultiplier = 0.5f;
    [SerializeField] private float jumpingPower = 16f;
    private bool isFacingRight = true;

    // --- Dash System ---
    private bool isDashing;
    private bool isJumping;
    private bool hasDashed;
    private bool canDoubleJump;
    [SerializeField] private float dashingPower = 40f;
    [SerializeField] private float dashingTime = 0.3f;
    [SerializeField] private float dashCooldown = 1f;
    private float dashCooldownTimer = 0f;

    // --- Wall & Ledge ---
    [SerializeField] private Transform wallCheckLeft;
    [SerializeField] private Transform wallCheckRight;
    [SerializeField] private Transform ledgeCheck;
    [SerializeField] private LayerMask wallLayer;
    private bool isTouchingWall;
    private bool isWallSliding;
    private bool isGrabbingLedge;

    // --- Unity Components ---
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private TrailRenderer tr;
    [SerializeField] private ParticleSystem jumpParticles;
    [SerializeField] private ParticleSystem dashParticles;
    [SerializeField] private ParticleSystem deathParticles;
    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        if (isDashing) return;

        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");

        dashCooldownTimer -= Time.deltaTime;

        HandleWallSlide();
        HandleJumpBuffering();
        HandleJump();
        HandleDash();
        Flip();
        UpdateAnimationStates();
    }

    private void FixedUpdate()
    {
        if (isDashing) return;

        float targetSpeed = horizontal * maxSpeed;
        float speedDiff = targetSpeed - rb.velocity.x;
        float accelRate = Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration;
        float control = IsGrounded() ? 1f : airControlMultiplier;
        float force = speedDiff * accelRate * control;

        rb.AddForce(new Vector2(force, 0f));

        if (Mathf.Abs(rb.velocity.x) > maxSpeed)
            rb.velocity = new Vector2(Mathf.Sign(rb.velocity.x) * maxSpeed, rb.velocity.y);

        if (IsGrounded())
        {
            hasDashed = false;
            canDoubleJump = true;
            jumpBufferCounter = 0f;
        }
    }

    // --- FIXED: Jump now uses Space key directly ---
    private void HandleJumpBuffering()
    {
        if (Input.GetKeyDown(KeyCode.Space))
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;
    }

    private void HandleJump()
    {
        if (IsGrounded())
            coyoteTimeCounter = coyoteTime;
        else
            coyoteTimeCounter -= Time.deltaTime;

        if ((coyoteTimeCounter > 0f || isWallSliding) && jumpBufferCounter > 0f && !isJumping)
        {
            Vector2 jumpDir = isWallSliding ? new Vector2(-horizontal, 1f).normalized : Vector2.up;
            rb.velocity = new Vector2(rb.velocity.x, 0f); // reset Y velocity
            rb.AddForce(jumpDir * jumpingPower, ForceMode2D.Impulse);
            jumpBufferCounter = 0f;
            StartCoroutine(JumpCooldown());
            jumpParticles?.Play();
        }
        else if (canDoubleJump && jumpBufferCounter > 0f && !IsGrounded())
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpingPower);
            canDoubleJump = false;
            jumpBufferCounter = 0f;
            StartCoroutine(JumpCooldown());
            jumpParticles?.Play();
        }

        if (Input.GetKeyUp(KeyCode.Space) && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
            coyoteTimeCounter = 0f;
        }
    }

    // --- FIXED: Dash now uses right-click and safe Camera.main check ---
    private void HandleDash()
    {
        if (!Input.GetMouseButtonDown(1) || hasDashed || dashCooldownTimer > 0f) return;

        Vector3 mouseWorldPos = Camera.main != null
            ? Camera.main.ScreenToWorldPoint(Input.mousePosition)
            : transform.position + Vector3.right;

        Vector2 dashDirection = (mouseWorldPos - transform.position).normalized;

        dashCooldownTimer = dashCooldown;
        StartCoroutine(Dash(dashDirection));
    }

    private IEnumerator Dash(Vector2 direction)
    {
        hasDashed = true;
        isDashing = true;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0.3f;

        rb.velocity = direction * dashingPower;
        tr.emitting = true;
        dashParticles?.Play();

        yield return new WaitForSeconds(dashingTime);

        tr.emitting = false;
        rb.velocity *= 0.6f;
        rb.gravityScale = originalGravity;
        isDashing = false;
    }

    private IEnumerator JumpCooldown()
    {
        isJumping = true;
        yield return new WaitForSeconds(0.4f);
        isJumping = false;
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    private void HandleWallSlide()
    {
        bool touchingLeft = Physics2D.OverlapCircle(wallCheckLeft.position, 0.1f, wallLayer);
        bool touchingRight = Physics2D.OverlapCircle(wallCheckRight.position, 0.1f, wallLayer);
        isTouchingWall = touchingLeft || touchingRight;

        isWallSliding = isTouchingWall && !IsGrounded() && horizontal != 0;

        if (isWallSliding)
        {
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -2f));
        }
    }

    private void Flip()
    {
        if ((isFacingRight && horizontal < 0f) || (!isFacingRight && horizontal > 0f))
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }

    private void UpdateAnimationStates()
    {
        if (anim == null) return;

        anim.SetBool("isRunning", Mathf.Abs(horizontal) > 0.01f);
        anim.SetBool("isGrounded", IsGrounded());
        anim.SetBool("isDashing", isDashing);
        anim.SetBool("isWallSliding", isWallSliding);
        anim.SetFloat("verticalVelocity", rb.velocity.y);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Object"))
        {
            deathParticles?.Play();
            StartCoroutine(Die());
        }
    }

    private IEnumerator Die()
    {
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("collectable"))
        {
            Destroy(other.gameObject);
            cm.coinCount++;
        }
    }
}
