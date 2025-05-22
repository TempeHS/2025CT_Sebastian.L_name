using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    private float horizontal;
    private float vertical;
    private float coyoteTime = 0.2f;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private float jumpBufferTime = 0.2f;

    [SerializeField] private float speed = 8f;
    [SerializeField] private float jumpingPower = 16f;
    private bool isFacingRight = true;

    private bool isDashing;
    private bool isJumping;

    private bool canHorizontalDash = true;
    private bool canVerticalDash = true;
    
    [SerializeField] private float dashingPower = 24f;
    [SerializeField] private float dashingTime = 0.2f;
    [SerializeField] private float horizontalDashCooldown = 1f; // Cooldown for horizontal dash
    [SerializeField] private float verticalDashCooldown = 2f; // Cooldown for vertical dash

    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private TrailRenderer tr;

    private void Update()
    {
        if (isDashing) return;
        horizontal = Input.GetAxisRaw("Horizontal");

        if (IsGrounded())
        {
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (Input.GetButtonDown("Jump"))
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        if (coyoteTimeCounter > 0f && jumpBufferCounter > 0f && !isJumping)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpingPower);

            jumpBufferCounter = 0f;

            StartCoroutine(JumpCooldown());
        }

        if (Input.GetButtonUp("Jump") && rb.velocity.y > 0f)
        {
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * 0.5f);

            coyoteTimeCounter = 0f;



            // Trigger different dash types based on input
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                if (vertical > 0 && canVerticalDash)
                    StartCoroutine(Dash(true)); // Vertical Dash
                else if (canHorizontalDash)
                    StartCoroutine(Dash(false)); // Horizontal Dash
            }

            Flip();
        }
    }

    private void FixedUpdate()
    {
        if (isDashing) return;
        rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    private void Flip()
    {
        if (isFacingRight && horizontal < 0f || !isFacingRight && horizontal > 0f)
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }

    private IEnumerator Dash(bool isVertical)
    {
        // Check cooldown before proceeding
        if (isVertical && !canVerticalDash) yield break;
        if (!isVertical && !canHorizontalDash) yield break;

        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0f;
        isDashing = true;

        // Adjust dash direction based on type
        float dashDirectionX = isVertical ? 0f : (horizontal != 0 ? horizontal : transform.localScale.x);
        float dashDirectionY = isVertical ? 1f : vertical;

        rb.velocity = new Vector2(dashDirectionX * dashingPower, dashDirectionY * dashingPower);

        tr.emitting = true;
        yield return new WaitForSeconds(dashingTime);
        tr.emitting = false;

        // **Fix: Reset velocity after dash**
        rb.velocity = Vector2.zero; // Prevents continued movement after dash ends

        rb.gravityScale = originalGravity;
        isDashing = false;

        // Apply cooldown separately based on dash type
        if (isVertical)
        {
            canVerticalDash = false;
            yield return new WaitForSeconds(verticalDashCooldown);
            canVerticalDash = true;
        }
        else
        {
            canHorizontalDash = false;
            yield return new WaitForSeconds(horizontalDashCooldown);
            canHorizontalDash = true;
        }
        
    }
   private IEnumerator JumpCooldown()
    {
        isJumping = true;
        yield return new WaitForSeconds(0.4f);
        isJumping = false;
    }
}