/*
using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // ---  Player Movement ---
    private float horizontal;
    private float vertical;

    // ---  Jump Timing  ---
    private float coyoteTime = 0.2f;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private float jumpBufferTime = 0.2f;

    // --- Movement Physics Settings ---
    [SerializeField] private float speed = 8f;
    [SerializeField] private float jumpingPower = 16f;
    private bool isFacingRight = true;

    // ---  Dash System ---
    private bool isDashing;
    private bool isJumping;
    private bool hasDashed;

    [SerializeField] private float dashingPower = 40f;
    [SerializeField] private float dashingTime = 0.3f;

    // ---  Unity Components ---
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private TrailRenderer tr;

    private void Update()
    {
        if (isDashing) return;

        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");

        HandleJump();
        HandleDash();
        Flip();
    }

    private void FixedUpdate()
    {
        if (isDashing) return;
        rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);

        if (IsGrounded()) hasDashed = false;
    }

    // ---  Jump ---
    private void HandleJump()
    {
        if (IsGrounded())
            coyoteTimeCounter = coyoteTime;
        else
            coyoteTimeCounter -= Time.deltaTime;

        if (Input.GetButtonDown("Jump"))
            jumpBufferCounter = jumpBufferTime;
        else
            jumpBufferCounter -= Time.deltaTime;

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
        }

        if (!isDashing)
            rb.velocity = new Vector2(horizontal * speed * 0.8f, rb.velocity.y);
    }

    // ---  Omnidirectional Dash ---
    private void HandleDash()
    {
        if (!Input.GetKeyDown(KeyCode.LeftShift) || hasDashed) return;

        Vector2 dashDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (dashDirection == Vector2.zero)
            dashDirection = isFacingRight ? Vector2.right : Vector2.left;

        StartCoroutine(Dash(dashDirection));
    }

    // ---  Ground Check ---
    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    // ---  Flip Character Direction ---
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

    // --- 🚀 Omnidirectional Dash Execution ---
    private IEnumerator Dash(Vector2 direction)
    {
        hasDashed = true;
        isDashing = true;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0.3f; // Retain slight fall effect during dash

        rb.velocity = direction.normalized * dashingPower;

        tr.emitting = true;
        yield return new WaitForSeconds(dashingTime);
        tr.emitting = false;

        rb.velocity *= 0.6f; // Preserves slight momentum after dash for fluidity
        rb.gravityScale = originalGravity;
        isDashing = false;
    }

    // --- ⏳ Jump Cooldown Logic ---
    private IEnumerator JumpCooldown()
    {
        isJumping = true;
        yield return new WaitForSeconds(0.4f);
        isJumping = false;
    }
}
*/

