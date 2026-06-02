using UnityEngine;

public class GhostEnemy : EnemyController
{
    private void Die()
    {
        Destroy(gameObject);
    }
}
