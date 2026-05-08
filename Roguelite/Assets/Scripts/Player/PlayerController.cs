using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Stats")]
    [SerializeField] private PlayerStats stats;
    
    [Header("Health")]
    private int currentHealth;
    
    [Header("Shield")]
    public bool isShielding;

    public PlayerStamina stamina;
    
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
    
    [Header("Post-Hit Invulnerability")]
    public SpriteRenderer spriteRenderer;      

    private bool isInvulnerable;
    [HideInInspector] public bool isHitLocked = false;          

    [Header("Attack")]
    public PlayerAttackHitbox attackHitbox;
    [HideInInspector] public Animator animator;
    
    [HideInInspector] public bool sHeld;  
    
    [Header("Air Down Attack")]
    public PlayerAttackHitbox airDownHitbox; 
    [HideInInspector] public bool isAirAttackingDown;
    
    [HideInInspector] public bool IsAttackingNormal;
    [HideInInspector] public bool IsAttackingDown;
    
    private bool facingRight = true;
    [HideInInspector] public float InputX;
    [HideInInspector] public float InputY;
    [HideInInspector] public bool JumpPressed; 
    [HideInInspector] public bool IsDropping;
    [HideInInspector] public Rigidbody2D rb;
    [HideInInspector] public bool IsDead;
    public GameObject damageTextPrefab;
    [SerializeField] private Transform headPoint;

    private Collider2D playerCollider;
    private bool isDropping = false;
    
    private bool airDownHitboxActivated = false;
    
    public int CurrentHealth => currentHealth;
    public System.Action OnHealthChanged;
    
    public int NormalAttackDamage => stats.normalAttackDamage;
    public int AirDownAttackDamage => stats.airDownAttackDamage;
    public int MaxHealth => stats.maxHealth;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        playerCollider = GetComponent<Collider2D>();
        currentHealth = stats.maxHealth;
        stamina = GetComponent<PlayerStamina>();
    }

    private void Update()
    {
        if (IsDead) return;
        UpdateAnimations();
        
        if (isShielding)
        {
            if (stamina.IsEmpty)
            {
                StopShield();
            }
            else
            {
                stamina.Use(stats.shieldDrainPerSecond * Time.deltaTime);
            }
        }
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

        if (isShielding)
        {
            stamina.Use(stats.shieldHitDrain);

            damage = Mathf.CeilToInt(damage * (1f - stats.damageReduction));

            if (stamina.IsEmpty)
            {
                StopShield();
            }
        }

        currentHealth -= damage;
        OnHealthChanged?.Invoke();
        
        CancelAllAttacks();

        animator.SetTrigger("Hit");

        
        isHitLocked = true;
        isInvulnerable = true;

        rb.linearVelocity = Vector2.zero;
        rb.AddForce(new Vector2(Mathf.Sign(hitDirection.x) * stats.knockbackSideForce, 0), ForceMode2D.Impulse);

        if (currentHealth <= 0)
        {
            Die();
            return;
        }

        StartCoroutine(WaitForGroundAfterHit());
        SpawnDamageText(damage);
    }
    
    void SpawnDamageText(int damage)
    {
        Vector3 offset = Vector3.up * 0.2f;

        GameObject obj = Instantiate(damageTextPrefab, headPoint.position + offset, Quaternion.identity);
        obj.GetComponent<DamageText>().SetDamage(damage);
    }
    
    private IEnumerator WaitForGroundAfterHit()
    {
        while (!IsGrounded())
            yield return null;
        
        float timer = 0f;
        while (timer < stats.postHitInvulnerableTime)
        {
            spriteRenderer.enabled = !spriteRenderer.enabled;
            yield return new WaitForSeconds(stats.blinkInterval);
            timer += stats.blinkInterval;
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
            rb.linearVelocity = new Vector2(0, stats.knockbackUpForce);
        }
        else
        {
            rb.linearVelocity = new Vector2(
                Mathf.Sign(direction.x) * stats.knockbackUpForce,
                stats.knockbackUpForce * 0.5f
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
            float dir = Mathf.Sign(x); if (IsTouchingWall(dir)) { moveX = 0; } 
            
        } 
        
        float speed = stats.moveSpeed;

        if (isShielding)
        {
            speed *= stats.shieldMoveSpeedMultiplier;
        }

        rb.linearVelocity = new Vector2(moveX * speed, rb.linearVelocity.y);

        if (moveX > 0 && !facingRight)
        {
            Flip();
        } 
        else if (moveX < 0 && facingRight)
        {
            Flip();
        } 
        
    }

    public bool IsTouchingWall(float direction)
    {
        RaycastHit2D[] hits = new RaycastHit2D[2];
        ContactFilter2D filter = new ContactFilter2D();
        filter.SetLayerMask(wallLayer);
        filter.useTriggers = false;

        int count = playerCollider.Cast(
            Vector2.right * direction,
            filter,
            hits,
            wallCheckDistance
        );

        for (int i = 0; i < count; i++)
        {
            if (hits[i].collider.CompareTag("OneWayPlatform"))
                count--; 
        }

        return count > 0;
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
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, stats.jumpForce);
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
    
    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireCube(groundCheck.position, groundCheckSize);
        }

        if (wallCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(wallCheck.position, wallCheck.position + Vector3.right * wallCheckDistance);
        }
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
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, stats.airDownBounceForce);
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
    
    public void StartShield()
    {
        if (IsDead) return;
        if (stamina.IsEmpty) return;

        isShielding = true;
        animator.SetBool("Shield", true);
    }

    public void StopShield()
    {
        isShielding = false;
        animator.SetBool("Shield", false);
    }
}
