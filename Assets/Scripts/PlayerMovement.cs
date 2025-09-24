using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CoinManager cm;

    // --- Movement Input ---
    private float horizontal;

    // --- Jump Timing ---
    [SerializeField] private float coyoteTime = 0.2f;
    [SerializeField] private float jumpBufferTime = 0.2f;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;

    // --- Movement Settings ---
    [SerializeField] private float maxSpeed = 8f;
    [SerializeField] private float acceleration = 100f;
    [SerializeField] private float deceleration = 80f;
    [SerializeField] private float airControlMultiplier = 0.6f;
    [SerializeField] private float jumpingPower = 16f;

    // --- Dash Settings ---
    [SerializeField] private float dashDistance = 6f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 1f;
    private float dashCooldownTimer;
    private bool isDashing, hasDashed;

    // --- Dash Preview ---
    [SerializeField] private LineRenderer dashPreview;
    [SerializeField] private Color previewColor = Color.cyan;

    // --- Wall Slide ---
    [SerializeField] private Transform wallCheckLeft;
    [SerializeField] private Transform wallCheckRight;
    [SerializeField] private LayerMask wallLayer;
    private bool isWallSliding;

    // --- Unity Components ---
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private ParticleSystem jumpParticles;
    [SerializeField] private ParticleSystem dashParticles;
    [SerializeField] private ParticleSystem deathParticles;
    private Animator anim;
    private bool isFacingRight = true;
    private HashSet<string> _animParams;

    private void Awake()
    {
        anim = GetComponent<Animator>();
        _animParams = new HashSet<string>();
        foreach (var p in anim.parameters)
            _animParams.Add(p.name);

        if (dashPreview != null)
        {
            dashPreview.positionCount = 2;
            dashPreview.enabled = true;
            dashPreview.startColor = previewColor;
            dashPreview.endColor = previewColor;
        }
    }

    private void Update()
    {
        if (!isDashing)
        {
            horizontal = Input.GetAxisRaw("Horizontal");

            // Jump buffering
            if (Input.GetButtonDown("Jump"))
                jumpBufferCounter = jumpBufferTime;
            else
                jumpBufferCounter -= Time.deltaTime;

            // Coyote time
            coyoteTimeCounter = IsGrounded() ? coyoteTime : coyoteTimeCounter - Time.deltaTime;

            // Dash cooldown
            dashCooldownTimer -= Time.deltaTime;

            HandleWallSlide();
            HandleJump();
            HandleDashPreview();

            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.LeftShift))
                TryDash();

            Flip();
        }

        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        if (isDashing) return;

        float targetSpeed = horizontal * maxSpeed;
        float speedDiff = targetSpeed - rb.velocity.x;
        float accelRate = Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration;
        float control = IsGrounded() ? 1f : airControlMultiplier;

        rb.AddForce(Vector2.right * speedDiff * accelRate * control);

        if (Mathf.Abs(rb.velocity.x) > maxSpeed)
            rb.velocity = new Vector2(Mathf.Sign(rb.velocity.x) * maxSpeed, rb.velocity.y);

        if (IsGrounded())
            hasDashed = false;
    }

    private void HandleJump()
    {
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpingPower);
            jumpBufferCounter = 0f;
            jumpParticles?.Play();
        }

        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0f)
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);
    }

    private void TryDash()
    {
        if (hasDashed || dashCooldownTimer > 0f) return;

        Vector3 mousePos = Input.mousePosition;
        mousePos.z = -Camera.main.transform.position.z;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        Vector2 dashDir = ((Vector2)worldPos - rb.position).normalized;

        StartCoroutine(DashRoutine(dashDir));
        dashCooldownTimer = dashCooldown;
        hasDashed = true;
    }

    private IEnumerator DashRoutine(Vector2 dir)
    {
        isDashing = true;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        dashParticles?.Play();

        rb.velocity = dir * dashDistance / dashDuration;
        yield return new WaitForSeconds(dashDuration);

        rb.gravityScale = originalGravity;
        isDashing = false;
    }

    private void HandleDashPreview()
    {
        if (dashPreview == null) return;

        Vector3 mousePos = Input.mousePosition;
        mousePos.z = -Camera.main.transform.position.z;
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(mousePos);
        Vector2 dashDir = ((Vector2)worldPos - rb.position).normalized;

        Vector2 start = rb.position;
        Vector2 end = start + dashDir * dashDistance;

        dashPreview.SetPosition(0, start);
        dashPreview.SetPosition(1, end);
    }

    private void HandleWallSlide()
    {
        if (wallCheckLeft == null || wallCheckRight == null)
        {
            isWallSliding = false;
            return;
        }

        bool leftTouch = Physics2D.OverlapCircle(wallCheckLeft.position, 0.1f, wallLayer);
        bool rightTouch = Physics2D.OverlapCircle(wallCheckRight.position, 0.1f, wallLayer);
        isWallSliding = (leftTouch || rightTouch) && !IsGrounded() && rb.velocity.y < 0f;

        if (isWallSliding)
            rb.velocity = new Vector2(rb.velocity.x, Mathf.Max(rb.velocity.y, -2f));
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    private void Flip()
    {
        if ((horizontal > 0 && !isFacingRight) || (horizontal < 0 && isFacingRight))
        {
            isFacingRight = !isFacingRight;
            Vector3 s = transform.localScale;
            s.x *= -1f;
            transform.localScale = s;
        }
    }

private void UpdateAnimations()
{
    if (anim == null) return;

    if (_animParams.Contains("isRunning"))
        anim.SetBool("isRunning", Mathf.Abs(horizontal) > 0.01f);

    if (_animParams.Contains("isGrounded"))
        anim.SetBool("isGrounded", IsGrounded());

    if (_animParams.Contains("isWallSliding"))
        anim.SetBool("isWallSliding", isWallSliding);

    if (_animParams.Contains("moveDirection"))
        anim.SetFloat("moveDirection", horizontal);
        horizontal = Input.GetAxisRaw("Horizontal");

    if (_animParams.Contains("verticalVelocity"))
            anim.SetFloat("verticalVelocity", rb.velocity.y);
}

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Object"))
        {
            deathParticles?.Play();
            Destroy(gameObject, 0.5f);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("collectable"))
        {
            cm.coinCount++;
            Destroy(other.gameObject);
        }
    }
}

