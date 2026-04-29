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
    
    public EnemyStats Stats => stats;

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
    
    private float patrolTargetX;
    private bool movingRight = true;

    private void HandleGroundEnemy()
    {
        float distance = Vector2.Distance(transform.position, player.position);
        float distanceX = Mathf.Abs(player.position.x - transform.position.x);
        float distanceY = player.position.y - transform.position.y;

        bool playerAbove = distanceY > stats.stopAboveHeight;

        // Игрок вне зоны
        if (distance > stats.detectionRadius)
        {
            Patrol();
            return;
        }

        // Игрок сверху
        if (playerAbove)
        {
            float leftPoint = player.position.x - stats.aboveOffsetRange;
            float rightPoint = player.position.x + stats.aboveOffsetRange;

            float dir = facingRight ? 1f : -1f;

            rb.linearVelocity = new Vector2(dir * stats.moveSpeed, rb.linearVelocity.y);
            animator.SetBool("IsMoving", true);

            // Мгновенный разворот у края маршрута
            if (transform.position.x >= rightPoint && facingRight)
                Flip();

            else if (transform.position.x <= leftPoint && !facingRight)
                Flip();

            return;
        }

        // Игрок на земле
        if (distanceX > stats.attackRange)
        {
            MoveToPlayer();
        }
        else
        {
            StopMoving();

            if (canAttack && Mathf.Abs(distanceY) <= stats.verticalAttackTolerance)
                StartCoroutine(AttackRoutine());
        }
    }
    
    private void HandleFlyingEnemy()
    {
        if (player == null) return;

        float distance = Vector2.Distance(transform.position, player.position);

        // Игрок слишком далеко — призрак просто висит
        if (distance > stats.detectionRadius)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Подлет к игроку
        if (distance > stats.hoverRadius)
        {
            Vector2 dir = (player.position - transform.position).normalized;
            rb.linearVelocity = dir * stats.approachSpeed;

            FacePlayer();
            return;
        }

        // Кружение вокруг игрока
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
        Vector2 direction = (target.position - transform.position);

        // Добавляем небольшую корректировку по Y, чтобы не застревать
        float stepY = Mathf.Clamp(direction.y, -0.05f, 0.05f);

        // Нормализуем по горизонтали, чтобы движение было ровное
        Vector2 moveDir = new Vector2(Mathf.Sign(direction.x), stepY);

        // Плавное движение к цели
        Vector2 newPos = rb.position + moveDir * stats.moveSpeed * Time.fixedDeltaTime;
        rb.MovePosition(newPos);

        // Анимация движения
        animator.SetBool("IsMoving", Mathf.Abs(direction.x) > 0.05f);

        // Поворот только если направление изменилось
        if ((moveDir.x > 0 && !facingRight) || (moveDir.x < 0 && facingRight))
            Flip();

        // Проверка, достигли ли точки
        if (Vector2.Distance(transform.position, target.position) < 0.1f)
            currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
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
        orbitAngle += stats.orbitSpeed * Time.fixedDeltaTime;

        float x = Mathf.Cos(orbitAngle) * stats.hoverRadius;
        float y = Mathf.Sin(orbitAngle) * stats.hoverRadius;

        y += Mathf.Sin(Time.time * 2f) * stats.orbitHeightVariation;

        Vector2 orbitPosition = (Vector2)player.position + new Vector2(x, y);

        Vector2 moveDir = orbitPosition - (Vector2)transform.position;

        if (moveDir.magnitude > 0.05f)
        {
            rb.linearVelocity = moveDir.normalized * stats.returnToOrbitSpeed;
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
        }
        
        Vector2 targetVelocity = moveDir.normalized * stats.returnToOrbitSpeed;

        rb.linearVelocity = Vector2.Lerp(rb.linearVelocity, targetVelocity, 5f * Time.fixedDeltaTime);

        FacePlayer();

        if (Random.value < stats.dashChance)
        {
            StartCoroutine(DashRoutine());
        }
    }
    
    private IEnumerator DashRoutine()
    {
        isDashing = true;

        Vector2 dashDir = (player.position - transform.position).normalized;

        rb.linearVelocity = dashDir * (stats.moveSpeed * stats.dashMultiplier);

        yield return new WaitForSeconds(stats.dashDuration);

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