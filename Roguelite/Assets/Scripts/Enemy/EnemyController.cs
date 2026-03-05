using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    #region === REFERENCES ===

    [SerializeField] private EnemyStats stats;
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private EnemyAttackHitbox attackHitbox;
    
    private float orbitAngle;
    private bool isDashing;

    [SerializeField] private float hoverRadius = 2f;
    [SerializeField] private float hoverSpeed = 2f;
    [SerializeField] private float dashSpeedMultiplier = 4f;
    [SerializeField] private float dashDuration = 0.4f;
    [SerializeField] private Vector2 minBounds;
    [SerializeField] private Vector2 maxBounds;

    #endregion

    #region === PRIVATE FIELDS ===

    private int currentHealth;
    private int currentPointIndex;

    private bool canAttack = true;
    private bool isDead;
    private bool facingRight = true;

    private Rigidbody2D rb;
    private Animator animator;
    private Collider2D col;
    private Transform player;

    #endregion

    #region === UNITY ===

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
    }

    private void Start()
    {
        currentHealth = stats.maxHealth;

        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            player = p.transform;
    }

    private void FixedUpdate()
    {
        if (isDead || player == null)
            return;

        if (stats.movementType == EnemyStats.MovementType.Ground)
        {
            HandleGroundEnemy();
        }
        else if (stats.movementType == EnemyStats.MovementType.Flying)
        {
            HandleFlyingEnemy();
        }
    }
    
    private void HandleGroundEnemy()
    {
        float distance = Vector2.Distance(transform.position, player.position);
        float distanceX = Mathf.Abs(player.position.x - transform.position.x);
        float distanceY = player.position.y - transform.position.y;

        bool playerInRange = distance <= stats.detectionRadius;

        if (!playerInRange)
        {
            if (stats.canPatrol)
                Patrol();
            return;
        }

        if (stats.stopIfPlayerAbove &&
            distanceY > stats.stopAboveHeight &&
            distanceX < 0.5f)
        {
            StopMoving();
            return;
        }

        if (distanceX > stats.attackRange)
        {
            MoveToPlayer();
        }
        else
        {
            StopMoving();

            if (canAttack &&
                Mathf.Abs(distanceY) <= stats.verticalAttackTolerance)
            {
                StartCoroutine(AttackRoutine());
            }
        }
    }
    
    private void HandleFlyingEnemy()
    {
        if (player == null) return;

        if (!isDashing)
        {
            OrbitAroundPlayer();
        }
    }

    #endregion

    #region === MOVEMENT ===

    private void MoveToPlayer()
    {
        float dir = Mathf.Sign(player.position.x - transform.position.x);

        rb.linearVelocity = new Vector2(dir * stats.moveSpeed, rb.linearVelocity.y);
        animator.SetBool("IsMoving", true);

        HandleFlipToPlayer();
    }

    private void Patrol()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
            return;

        Transform target = patrolPoints[currentPointIndex];
        float dir = Mathf.Sign(target.position.x - transform.position.x);

        if (!IsGroundAhead(dir))
        {
            Flip();
            return;
        }

        rb.linearVelocity = new Vector2(dir * stats.moveSpeed, rb.linearVelocity.y);
        animator.SetBool("IsMoving", true);

        if (Mathf.Abs(transform.position.x - target.position.x) < 0.1f)
            currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;

        HandleFlip(dir);
    }

    private void StopMoving()
    {
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        animator.SetBool("IsMoving", false);
    }

    private bool IsGroundAhead(float dir)
    {
        if (groundCheck == null)
            return false;

        Vector2 pos = (Vector2)groundCheck.position + Vector2.right * (dir * 0.2f);
        return Physics2D.OverlapBox(pos, new Vector2(0.3f, 0.15f), 0f, groundLayer);
    }
    
    private void OrbitAroundPlayer()
    {
        orbitAngle += hoverSpeed * Time.fixedDeltaTime;

        float x = Mathf.Cos(orbitAngle) * hoverRadius;
        float y = Mathf.Sin(orbitAngle) * hoverRadius;

        Vector2 orbitPosition = (Vector2)player.position + new Vector2(x, y);

        Vector2 moveDir = (orbitPosition - (Vector2)transform.position);
        rb.linearVelocity = moveDir * 5f; // скорость возврата на орбиту

        FacePlayer();

        // Рандомный момент для дэша
        if (Random.value < 0.005f)
        {
            StartCoroutine(DashRoutine());
        }
    }
    
    private IEnumerator DashRoutine()
    {
        isDashing = true;

        Vector2 dashDir = (player.position - transform.position).normalized;
        rb.linearVelocity = dashDir * hoverSpeed * dashSpeedMultiplier;

        yield return new WaitForSeconds(dashDuration);

        isDashing = false;
    }

    public void FacePlayer()
    {
        if (player == null) return;

        float dir = player.position.x - transform.position.x;

        if ((dir > 0 && !facingRight) ||
            (dir < 0 && facingRight))
        {
            Flip();
        }
    }

    #endregion

    #region === FLIP ===

    private void HandleFlip(float dir)
    {
        if ((dir > 0 && !facingRight) || (dir < 0 && facingRight))
            Flip();
    }

    private void HandleFlipToPlayer()
    {
        float horizontalDistance = player.position.x - transform.position.x;

        if (Mathf.Abs(horizontalDistance) < stats.flipRadius)
            return;

        if ((horizontalDistance > 0 && !facingRight) ||
            (horizontalDistance < 0 && facingRight))
        {
            Flip();
        }
    }

    private void Flip()
    {
        facingRight = !facingRight;

        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    #endregion

    #region === ATTACK ===

    private IEnumerator AttackRoutine()
    {
        canAttack = false;

        StopMoving();
        FacePlayer();
        animator.SetTrigger("Attack");

        yield return new WaitForSeconds(stats.attackCooldown);

        canAttack = true;
    }

    public void ActivateAttackHitbox()
    {
        if (attackHitbox != null)
        {
            attackHitbox.SetDamage(stats.attackDamage);
            attackHitbox.Activate();
        }
    }

    public void DeactivateAttackHitbox()
    {
        if (attackHitbox != null)
            attackHitbox.Deactivate();
    }

    #endregion

    #region === HEALTH ===

    public void TakeDamage(int damage)
    {
        if (isDead)
            return;

        currentHealth -= damage;
        animator.SetTrigger("Hit");

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        isDead = true;

        StopMoving();
        rb.simulated = false;
        col.enabled = false;

        animator.SetTrigger("Die");

        if (patrolPoints != null)
        {
            foreach (var point in patrolPoints)
            {
                if (point != null)
                    Destroy(point.gameObject);
            }
        }
    }

    public void OnDeathAnimationFinished()
    {
        Destroy(gameObject);
    }

    #endregion
}