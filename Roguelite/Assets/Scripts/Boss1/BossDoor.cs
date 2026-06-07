using UnityEngine;
using UnityEngine.InputSystem;

public class BossDoor : MonoBehaviour
{
    [SerializeField] private RoomController bossRoomPrefab;
    [SerializeField] private Transform bossSpawnPoint;
    private bool playerNearby;
    private bool used;

    private void Update()
    {
        if (used || !playerNearby) return;
        if (Keyboard.current.wKey.wasPressedThisFrame) EnterBossRoom();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerNearby = true;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player")) playerNearby = false;
    }

    private void EnterBossRoom()
    {
        used = true;

        // Удаляем все активные комнаты
        var allRooms = FindObjectsByType<RoomController>(FindObjectsSortMode.None);
        foreach (var r in allRooms) Destroy(r.gameObject);

        // Создаём комнату босса
        var bossRoom = Instantiate(bossRoomPrefab);
        bossRoom.ResetRoom();

        // Ищем спавн-поинт внутри созданной комнаты (по имени)
        Transform spawnPoint = bossRoom.transform.Find("SpawnPoint"); // или bossRoom.spawnDown
        if (spawnPoint == null)
            spawnPoint = bossRoom.spawnDown; // fallback на любой спавн
    
        RoomManager.Instance.player.position = spawnPoint.position;
        Camera.main.GetComponent<CameraFollow2D>().SetBounds(bossRoom.cameraBounds);
        Camera.main.GetComponent<CameraFollow2D>().InstantSnap();
    }
}