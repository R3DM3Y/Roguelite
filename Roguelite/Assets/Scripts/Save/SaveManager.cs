using UnityEngine;

public static class SaveManager
{
    private const string COINS_KEY = "MetaCoins";
    private const string UPGRADE_LEVEL_KEY = "MetaUpgrade_";
    
    // Монеты
    public static int GetCoins()
    {
        return PlayerPrefs.GetInt(COINS_KEY, 0);
    }
    
    public static void SaveCoins(int amount)
    {
        PlayerPrefs.SetInt(COINS_KEY, amount);
        PlayerPrefs.Save();
        Debug.Log($"[SaveManager] SaveCoins: {amount}");
    }
    
    public static bool SpendCoins(int amount)
    {
        int current = GetCoins();
        if (current < amount) return false;
        PlayerPrefs.SetInt(COINS_KEY, current - amount);
        PlayerPrefs.Save();
        return true;
    }
    
    // Мета-улучшения (для будущего)
    public static int GetUpgradeLevel(string upgradeId)
    {
        return PlayerPrefs.GetInt(UPGRADE_LEVEL_KEY + upgradeId, 0);
    }
    
    public static void SetUpgradeLevel(string upgradeId, int level)
    {
        PlayerPrefs.SetInt(UPGRADE_LEVEL_KEY + upgradeId, level);
        PlayerPrefs.Save();
    }
}