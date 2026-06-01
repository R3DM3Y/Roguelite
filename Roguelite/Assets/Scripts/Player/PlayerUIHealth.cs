using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerUIHealth : MonoBehaviour
{
    public TextMeshProUGUI hpText;
    public Image healthImage;
    public PlayerController player;

    private void Start()
    {
        UpdateHealth();
    }

    public void UpdateHealth()
    {
        float fill = (float)player.CurrentHealth / player.MaxHealth;
        healthImage.fillAmount = Mathf.Clamp01(fill);

        int displayHP = Mathf.Max(0, player.CurrentHealth);
        hpText.text = displayHP.ToString();
    }
    
    private void OnEnable()
    {
        player.OnHealthChanged += UpdateHealth;
    }
}