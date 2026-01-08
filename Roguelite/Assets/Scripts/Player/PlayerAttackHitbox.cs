using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerAttackHitbox : MonoBehaviour
{
    private Collider2D hitbox;
    private PlayerController player;
    private HashSet<Collider2D> hitEnemies = new HashSet<Collider2D>();

    public float activeTime = 0.2f;

    private void Awake()
    {
        hitbox = GetComponent<Collider2D>();
        hitbox.enabled = false;
        player = GetComponentInParent<PlayerController>();
    }

    public void ActivateHitbox()
    {
        StopAllCoroutines(); // чтобы не было конфликтов при повторном активации
        hitEnemies.Clear();  // каждый новый удар может поразить врагов заново
        StartCoroutine(HitboxRoutine());
    }

    private IEnumerator HitboxRoutine()
    {
        hitbox.enabled = true;
        yield return new WaitForSeconds(activeTime);
        hitbox.enabled = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!hitbox.enabled) return;
        if (hitEnemies.Contains(collision)) return; // уже ударили этого врага

        EnemyController enemy = collision.GetComponent<EnemyController>();
        if (enemy != null)
        {
            Vector2 hitDir = (enemy.transform.position - player.transform.position).normalized;
            enemy.TakeDamage(1);
            hitEnemies.Add(collision);
        }
    }
}