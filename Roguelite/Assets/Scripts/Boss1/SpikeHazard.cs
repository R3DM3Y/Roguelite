using UnityEngine;

public class SpikeHazard : MonoBehaviour
{
    public int damage = 1;

    private bool dealtDamage;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (dealtDamage)
            return;

        PlayerController player = other.GetComponent<PlayerController>();

        if (player == null)
            return;

        dealtDamage = true;

        Vector2 dir = (player.transform.position - transform.position).normalized;

        player.TakeDamage(damage, dir);
    }

    public void DestroySpike()
    {
        Destroy(gameObject);
    }
}