using UnityEngine;

public class EnemyHitbox : MonoBehaviour
{
    [Header("Damage Settings")]
    public int damage = 1; // Урон игроку

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Проверяем, столкнулся ли игрок
        PlayerController player = collision.GetComponent<PlayerController>();
        if (player != null)
        {
            // Можно здесь вызвать метод у игрока, например TakeDamage
            player.TakeDamage(damage); 
        }

        // Проверяем, если урон должен получать сам враг
        EnemyController enemy = collision.GetComponent<EnemyController>();
        if (enemy != null)
        {
            // Передаем урон целым числом
            enemy.TakeDamage(damage);
        }
    }
}