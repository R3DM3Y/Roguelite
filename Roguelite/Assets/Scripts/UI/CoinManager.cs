using UnityEngine;
using TMPro;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance;

    public int coins;
    [SerializeField]
    private TextMeshProUGUI coinsText;

    private void Awake()
    {
        Instance = this;
    }
    
    private void Start()
    {
        UpdateUI();
    }

    public void AddCoins(int amount)
    {
        coins += amount;

        UpdateUI();
    }
    
    private void UpdateUI()
    {
        coinsText.text = coins.ToString();
    }
}