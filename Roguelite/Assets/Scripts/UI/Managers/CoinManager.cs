using UnityEngine;
using TMPro;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance;

    public int coins;
    [SerializeField]
    private TextMeshProUGUI coinsText;
    public int Coins => coins;

    public void SetCoins(int amount)
    {
        coins = amount;
        UpdateUI();
    }

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }
    
        Instance = this;
    
        // Загружаем мета-монеты один раз при старте сцены
        coins = SaveManager.GetCoins();
        Debug.Log($"[CoinManager] Awake - loaded coins: {coins}");
    }
    
    private void Start()
    {
        UpdateUI();
    }

    public void AddCoins(int amount)
    {
        coins += amount;
        Debug.Log("Coins now = " + coins);
        UpdateUI();
    }
    
    private void UpdateUI()
    {
        coinsText.text = coins.ToString();
    }
}