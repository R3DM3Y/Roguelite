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
        healthImage.fillAmount = (float)player.CurrentHealth / player.MaxHealth;

        hpText.text = player.CurrentHealth.ToString();
    }
    
    private void OnEnable()
    {
        player.OnHealthChanged += UpdateHealth;
    }
}