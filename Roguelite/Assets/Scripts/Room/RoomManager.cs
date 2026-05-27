using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance;

    public RoomController startRoom;
    public RoomController[] possibleRooms;
    public Transform player;

    private CameraFollow2D cameraFollow;
    private RoomNode currentNode;

    private Dictionary<Vector2Int, RoomNode> generated =
        new Dictionary<Vector2Int, RoomNode>();

    [SerializeField] private float difficultyMultiplier = 1f;

    public float DifficultyMultiplier => difficultyMultiplier;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        cameraFollow = Camera.main.GetComponent<CameraFollow2D>();

        foreach (var r in possibleRooms)
            r.gameObject.SetActive(false);

        startRoom.gameObject.SetActive(true);

        RoomNode startNode = new RoomNode
        {
            uniqueID = System.Guid.NewGuid().ToString(),
            prefab = startRoom,
            gridPosition = Vector2Int.zero
        };

        generated.Add(startNode.gridPosition, startNode);
        currentNode = startNode;

        // Получаем размеры АКТИВНОЙ стартовой комнаты
        Vector2 startRoomSize = GetRoomDimensions(startRoom);
        Debug.Log($"Start room size: {startRoomSize}");
        
        MinimapManager.Instance.CreateRoom(startNode.gridPosition, startRoomSize);
        MinimapManager.Instance.SetCurrent(startNode.gridPosition);

        cameraFollow.SetBounds(startRoom.cameraBounds);
        cameraFollow.InstantSnap();
    }

    public void ChangeRoom(ExitDirection dir)
    {
        Vector2Int target = currentNode.gridPosition + GetOffset(dir);
        Debug.Log($"Moving from {currentNode.gridPosition} to {target}");
        
        RoomNode nextNode;

        if (generated.ContainsKey(target))
        {
            nextNode = generated[target];
            Debug.Log($"Room {target} already exists, reusing");
        }
        else
        {
            RoomController prefab = GetRandomRoom(dir);
            if (prefab == null)
            {
                Debug.LogError($"No valid room for direction {dir}");
                return;
            }
            
            // ВАЖНО: временно активируем, чтобы получить размеры
            Vector2 roomDimensions = GetRoomDimensions(prefab);
            
            Debug.Log($"Creating new room at {target} with size {roomDimensions}");

            nextNode = new RoomNode
            {
                uniqueID = System.Guid.NewGuid().ToString(),
                prefab = prefab,
                gridPosition = target
            };

            generated.Add(target, nextNode);
            MinimapManager.Instance.CreateRoom(target, roomDimensions);
        }

        // Рисуем соединение
        MinimapManager.Instance.DrawConnection(currentNode.gridPosition, target);

        currentNode.prefab.gameObject.SetActive(false);

        nextNode.prefab.gameObject.SetActive(true);
        nextNode.prefab.ResetRoom();

        currentNode = nextNode;

        MinimapManager.Instance.SetCurrent(currentNode.gridPosition);

        Transform spawn = GetSpawnPoint(currentNode.prefab, dir);
        player.position = spawn.position;

        cameraFollow.SetBounds(currentNode.prefab.cameraBounds);
        cameraFollow.InstantSnap();
    }
    
    private Vector2 GetRoomDimensions(RoomController room)
    {
        // Пробуем получить размер из cameraBounds
        if (room.cameraBounds != null)
        {
            // size даёт размеры коллайдера независимо от активности объекта
            Vector2 size = room.cameraBounds.size;
        
            if (size.x > 0.01f && size.y > 0.01f)
            {
                Debug.Log($"Room {room.name} size from cameraBounds: {size}");
                return size;
            }
        }
    
        // Запасной вариант — фиксированный размер
        Debug.LogWarning($"Room {room.name}: using default size 20x15");
        return new Vector2(20f, 15f);
    }

    private Vector2Int GetOffset(ExitDirection dir)
    {
        return dir switch
        {
            ExitDirection.Left => Vector2Int.left,
            ExitDirection.Right => Vector2Int.right,
            ExitDirection.Up => Vector2Int.up,
            ExitDirection.Down => Vector2Int.down,
            _ => Vector2Int.zero
        };
    }

    private Transform GetSpawnPoint(RoomController room, ExitDirection dir)
    {
        return dir switch
        {
            ExitDirection.Left => room.spawnRight,
            ExitDirection.Right => room.spawnLeft,
            ExitDirection.Up => room.spawnDown,
            ExitDirection.Down => room.spawnUp,
            _ => room.transform
        };
    }

    private RoomController GetRandomRoom(ExitDirection dir)
    {
        List<RoomController> valid = new();

        foreach (var r in possibleRooms)
        {
            if (dir == ExitDirection.Left && r.hasRight) valid.Add(r);
            if (dir == ExitDirection.Right && r.hasLeft) valid.Add(r);
            if (dir == ExitDirection.Up && r.hasDown) valid.Add(r);
            if (dir == ExitDirection.Down && r.hasUp) valid.Add(r);
        }

        if (valid.Count == 0)
        {
            Debug.LogError($"No valid rooms for direction: {dir}");
            return null;
        }

        return valid[Random.Range(0, valid.Count)];
    }

    public bool IsRoomGenerated(Vector2Int pos)
    {
        return generated.ContainsKey(pos);
    }
    
    private HashSet<string> killedEnemies = new HashSet<string>();

    public void MarkEnemyKilled(string enemyID)
    {
        killedEnemies.Add(enemyID);
    }

    public bool IsEnemyKilled(string enemyID)
    {
        return killedEnemies.Contains(enemyID);
    }
}