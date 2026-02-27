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

        if (!IsPlayerInDetectionRange())
        {
            Patrol();
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        if (distance > attackRange)
        {
            MoveToPlayer();
        }
        else if (canAttack)
        {
            StartCoroutine(AttackRoutine());
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("IsMoving", false);
        }
    }

    #endregion

    #region === MOVEMENT ===

    private void MoveToPlayer()
    {
        float dir = Mathf.Sign(player.position.x - transform.position.x);

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