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

    private void OnTriggerStay2D(Collider2D other)
    {
        PlayerAttackHitbox hitbox = other.GetComponent<PlayerAttackHitbox>();
        if (hitbox == null) return;

        // Используем TryHit из хитбокса, чтобы срабатывало для воздушной атаки
        // (через overlap внутри хитбокса уже работает, но на всякий случай)
    }
}