/*
using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // --- 🏃 Player Movement Variables ---
    private float horizontal;
    private float vertical;

    // --- ⏳ Jump Timing Variables ---
    private float coyoteTime = 0.2f; 
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private float jumpBufferTime = 0.2f; // Jump buffering duration

    // --- ⚙️ Movement & Physics Settings ---
    [SerializeField] private float speed = 8f;         
    [SerializeField] private float jumpingPower = 16f;  
    private bool isFacingRight = true;                

    // --- ✨ Dash System ---
    private bool isDashing;          
    private bool isJumping;          
    private bool hasDashed;          
    
    [SerializeField] private float dashingPower = 40f;  
    [SerializeField] private float dashingTime = 0.3f;  

    // --- 🔧 Unity Components ---
    [SerializeField] private Rigidbody2D rb;        
    [SerializeField] private Transform groundCheck; 
    [SerializeField] private LayerMask groundLayer; 
    [SerializeField] private TrailRenderer tr;      

    private void Update()
    {
        if (isDashing) return;
        
        horizontal = Input.GetAxisRaw("Horizontal"); 
        vertical = Input.GetAxisRaw("Vertical");     

        HandleJumpBuffering(); // Buffers jump inputs
        HandleJump(); 
        HandleDash(); 
        Flip();      
    }

    private void FixedUpdate()
    {
        if (isDashing) return;
        rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);

        if (IsGrounded()) 
        {
            hasDashed = false; 
            jumpBufferCounter = 0f; // Reset jump buffer when grounded
        }
    }

    // --- 🔄 Jump Buffering ---
    private void HandleJumpBuffering()
    {
        if (Input.GetButtonDown("Jump"))
            jumpBufferCounter = jumpBufferTime; // Stores jump input temporarily
        else
            jumpBufferCounter -= Time.deltaTime; // Decreases buffer timer
    }

    // --- 🦘 Jump Logic ---
    private void HandleJump()
    {
        if (IsGrounded())
            coyoteTimeCounter = coyoteTime;
        else
            coyoteTimeCounter -= Time.deltaTime;

        // Check if buffered jump should be executed
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
        }

        if (!isDashing)
            rb.velocity = new Vector2(horizontal * speed * 0.8f, rb.velocity.y); 
    }

    // --- ⚡ Omnidirectional Dash Logic ---
    private void HandleDash()
    {
        if (!Input.GetKeyDown(KeyCode.LeftShift) || hasDashed) return;

        Vector2 dashDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (dashDirection == Vector2.zero)
            dashDirection = isFacingRight ? Vector2.right : Vector2.left;

        StartCoroutine(Dash(dashDirection));
    }

    // --- 🏗 Ground Check Logic ---
    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
    }

    // --- 🔄 Flip Character Direction ---
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

    // --- 🚀 Omnidirectional Dash Execution ---
    private IEnumerator Dash(Vector2 direction)
    {
        hasDashed = true;
        isDashing = true;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0.3f; 

        rb.velocity = direction.normalized * dashingPower; 

        tr.emitting = true; 
        yield return new WaitForSeconds(dashingTime);
        tr.emitting = false;

        rb.velocity *= 0.6f; // Preserves slight momentum after dash
        rb.gravityScale = originalGravity;
        isDashing = false;
    }

    // --- ⏳ Jump Cooldown Logic ---
    private IEnumerator JumpCooldown()
    {
        isJumping = true;
        yield return new WaitForSeconds(0.4f); 
        isJumping = false;
    }
}
*/
using System.Collections;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CoinManager cm;
    // --- 🏃 Player Movement Variables ---
    private float horizontal;
    private float vertical;

    // --- ⏳ Jump Timing Variables ---
    private float coyoteTime = 0.2f;
    private float coyoteTimeCounter;
    private float jumpBufferCounter;
    private float jumpBufferTime = 0.2f;
    
    // --- ⚙️ Movement & Physics Settings ---
    [SerializeField] private float maxSpeed = 8f;
    [SerializeField] private float acceleration = 60f;
    [SerializeField] private float deceleration = 30f;
    [SerializeField] private float jumpingPower = 16f;
    private bool isFacingRight = true;

    // --- ✨ Dash System ---
    private bool isDashing;
    private bool isJumping;
    private bool hasDashed;

    [SerializeField] private float dashingPower = 40f;
    [SerializeField] private float dashingTime = 0.3f;

    // --- 🔧 Unity Components ---
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private TrailRenderer tr;

    private void Update()
    {
        if (isDashing) return;

        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");

        HandleJumpBuffering();
        HandleJump();
        HandleDash();
        Flip();
    }

    private void FixedUpdate()
    {
        if (isDashing) return;

        // Smooth acceleration/deceleration logic
        float targetSpeed = horizontal * maxSpeed;
        float speedDiff = targetSpeed - rb.velocity.x;
        float accelRate = Mathf.Abs(targetSpeed) > 0.01f ? acceleration : deceleration;
        float force = speedDiff * accelRate;

        rb.AddForce(new Vector2(force, 0f));

        // Clamp max horizontal speed
        if (Mathf.Abs(rb.velocity.x) > maxSpeed)
            rb.velocity = new Vector2(Mathf.Sign(rb.velocity.x) * maxSpeed, rb.velocity.y);

        if (IsGrounded())
        {
            hasDashed = false;
            jumpBufferCounter = 0f;
        }
    }

    private void HandleJumpBuffering()
    {
        if (Input.GetButtonDown("Jump"))
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
        }
    }

    private void HandleDash()
    {
        if (!Input.GetKeyDown(KeyCode.LeftShift) || hasDashed) return;

        Vector2 dashDirection = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        if (dashDirection == Vector2.zero)
            dashDirection = isFacingRight ? Vector2.right : Vector2.left;

        StartCoroutine(Dash(dashDirection));
    }

    private IEnumerator Dash(Vector2 direction)
    {
        hasDashed = true;
        isDashing = true;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0.3f;

        rb.velocity = direction.normalized * dashingPower;
        tr.emitting = true;

        yield return new WaitForSeconds(dashingTime);

        tr.emitting = false;
        rb.velocity *= 0.6f;
        rb.gravityScale = originalGravity;
        isDashing = false;
    }

    private bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
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

    private IEnumerator JumpCooldown()
    {
        isJumping = true;
        yield return new WaitForSeconds(0.4f);
        isJumping = false;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("collectable"))
            Destroy(other.gameObject);
            cm.coinCount++;
    }

}