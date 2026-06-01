using TMPro;
using UnityEngine;

public class DamageText : MonoBehaviour
{
    public TextMeshPro text;
    public float lifeTime = 1f;

    private float timer;

    void Start()
    {
        transform.localScale = Vector3.one;
        timer = lifeTime;
    }

    public void SetDamage(int dmg)
    {
        text.text = dmg.ToString();
    }

    void Update()
    {
        timer -= Time.deltaTime;

        float alpha = Mathf.Clamp01(timer / lifeTime);
        text.alpha = alpha;

        if (timer <= 0f)
        {
            Destroy(gameObject);
        }
    }
}