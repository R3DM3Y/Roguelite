using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerAttackHitbox : MonoBehaviour
{
    private Collider2D hitbox;
    public PlayerController player;

    // Чтобы не бить одного врага несколько раз за одну атаку
    private HashSet<Collider2D> hitEnemies = new HashSet<Collider2D>();

    public float activeTime = 0.2f;

    // Флаг для воздушной атаки — чтобы срабатывала один раз
    private bool airDownHitDone;

    public int Damage =>
        player.IsAttackingDown
            ? player.AirDownAttackDamage
            : player.NormalAttackDamage;

    private void Awake()
    {
        hitbox = GetComponent<Collider2D>();
        hitbox.enabled = false;
        player = GetComponentInParent<PlayerController>();
    }

    public void ActivateHitbox()
    {
        StopAllCoroutines();
        hitEnemies.Clear();
        airDownHitDone = false; // сбрасываем флаг для воздушной атаки
        StartCoroutine(HitboxRoutine());
    }

    private IEnumerator HitboxRoutine()
    {
        hitbox.enabled = true;
        float timer = 0f;

        while (timer < activeTime)
        {
            // Каждый кадр проверяем всех врагов, которые внутри хитбокса
            Collider2D[] overlaps = Physics2D.OverlapBoxAll(hitbox.bounds.center, hitbox.bounds.size, 0f);
            foreach (var col in overlaps)
            {
                TryHit(col);
            }

            timer += Time.fixedDeltaTime; // фиксированный шаг для стабильности
            yield return new WaitForFixedUpdate();
        }

        hitbox.enabled = false;
    }

    private void TryHit(Collider2D collision)
    {
        if (hitEnemies.Contains(collision)) return;

        EnemyController enemy = collision.GetComponent<EnemyController>();
        if (enemy == null) return;

        if (player.IsAttackingDown && !airDownHitDone)
        {
            enemy.TakeDamage(player.AirDownAttackDamage);
            player.OnAirDownHitSuccess();
            airDownHitDone = true; // чтобы воздушная атака срабатывала только один раз
        }
        else if (!player.IsAttackingDown)
        {
            enemy.TakeDamage(player.NormalAttackDamage);
        }

        hitEnemies.Add(collision);
    }

    public void DisableImmediately()
    {
        StopAllCoroutines();
        hitbox.enabled = false;
    }
}