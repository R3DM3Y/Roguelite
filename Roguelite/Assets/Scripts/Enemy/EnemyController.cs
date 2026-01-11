using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Health")]
    public int maxHealth = 3;

    public int currentHealth;

    [Header("Movement")]
    public float moveSpeed = 2f;
    public Transform[] patrolPoints;
    private int currentPointIndex = 0;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundCheckDistance = 0.2f;
    public LayerMask groundLayer;

    [HideInInspector] public Rigidbody2D rb;
    private bool facingRight = true;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        currentHealth = maxHealth;
    }

    private void FixedUpdate()
    {
        Patrol();
    }

    private void Patrol()
    {
        if (patrolPoints.Length == 0) return;

        Transform targetPoint = patrolPoints[currentPointIndex];
        float direction = Mathf.Sign(targetPoint.position.x - transform.position.x);

        if (!IsGroundAhead(direction))
        {
            Flip();
            return;
        }

        rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);

        if (Mathf.Abs(transform.position.x - targetPoint.position.x) < 0.1f)
        {
            currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
        }

        if ((direction > 0 && !facingRight) || (direction < 0 && facingRight))
        {
            Flip();
        }
    }

    private bool IsGroundAhead(float direction)
    {
        Vector2 origin = groundCheck.position + Vector3.right * direction * 0.1f;
        RaycastHit2D hit = Physics2D.Raycast(origin, Vector2.down, groundCheckDistance, groundLayer);
        Debug.DrawRay(origin, Vector2.down * groundCheckDistance, Color.red);
        return hit.collider != null;
    }

    private void Flip()
    {
        facingRight = !facingRight;
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (currentHealth <= 0)
            Die();
    }

    private void Die()
    {
        Destroy(gameObject);
    }
}
