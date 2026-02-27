using UnityEngine;

public class EnemyAttackHitbox : MonoBehaviour
{
    public int damage = 1;
    private Collider2D hitbox;

    private void Awake()
    {
        hitbox = GetComponent<Collider2D>();
        hitbox.enabled = false;
    }

    public void Activate()
    {
        hitbox.enabled = true;
        Debug.Log("HITBOX ENABLED: " + hitbox.enabled);
    }

    public void Deactivate()
    {
        hitbox.enabled = false;
        Debug.Log("HITBOX DISABLED");
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerController player =
            other.GetComponentInParent<PlayerController>();

        if (player == null || player.IsDead) return;

        Vector2 dir =
            (player.transform.position - transform.position).normalized;

        player.TakeDamage(damage, dir);
    }
}