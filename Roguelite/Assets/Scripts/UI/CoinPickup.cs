using UnityEngine;

public class CoinPickup : MonoBehaviour
{
    private Coin coin;

    private void Awake()
    {
        coin = GetComponentInParent<Coin>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        CoinManager.Instance.AddCoins(coin.value);

        Destroy(coin.gameObject);
    }
}