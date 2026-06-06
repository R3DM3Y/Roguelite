using UnityEngine;

public class BossBodyDamage : MonoBehaviour
{
    public BossController boss;

    public int damage = 1;

    private void OnCollisionStay2D(Collision2D collision)
    {
        PlayerController player =
            collision.collider.GetComponent<PlayerController>();

        if (player == null)
            return;

        Vector2 dir =
            (player.transform.position -
             boss.transform.position).normalized;

        player.TakeDamage(damage, dir);
    }
}