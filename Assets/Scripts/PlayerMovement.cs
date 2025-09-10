using System.Collections;
using System.Collections.Generic;    // ← for HashSet
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CoinManager cm;

    // --- Movement Input ---
    private float horizontal;

    // --- Jump Timing ---
    private float coyoteTime = 0.2f, coyoteTimeCounter;
    private float jumpBufferTime = 0.2f, jumpBufferCounter;

    // --- Movement Settings ---
    [SerializeField] private float maxSpeed = 8f;
    [SerializeField] private float acceleration = 60f;
    [SerializeField] private float deceleration = 30f;
    [SerializeField] private float airControlMultiplier = 0.5f;
    [SerializeField] private float jumpingPower = 16f;

    // --- Dash Settings ---
    [SerializeField] private float dashDistance = 5f;
    [SerializeField] private float dashDuration = 0.15f;
    [SerializeField] private float dashCooldown = 1f;
    private float dashCooldownTimer;
    private bool isDashing, hasDashed;

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

    // ← Declare the HashSet to cache animator parameters
    private HashSet<string> _animParams;

    private void Awake()
    {
        anim = GetComponent<Animator>();

        // Populate the HashSet with all parameter names
        _animParams = new HashSet<string>();
        foreach (var p in anim.parameters)
            _animParams.Add(p.name);
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

            // Dash cooldown
            dashCooldownTimer -= Time.deltaTime;

            HandleWallSlide();
            HandleJump();
            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.LeftShift))
                TryDash();
            Flip();
        }

        UpdateAnimations();
    }

    private void FixedUpdate()
    {
        if (isDashing) return;

        // Smooth accel/decel
        float targetSpeed = horizontal * maxSpeed;
        float speedDiff = targetSpeed - rb.velocity.x;
        float accelRate = Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration;
        float control = IsGrounded() ? 1f : airControlMultiplier;
        rb.AddForce(Vector2.right * speedDiff * accelRate * control);

        // Clamp speed
        if (Mathf.Abs(rb.velocity.x) > maxSpeed)
            rb.velocity = new Vector2(Mathf.Sign(rb.velocity.x) * maxSpeed, rb.velocity.y);

        // Reset dash availability when grounded
        if (IsGrounded())
            hasDashed = false;
    }

    private void HandleJump()
    {
        // Coyote time countdown
        if (IsGrounded()) coyoteTimeCounter = coyoteTime;
        else coyoteTimeCounter -= Time.deltaTime;

        // Execute jump when buffered & in coyote window
        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpingPower);
            jumpBufferCounter = 0f;
            jumpParticles.Play();
        }
    }

    private void TryDash()
    {
        if (hasDashed || dashCooldownTimer > 0f) return;

        Vector3 m = Input.mousePosition;
        m.z = -Camera.main.transform.position.z;
        Vector3 world = Camera.main.ScreenToWorldPoint(m);
        Vector2 dir = ((Vector2)world - rb.position).normalized;

        StartCoroutine(DashRoutine(dir));
        dashCooldownTimer = dashCooldown;
        hasDashed = true;
    }

    private IEnumerator DashRoutine(Vector2 dir)
    {
        isDashing = true;
        float origGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        dashParticles.Play();

        Vector2 start = rb.position;
        Vector2 end = start + dir * dashDistance;
        float elapsed = 0f;

        while (elapsed < dashDuration)
        {
            rb.MovePosition(Vector2.Lerp(start, end, elapsed / dashDuration));
            elapsed += Time.deltaTime;
            yield return null;
        }

        rb.MovePosition(end);
        rb.gravityScale = origGravity;
        isDashing = false;
    }

    private void HandleWallSlide()
    {
        if (wallCheckLeft == null || wallCheckRight == null)
        {
            Debug.LogWarning(
                "Wall-check transforms not assigned on PlayerMovement. Skipping wall slide.",
                this
            );
            isWallSliding = false;
            return;
        }

        bool leftTouch = Physics2D.OverlapCircle(
            wallCheckLeft.position, 0.1f, wallLayer
        );
        bool rightTouch = Physics2D.OverlapCircle(
            wallCheckRight.position, 0.1f, wallLayer
        );
        isWallSliding = (leftTouch || rightTouch)
                        && !IsGrounded()
                        && rb.velocity.y < 0f;

        if (isWallSliding)
            rb.velocity = new Vector2(
                rb.velocity.x,
                Mathf.Max(rb.velocity.y, -2f)
            );
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(
            groundCheck.position, 0.2f, groundLayer
        );
    }

    private void Flip()
    {
        if ((horizontal > 0 && !isFacingRight) ||
            (horizontal < 0 && isFacingRight))
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

        // Only set these if they exist in the Animator
        if (_animParams.Contains("isRunning"))
            anim.SetBool("isRunning", Mathf.Abs(horizontal) > 0.01f);

        if (_animParams.Contains("isGrounded"))
            anim.SetBool("isGrounded", IsGrounded());

        if (_animParams.Contains("isWallSliding"))
            anim.SetBool("isWallSliding", isWallSliding);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Object"))
        {
            deathParticles.Play();
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
