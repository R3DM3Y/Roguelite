using System.Collections;
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
    public LayerMask groundLayer;
    public Transform groundCheck;
    public float groundCheckRadius = 0.1f;

    [Header("DropDown")]
    public float dropDownTime = 0.2f;
    
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
        playerCollider = GetComponent<Collider2D>();
        currentHealth = maxHealth;
    }
    
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            // Можно добавить эффект удара или анимацию
            Debug.Log($"Игрок получил {damage} урона, осталось {currentHealth} HP");
        }
    }

    private void Die()
    {
        Debug.Log("Игрок погиб");
        // Здесь можно проиграть анимацию смерти или перезапустить сцену
        Destroy(gameObject);
    }

    public void Move(float x)
    {
        rb.linearVelocity = new Vector2(x * moveSpeed, rb.linearVelocity.y);
    }

    public void Jump()
    {
        if (IsGrounded() && rb.linearVelocity.y <= 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    public bool IsGrounded()
    {
        return Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
    }

    public void DropDown()
    {
        // Если уже спускаемся или не на платформе — ничего не делаем
        if (isDropping || !IsStandingOnOneWayPlatform())
            return;

        StartCoroutine(DisableColliderTemporarily());
    }

    private bool IsStandingOnOneWayPlatform()
    {
        // Только проверяем объекты под игроком
        Collider2D[] hits = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius, groundLayer);
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

        // Ждём один FixedUpdate для безопасного проваливания через платформу
        yield return new WaitForFixedUpdate();

        // Временно выключаем коллайдер на заданное время
        yield return new WaitForSeconds(dropDownTime);
        playerCollider.enabled = true;
        isDropping = false;
    }
}
