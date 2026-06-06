using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class BossSpikeAttack : MonoBehaviour
{
    [Header("Spike")]
    public GameObject spikePrefab;

    [Header("Attack")]
    public float stepDistance = 2f;
    public int spikeCount = 15;
    public float delayBetweenSpikes = 0.1f;
    
    private bool isLaunching;
    
    public void Launch(Vector2 startPos)
    {
        if (isLaunching) return;
        StartCoroutine(LaunchRoutine(startPos));
    }
    
    private IEnumerator LaunchRoutine(Vector2 startPos)
    {
        isLaunching = true;
    
        StartCoroutine(SpawnSpikes(startPos, 1));
        StartCoroutine(SpawnSpikes(startPos, -1));
    
        yield return new WaitForSeconds(spikeCount * delayBetweenSpikes + 0.2f);
    
        isLaunching = false;
    }

    private IEnumerator SpawnSpikes(Vector2 startPos, int direction)
    {
        for (int i = 0; i < spikeCount; i++)
        {
            Vector2 pos = startPos + Vector2.right * (direction * stepDistance * i);

            GameObject spike = Instantiate(spikePrefab, pos, Quaternion.identity);

            Vector3 scale = spike.transform.localScale;

            scale.x = Mathf.Abs(scale.x) * direction;

            spike.transform.localScale = scale;

            yield return new WaitForSeconds(delayBetweenSpikes);
        }
    }
}