using UnityEngine;

public class ScrollPickup : MonoBehaviour
{
    private bool canPickup;
    private bool picked;

    [SerializeField] private Collider2D pickupCollider;

    private void Start()
    {
        // ❗ отключаем pickup сразу
        pickupCollider.enabled = false;

        Invoke(nameof(EnablePickup), 0.7f);
    }

    private void EnablePickup()
    {
        canPickup = true;
        pickupCollider.enabled = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!canPickup || picked) return;
        if (!other.CompareTag("Player")) return;

        picked = true;

        UpgradeManager.Instance.ApplyRandomUpgrade();
        Destroy(transform.root.gameObject);
    }
}