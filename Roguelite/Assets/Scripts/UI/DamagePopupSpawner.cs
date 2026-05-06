using UnityEngine;

public class DamagePopupSpawner : MonoBehaviour
{
    public GameObject prefab;

    public void Spawn(int damage, Transform target)
    {
        Vector3 offset = new Vector3(
            Random.Range(-0.3f, 0.3f),
            Random.Range(0.5f, 1f),
            0
        );

        Vector3 pos = target.position + offset;

        GameObject obj = Instantiate(prefab, pos, Quaternion.identity);

        obj.GetComponent<DamageText>().SetDamage(damage);
    }
}