using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerAttackHitbox : MonoBehaviour
{
    private Collider2D hitbox;
    public PlayerController player;
    private HashSet<Collider2D> hitEnemies = new HashSet<Collider2D>();

    public float activeTime = 0.2f;
    
    public int Damage =>
        player.IsAttackingDown
            ? player.airDownAttackDamage
            : player.normalAttackDamage;

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
        if (hitEnemies.Contains(collision)) return;

        EnemyController enemy = collision.GetComponent<EnemyController>();
        if (enemy == null) return;

        if (player.IsAttackingDown)
        {
            enemy.TakeDamage(player.airDownAttackDamage);
            player.OnAirDownHitSuccess();
        }
        else
        {
            enemy.TakeDamage(player.normalAttackDamage);
        }

        hitEnemies.Add(collision);
    }
    
    public void DisableImmediately()
    {
        StopAllCoroutines();
        hitbox.enabled = false;
    }
}