using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    [Header("Damage Settings")]
    public int damage = 1;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        TryHit(collision);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        TryHit(collision);
    }

    private void TryHit(Collider2D collision)
    {
        PlayerController player = collision.GetComponent<PlayerController>();
        if (player != null)
        {
            Vector2 hitDirection = (player.transform.position - transform.position).normalized;
            player.TakeDamage(damage, hitDirection);

        }
    }

}