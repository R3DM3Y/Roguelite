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
    public float postHitInvulnerableTime = 1f; // время неуязвимости после касания земли
    public SpriteRenderer spriteRenderer;      // мигание
    public float blinkInterval = 0.1f;         // частота мигания

    private bool isInvulnerable;               // общий флаг неуязвимости
    private bool isHitLocked = false;          // блокировка действий после удара

    
    [HideInInspector] public bool IsAttacking;
    [HideInInspector] public bool sHeld;  // флаг, что S зажат
    [HideInInspector] public Animator animator;

    private bool facingRight = true;
    [HideInInspector] public float InputX;
    [HideInInspector] public float InputY;
    [HideInInspector] public bool JumpPressed; 
    [HideInInspector] public bool IsDropping;

    [HideInInspector] public Rigidbody2D rb;

    private Collider2D playerCollider;
    private bool isDropping = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerCollider = GetComponent<Collider2D>();
        currentHealth = maxHealth;
        
    }

    private void Update()
    {
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
        if (isInvulnerable) return;

        currentHealth -= damage;
        Debug.Log($"Игрок получил {damage} урона, осталось {currentHealth} HP");

        animator.SetTrigger("Hit");

        // блокируем действия и отталкиваем в сторону
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
        // Ждем, пока игрок не коснется земли
        while (!IsGrounded())
            yield return null;

        // После касания земли — пост-хит мигание
        float timer = 0f;
        while (timer < postHitInvulnerableTime)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(blinkInterval);
            timer += blinkInterval;
        }

        spriteRenderer.enabled = true;
        isInvulnerable = false;
        isHitLocked = false; // снимаем блокировку действий
    }

    
    private void ApplyKnockback(Vector2 direction)
    {
        rb.linearVelocity = Vector2.zero;

        // Удар сверху (игрок был над врагом)
        if (direction.y > 0.5f)
        {
            rb.linearVelocity = new Vector2(0, knockbackUpForce);
        }
        else
        {
            // Удар сбоку
            rb.linearVelocity = new Vector2(
                Mathf.Sign(direction.x) * knockbackSideForce,
                knockbackUpForce * 0.5f
            );
        }
    }

    private void Die()
    {
        Debug.Log("Игрок погиб");
        Destroy(gameObject);
    }

    public void Move(float x)
    {
        if (isHitLocked) return;

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
        Debug.DrawRay(
            wallCheck.position,
            Vector2.right * direction * wallCheckDistance,
            Color.red
        );
        return Physics2D.Raycast(
            wallCheck.position,
            Vector2.right * direction,
            wallCheckDistance,
            wallLayer
        );
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
    private void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
    }

    
    public void DropDown()
    {
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
    
}
