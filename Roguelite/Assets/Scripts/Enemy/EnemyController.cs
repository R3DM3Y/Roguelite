using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    #region === SETTINGS ===

    [Header("Health")]
    [SerializeField] private int maxHealth = 3;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private Transform[] patrolPoints;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    [Header("Detection & Attack")]
    [SerializeField] private float detectionRadius = 5f;
    [SerializeField] private float attackRange = 1.2f;
    [SerializeField] private float attackCooldown = 1.5f;
    [SerializeField] private EnemyAttackHitbox attackHitbox;

    [Header("Flip Settings")]
    [SerializeField] private float flipRadius = 0.5f; // горизонтальный радиус для разворота на игрока

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

    #region === UNITY METHODS ===

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        col = GetComponent<Collider2D>();
        currentHealth = maxHealth;
    }

    private void Start()
    {
        GameObject p = GameObject.FindGameObjectWithTag("Player");
        if (p != null)
            player = p.transform;
    }

    private void FixedUpdate()
    {
        if (isDead || player == null)
            return;

        // Считаем расстояние до игрока
        float distanceX = Mathf.Abs(player.position.x - transform.position.x);
        float distanceY = player.position.y - transform.position.y;

        bool playerInRange = IsPlayerInDetectionRange();

        if (!playerInRange)
        {
            // Если игрок вне радиуса — патрулируем
            Patrol();
            return;
        }

        // Если игрок слишком высоко над врагом и почти сверху → враг не двигается
        if (distanceY > 1.5f && distanceX < 0.5f)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            animator.SetBool("IsMoving", false);
            return;
        }

        // Двигаемся к игроку только по горизонтали
        if (distanceX > attackRange)
        {
            MoveToPlayer();
        }
        else
        {
            // Останавливаемся при достижении диапазона атаки
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            animator.SetBool("IsMoving", false);

            // Атакуем только если игрок "доступен" по вертикали
            if (canAttack && distanceY <= 1f)
            {
                StartCoroutine(AttackRoutine());
            }
        }
    }

    #endregion

    #region === MOVEMENT ===

    private void MoveToPlayer()
    {
        if (player == null) return;

        float dir = Mathf.Sign(player.position.x - transform.position.x);

        // Не подходить ближе, чем радиус атаки
        float distanceX = Mathf.Abs(player.position.x - transform.position.x);
        if (distanceX < attackRange)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            animator.SetBool("IsMoving", false);
            return;
        }

        rb.linearVelocity = new Vector2(dir * moveSpeed, rb.linearVelocity.y);
        animator.SetBool("IsMoving", true);

        HandleFlip(dir);
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

        rb.linearVelocity = new Vector2(dir * moveSpeed, rb.linearVelocity.y);
        animator.SetBool("IsMoving", true);

        if (Mathf.Abs(transform.position.x - target.position.x) < 0.1f)
            currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;

        // Патруль не зависит от игрока
        HandleFlip(dir);
    }

    private bool IsGroundAhead(float dir)
    {
        if (groundCheck == null)
            return false;

        Vector2 pos = (Vector2)groundCheck.position + Vector2.right * (dir * 0.2f);
        return Physics2D.OverlapBox(pos, new Vector2(0.3f, 0.15f), 0f, groundLayer);
    }

    private void HandleFlip(float dir)
    {
        if ((dir > 0 && !facingRight) || (dir < 0 && facingRight))
            Flip();
    }

    private void HandleFlipToPlayer()
    {
        if (player == null) return;

        float horizontalDistance = player.position.x - transform.position.x;

        // Если игрок слишком близко по горизонтали — не переворачиваем
        if (Mathf.Abs(horizontalDistance) < flipRadius) 
            return;

        // Переворачиваем только если игрок слева/справа за пределами радиуса
        if (horizontalDistance > 0 && !facingRight)
            Flip();
        else if (horizontalDistance < 0 && facingRight)
            Flip();
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

        rb.linearVelocity = Vector2.zero;
        animator.SetBool("IsMoving", false);

        FacePlayer();
        animator.SetTrigger("Attack");

        yield return new WaitForSeconds(attackCooldown);

        canAttack = true;
    }

    public void ActivateAttackHitbox()
    {
        if (attackHitbox != null)
            attackHitbox.Activate();
    }

    public void DeactivateAttackHitbox()
    {
        if (attackHitbox != null)
            attackHitbox.Deactivate();
    }

    private void FacePlayer()
    {
        if (player == null)
            return;

        if (player.position.x > transform.position.x && !facingRight)
            Flip();
        else if (player.position.x < transform.position.x && facingRight)
            Flip();
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

        rb.linearVelocity = Vector2.zero;
        rb.simulated = false;
        col.enabled = false;

        animator.SetBool("IsMoving", false);
        animator.SetTrigger("Die");

        // Удаляем точки патрулирования, если они есть
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

    #region === DETECTION ===

    private bool IsPlayerInDetectionRange()
    {
        if (player == null)
            return false;

        PlayerController pc = player.GetComponent<PlayerController>();
        if (pc == null || pc.IsDead)
            return false;

        return Vector2.Distance(transform.position, player.position) <= detectionRadius;
    }

    #endregion
}