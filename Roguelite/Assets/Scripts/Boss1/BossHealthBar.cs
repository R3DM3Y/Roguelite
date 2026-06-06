using UnityEngine;
using UnityEngine.UI;

public class BossHealthBar : MonoBehaviour
{
    public BossController boss;
    public Image fill;

    public float smoothSpeed = 5f;

    private float currentFill = 1f;

    private void Update()
    {
        float targetFill =
            (float)boss.CurrentHealth /
            boss.MaxHealth;

        currentFill = Mathf.Lerp(
            currentFill,
            targetFill,
            Time.deltaTime * smoothSpeed
        );

        fill.fillAmount = currentFill;

        fill.color =
            Color.Lerp(
                Color.red,
                Color.green,
                currentFill
            );
    }
}