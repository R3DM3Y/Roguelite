using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour, IControllable
{
    private bool dropUsed = false;
    
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 7f;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.1f;

    private Rigidbody2D rb;
    private float moveInput;
    private Collider2D playerCollider;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(moveInput * moveSpeed, rb.linearVelocity.y);
    }

    public void Move(Vector3 direction)
    {
        moveInput = direction.x;

        bool pressingDown = direction.y < 0;

        if (pressingDown)
        {
            bool standingStill = Mathf.Abs(rb.linearVelocity.y) < 0.01f;

            if (standingStill && IsOnOneWayPlatform() && !dropUsed)
            {
                dropUsed = true;
                DropDown();
            }
        }
        else
        {
            dropUsed = false;
        }
    }



    public void Jump()
    {
        bool grounded = IsGrounded();
        bool stoppedVertically = Mathf.Abs(rb.linearVelocity.y) < 0.01f;

        if (grounded && stoppedVertically)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }



    private bool IsGrounded()
    {
        float extraHeight = 0.05f;
        RaycastHit2D hit = Physics2D.BoxCast(playerCollider.bounds.center, playerCollider.bounds.size, 0f,
            Vector2.down, extraHeight, groundLayer);
        return hit.collider != null;
    }

    private bool IsOnOneWayPlatform()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius);
        foreach (var hit in hits)
        {
            if (hit.CompareTag("OneWayPlatform"))
                return true;
        }
        return false;
    }


    private void DropDown()
    {
        StartCoroutine(DisableColliderTemporarily());
    }

    private IEnumerator DisableColliderTemporarily()
    {
        playerCollider.enabled = false;
        yield return new WaitForSeconds(0.2f); // Время прохождения платформы
        playerCollider.enabled = true;
    }
}