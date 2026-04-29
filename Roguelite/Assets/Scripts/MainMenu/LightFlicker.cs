using UnityEngine;

public class LightFlicker : MonoBehaviour
{
    private SpriteRenderer sr;
    private Color baseColor;

    public float speed = 2f;
    public float minAlpha = 0.4f;
    public float maxAlpha = 0.8f;

    private float offset;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        baseColor = sr.color;

        offset = Random.Range(0f, 100f);
    }

    void Update()
    {
        float a = Mathf.Lerp(
            minAlpha,
            maxAlpha,
            (Mathf.Sin((Time.time + offset) * speed) + 1f) / 2f
        );

        sr.color = new Color(baseColor.r, baseColor.g, baseColor.b, a);
    }
}