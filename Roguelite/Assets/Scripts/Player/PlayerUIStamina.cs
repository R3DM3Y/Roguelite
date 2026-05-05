using UnityEngine;
using UnityEngine.UI;

public class PlayerUIStamina : MonoBehaviour
{
    public Image staminaImage;
    public PlayerStamina stamina;

    private void Start()
    {
        UpdateStamina();
    }

    public void UpdateStamina()
    {
        staminaImage.fillAmount = stamina.currentStamina / stamina.maxStamina;
    }

    private void OnEnable()
    {
        stamina.OnStaminaChanged += UpdateStamina;
    }

    private void OnDisable()
    {
        stamina.OnStaminaChanged -= UpdateStamina;
    }
}