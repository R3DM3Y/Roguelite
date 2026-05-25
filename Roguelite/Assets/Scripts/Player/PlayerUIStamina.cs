using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIStamina : MonoBehaviour
{
    public TextMeshProUGUI StaminaText;
    public Image staminaImage;
    public PlayerStamina stamina;

    private void Start()
    {
        UpdateStamina();
    }

    public void UpdateStamina()
    {
        StaminaText.text = ((int)stamina.currentStamina).ToString();

        staminaImage.fillAmount = (float)stamina.currentStamina / stamina.maxStamina;
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