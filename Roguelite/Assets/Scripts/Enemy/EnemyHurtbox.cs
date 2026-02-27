using UnityEngine;

public class EnemyHurtbox : MonoBehaviour
{
    private EnemyController enemy;

    private void Awake()
    {
        enemy = GetComponentInParent<EnemyController>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerAttackHitbox hitbox = other.GetComponent<PlayerAttackHitbox>();
        if (hitbox == null) return;

        enemy.TakeDamage(hitbox.Damage);
    }
}