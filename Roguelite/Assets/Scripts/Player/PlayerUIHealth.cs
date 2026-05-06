using UnityEngine;
using UnityEngine.UI;

public class PlayerUIHealth : MonoBehaviour
{
    public Image healthImage;
    public PlayerController player;

    private void Start()
    {
        UpdateHealth();
    }

    public void UpdateHealth()
    {
        healthImage.fillAmount = (float)player.CurrentHealth / player.MaxHealth;
    }
    
    private void OnEnable()
    {
        player.OnHealthChanged += UpdateHealth;
    }
}