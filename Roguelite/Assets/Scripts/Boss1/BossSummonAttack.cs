using System.Collections.Generic;
using UnityEngine;

public class BossSummonAttack : MonoBehaviour
{
    [Header("Ground Enemies")]
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform[] spawnPoints;
    
    [Header("Flying Enemies")]
    [SerializeField] private GameObject flyingEnemyPrefab;
    [SerializeField] private int flyingEnemiesCount = 1;
    
    [Header("Limits")]
    [SerializeField] private int maxGroundEnemies = 6;
    [SerializeField] private int maxFlyingEnemies = 1;
    
    private List<GameObject> summonedGround = new List<GameObject>();
    private List<GameObject> summonedFlying = new List<GameObject>();

    public void SummonEnemies()
    {
        summonedGround.RemoveAll(e => e == null);
        
        if (summonedGround.Count >= maxGroundEnemies) return;
        
        int canSpawn = Mathf.Min(2, maxGroundEnemies - summonedGround.Count);
        
        List<Transform> available = new List<Transform>(spawnPoints);
        Shuffle(available);
        
        for (int i = 0; i < canSpawn && i < available.Count; i++)
        {
            GameObject enemy = Instantiate(enemyPrefab, available[i].position, Quaternion.identity);
            summonedGround.Add(enemy);
        }
    }
    
    public void SummonFlyingEnemies()
    {
        summonedFlying.RemoveAll(e => e == null);
        
        if (summonedFlying.Count >= maxFlyingEnemies) return;
        
        int canSpawn = Mathf.Min(flyingEnemiesCount, maxFlyingEnemies - summonedFlying.Count);
        
        for (int i = 0; i < canSpawn; i++)
        {
            Vector2 pos = (Vector2)transform.position + Random.insideUnitCircle * 3f;
            GameObject enemy = Instantiate(flyingEnemyPrefab, pos, Quaternion.identity);
            summonedFlying.Add(enemy);
        }
    }
    
    private void Shuffle(List<Transform> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (list[i], list[j]) = (list[j], list[i]);
        }
    }
}