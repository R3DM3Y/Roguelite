using UnityEngine;
using UnityEngine.InputSystem;

public class Chest : MonoBehaviour
{
    public GameObject scrollPrefab;

    private bool playerInside;
    private bool opened;

    private Animator animator;
    
    [Header("Save")]
    public string chestID;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }
    
    private void Start()
    {
        if (RoomManager.Instance.IsChestOpened(chestID))
        {
            opened = true;

            if (animator != null)
                animator.Play("Opened");

            return;
        }
    }

    private void Update()
    {
        if (opened)
            return;

        if (!playerInside)
            return;

        if (Keyboard.current.wKey.wasPressedThisFrame)
        {
            OpenChest();
        }
    }

    private void OpenChest()
    {
        opened = true;
        
        RoomManager.Instance.MarkChestOpened(chestID);

        RoomManager.Instance.SaveGame();

        if (animator != null)
        {
            animator.SetTrigger("Open");
        }

        SpawnScroll();
    }

    private void SpawnScroll()
    {
        GameObject scroll =
            Instantiate(
                scrollPrefab,
                transform.position + Vector3.up,
                Quaternion.identity
            );

        Rigidbody2D rb =
            scroll.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            Vector2 force =
                new Vector2(
                    Random.Range(-1f, 1f),
                    2f
                );

            rb.AddForce(force, ForceMode2D.Impulse);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = false;
    }
    
    private void OnValidate()
    {
        if (string.IsNullOrEmpty(chestID))
            chestID = System.Guid.NewGuid().ToString();
    }
}