using UnityEngine;

public class PlayerStamina : MonoBehaviour
{
    [Header("Stamina")]
    public float maxStamina = 100f;
    public float currentStamina;

    [Header("Regen")]
    public float regenRate = 15f;
    public float regenDelay = 1f;

    private float lastUseTime;

    public bool IsEmpty => currentStamina <= 0;

    public System.Action OnStaminaChanged;

    private void Awake()
    {
        currentStamina = maxStamina;
    }

    private void Update()
    {
        if (Time.time - lastUseTime > regenDelay)
        {
            Restore(regenRate * Time.deltaTime);
        }
    }

    public bool CanUse(float amount)
    {
        return currentStamina >= amount;
    }

    public void Use(float amount)
    {
        currentStamina -= amount;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);

        lastUseTime = Time.time;
        OnStaminaChanged?.Invoke();
    }

    public void Restore(float amount)
    {
        currentStamina += amount;
        currentStamina = Mathf.Clamp(currentStamina, 0, maxStamina);

        OnStaminaChanged?.Invoke();
    }
}