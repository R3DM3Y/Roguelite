using UnityEngine;

public class RoomController : MonoBehaviour
{
    [Header("ID")]
    public string roomID;
    
    [Header("Available Exits")]
    public bool hasLeft;
    public bool hasRight;
    public bool hasUp;
    public bool hasDown;

    [Header("Spawn Points")]
    public Transform spawnLeft;
    public Transform spawnRight;
    public Transform spawnUp;
    public Transform spawnDown;

    [Header("Camera Bounds")]
    public BoxCollider2D cameraBounds;
    
    private EnemyController[] enemies;
    
    private void Awake()
    {
        enemies =
            GetComponentsInChildren<EnemyController>(true);
    }
    
    public void ResetRoom()
    {
        foreach (var enemy in enemies)
        {
            if (enemy == null)
                continue;

            if (RoomManager.Instance.IsEnemyKilled(enemy.enemyID))
            {
                enemy.gameObject.SetActive(false);
            }
            else
            {
                enemy.ResetEnemy();
            }
        }
    }
}