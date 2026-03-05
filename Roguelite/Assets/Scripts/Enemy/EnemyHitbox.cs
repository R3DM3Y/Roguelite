using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    private EnemyController enemy;

    private void Awake()
    {
        enemy = GetComponentInParent<EnemyController>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player = other.GetComponent<PlayerController>();

        if (player != null && enemy != null)
        {
            Vector2 dir = (player.transform.position - transform.position).normalized;

            player.TakeDamage(enemy.Stats.contactDamage, dir);
        }
    }
}