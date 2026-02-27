using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    public int damage = 1;

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();
        if (player != null)
        {
            Vector2 dir = (player.transform.position - transform.position).normalized;
            player.TakeDamage(damage, dir);
        }
    }
}
