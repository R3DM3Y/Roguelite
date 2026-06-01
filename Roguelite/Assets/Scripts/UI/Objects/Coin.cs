using UnityEngine;

public class Coin : MonoBehaviour
{
    public int value = 10;

    [SerializeField]
    private float launchForce = 5f;

    private Rigidbody2D rb;

    private bool picked;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        Vector2 force =
            new Vector2(
                Random.Range(-1f, 1f),
                1f
            ).normalized;

        rb.AddForce(force * launchForce,
            ForceMode2D.Impulse);
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (picked) return;

        if (!other.collider.CompareTag("Player"))
            return;

        picked = true;

        CoinManager.Instance.AddCoins(value);

        Destroy(gameObject);
    }
}