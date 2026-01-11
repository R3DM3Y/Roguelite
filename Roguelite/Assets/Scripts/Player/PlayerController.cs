using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 5;
    private int currentHealth;

    [Header("Movement")]
    public float moveSpeed = 5f;

    [Header("Jump")]
    public float jumpForce = 7f;
    
    [Header("Ground Check")]
    public Transform groundCheck;
    public Vector2 groundCheckSize = new Vector2(0.8f, 0.1f);
    public LayerMask groundLayer;

    [Header("DropDown")]
    public float dropDownTime = 0.2f;
    
    [Header("Wall Check")]
    public Transform wallCheck;
    public float wallCheckDistance = 0.1f;
    public LayerMask wallLayer;
    
    [Header("Knockback")]
    public float knockbackSideForce = 6f;
    public float knockbackUpForce = 8f;
    
    [Header("Post-Hit Invulnerability")]
    public float postHitInvulnerableTime = 1f; 
    public SpriteRenderer spriteRenderer;      
    public float blinkInterval = 0.1f;         

    private bool isInvulnerable;
    [HideInInspector] public bool isHitLocked = false;          

    [Header("Attack")]
    public int normalAttackDamage = 1;
    public PlayerAttackHitbox attackHitbox;
    [HideInInspector] public Animator animator;
    
    [HideInInspector] public bool sHeld;  
    
    [Header("Air Down Attack")]
    public int airDownAttackDamage = 1;
    public PlayerAttackHitbox airDownHitbox; 
    [HideInInspector] public bool isAirAttackingDown;
    public float airDownBounceForce = 6f;
    
    [HideInInspector] public bool IsAttackingNormal;
    [HideInInspector] public bool IsAttackingDown;
    
    private bool facingRight = true;
    [HideInInspector] public float InputX;
    [HideInInspector] public float InputY;
    [HideInInspector] public bool JumpPressed; 
    [HideInInspector] public bool IsDropping;
    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public bool IsDead;

    private Collider2D playerCollider;
    private bool isDropping = false;
    
    private bool airDownHitboxActivated = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerCollider = GetComponent<Collider2D>();
        currentHealth = maxHealth;
        
    }

    private void Update()
    {
        if (IsDead) return;
        UpdateAnimations();
    }

    private void UpdateAnimations()
    {
        bool grounded = IsGrounded();
        animator.SetBool("IsGrounded", grounded);
        animator.SetBool("InAir", !grounded);
        animator.SetFloat("Speed", Mathf.Abs(rb.linearVelocity.x));
    }
    
    public void TakeDamage(int damage, Vector2 hitDirection)
    {
        if (IsDead || isInvulnerable) return;

        currentHealth -= damage;
        
        CancelAllAttacks();

        animator.SetTrigger("Hit");

        
        isHitLocked = true;
        isInvulnerable = true;

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(new Vector2(Mathf.Sign(hitDirection.x) * knockbackSideForce, 0), ForceMode2D.Impulse);

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        StartCoroutine(WaitForGroundAfterHit());
    }
    
    private IEnumerator WaitForGroundAfterHit()
    {
        while (!IsGrounded())
            yield return null;
        
        float timer = 0f;
        while (timer < postHitInvulnerableTime)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(blinkInterval);
            timer += blinkInterval;
        }

        spriteRenderer.enabled = true;
        isInvulnerable = false;
        isHitLocked = false; 
    }
    
    private void CancelAllAttacks()
    {
        IsAttackingNormal = false;
        IsAttackingDown = false;

        if (attackHitbox != null)
            attackHitbox.DisableImmediately();

        if (airDownHitbox != null)
            airDownHitbox.DisableImmediately();
    }
    
    private void ApplyKnockback(Vector2 direction)
    {
        rb.linearVelocity = Vector2.zero;
        
        if (direction.y > 0.5f)
        {
            rb.linearVelocity = new Vector2(0, knockbackUpForce);
        }
        else
        {
            rb.linearVelocity = new Vector2(
                Mathf.Sign(direction.x) * knockbackSideForce,
                knockbackUpForce * 0.5f
            );
        }
    }

    private void Die()
    {
        if (IsDead) return;

        IsDead = true;
        isHitLocked = true;
        isInvulnerable = true;

        CancelAllAttacks();

        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 1.5f;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        animator.SetTrigger("Die");
    }
    
    public void OnDeathAnimationFinished()
    {
        Destroy(gameObject);
    }

    public void Move(float x)
    {
        if (IsDead || isHitLocked) return;

        float moveX = x;

        if (Mathf.Abs(x) > 0.01f)
        {
            float dir = Mathf.Sign(x);

            if (IsTouchingWall(dir))
            {
                moveX = 0;
            }
        }

        rb.linearVelocity = new Vector2(moveX * moveSpeed, rb.linearVelocity.y);

        if (moveX > 0 && !facingRight) Flip();
        else if (moveX < 0 && facingRight) Flip();
    }
    
    public bool IsTouchingWall(float direction)
    {
        Vector2 boxSize = new Vector2(0.1f, 1.45f); 
        Vector2 boxCenter = (Vector2)wallCheck.position + Vector2.right * (direction * (boxSize.x / 2f));

        Collider2D[] hits = Physics2D.OverlapBoxAll(
            boxCenter,
            boxSize,
            0f,
            wallLayer
        );

        foreach (var hit in hits)
        {
            if (hit.CompareTag("OneWayPlatform")) continue; 
            return true;
        }

        return false;
    }
    
    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    public void Jump()
    {
        if (IsDead || isHitLocked) return;
        
        if (IsGrounded())
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            animator.SetBool("InAir", true);
        }
    }

    public bool IsGrounded()
    {
        return Physics2D.OverlapBox(
            groundCheck.position,
            groundCheckSize,
            0f,
            groundLayer
        );
    }
    
    public void DropDown()
    {
        if (IsDead) return;
        if (isDropping || !IsStandingOnOneWayPlatform()) return;
        StartCoroutine(DisableColliderTemporarily());
    }

    private bool IsStandingOnOneWayPlatform()
    {
        Collider2D[] hits = Physics2D.OverlapBoxAll(
            groundCheck.position,
            groundCheckSize,
            0f,
            groundLayer
        );

        foreach (var hit in hits)
        {
            if (hit.CompareTag("OneWayPlatform") && rb.linearVelocity.y <= 0f)
                return true;
        }

        return false;
    }

    private IEnumerator DisableColliderTemporarily()
    {
        isDropping = true;
        playerCollider.enabled = false;
        yield return new WaitForFixedUpdate();
        yield return new WaitForSeconds(dropDownTime);
        playerCollider.enabled = true;
        isDropping = false;
    }
    
    public void StartAttack()
    {
        if (IsDead || isHitLocked) return;
        if (IsAttackingNormal || IsAttackingDown) return; 
        if (!IsGrounded() && sHeld) return; 
    
        IsAttackingNormal = true;
        animator.SetTrigger("Attack");
    }

    public void ActivateAttackHitbox()
    {
        attackHitbox.ActivateHitbox();
    }

    public void EndAttackNormal()
    {
        IsAttackingNormal = false;
    }
    
    public void StartAirAttackDown()
    {
        if (IsDead || isHitLocked) return;
        if (IsGrounded() || !sHeld || IsAttackingDown) return;

        IsAttackingDown = true;
        animator.SetTrigger("AirAttackDown");
    }
    
    public void OnAirDownHitSuccess()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, airDownBounceForce);
    }
    
    public void ActivateAirDownHitbox()
    {
        if (airDownHitbox != null)
        {
            airDownHitbox.ActivateHitbox();
        }
    }

    public void EndAttackDown()
    {
        IsAttackingDown = false;
    }
}
