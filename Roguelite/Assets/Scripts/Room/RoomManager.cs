using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance;

    [Header("Rooms")]
    public RoomController startRoom;

    public RoomController[] possibleRooms;

    [Header("Player")]
    public Transform player;
    
    [Header("Difficulty")]
    public int roomsPassed;

    public float difficultyMultiplier = 1f;

    private RoomController currentRoom;
    
    private CameraFollow2D cameraFollow;

    private HashSet<string> killedEnemies =
        new HashSet<string>();
    
    private HashSet<string> visitedRooms =
        new HashSet<string>();

    // связь комнат
    private Dictionary<string, RoomController> roomConnections =
        new Dictionary<string, RoomController>();

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        cameraFollow =
            Camera.main.GetComponent<CameraFollow2D>();

        // выключаем все комнаты
        foreach (var room in possibleRooms)
        {
            room.gameObject.SetActive(false);
        }

        // включаем стартовую
        startRoom.gameObject.SetActive(true);

        currentRoom = startRoom;

        // ВАЖНО
        cameraFollow.SetBounds(
            currentRoom.cameraBounds
        );

        cameraFollow.InstantSnap();
    }

    public void ChangeRoom(ExitDirection direction)
    {
        string key =
            currentRoom.roomID + "_" + direction;

        RoomController nextRoom;

        // если связь уже есть
        if (roomConnections.ContainsKey(key))
        {
            nextRoom = roomConnections[key];
        }
        else
        {
            nextRoom = GetRandomRoom(direction);

            roomConnections.Add(key, nextRoom);

            // СОЗДАЕМ ОБРАТНУЮ СВЯЗЬ
            ExitDirection opposite =
                GetOppositeDirection(direction);

            string backKey =
                nextRoom.roomID + "_" + opposite;

            if (!roomConnections.ContainsKey(backKey))
            {
                roomConnections.Add(backKey, currentRoom);
            }
        }

        currentRoom.gameObject.SetActive(false);

        nextRoom.gameObject.SetActive(true);
        
        nextRoom.ResetRoom();

        currentRoom = nextRoom;
        
        if (!visitedRooms.Contains(nextRoom.roomID))
        {
            visitedRooms.Add(nextRoom.roomID);

            roomsPassed++;

            difficultyMultiplier =
                1f + roomsPassed * 0.05f;
        }

        difficultyMultiplier =
            1f + roomsPassed * 0.05f;
        
        Transform spawnPoint = GetSpawnPoint(
            nextRoom,
            direction
        );

        player.position = spawnPoint.position;

        cameraFollow.SetBounds(
            nextRoom.cameraBounds
        );

        cameraFollow.InstantSnap();

        // отключаем триггер на секунду
        RoomExit exit =
            spawnPoint.GetComponentInChildren<RoomExit>();

        if (exit != null)
        {
            exit.DisableTemporarily();
        }
    }

    private Transform GetSpawnPoint(
        RoomController room,
        ExitDirection enterDirection)
    {
        Transform point = null;

        switch (enterDirection)
        {
            case ExitDirection.Left:
                point = room.spawnRight;
                break;

            case ExitDirection.Right:
                point = room.spawnLeft;
                break;

            case ExitDirection.Up:
                point = room.spawnDown;
                break;

            case ExitDirection.Down:
                point = room.spawnUp;
                break;
        }

        // ЗАЩИТА ОТ NULL
        if (point == null)
        {
            Debug.LogError(
                "В комнате " + room.roomID +
                " нет нужного spawn point!"
            );

            return room.transform;
        }

        return point;
    }

    private ExitDirection GetOppositeDirection(
        ExitDirection dir)
    {
        switch (dir)
        {
            case ExitDirection.Left:
                return ExitDirection.Right;

            case ExitDirection.Right:
                return ExitDirection.Left;

            case ExitDirection.Up:
                return ExitDirection.Down;

            case ExitDirection.Down:
                return ExitDirection.Up;
        }

        return ExitDirection.Left;
    }

    public void MarkEnemyKilled(string enemyID)
    {
        killedEnemies.Add(enemyID);
    }

    public bool IsEnemyKilled(string enemyID)
    {
        return killedEnemies.Contains(enemyID);
    }
    
    private RoomController GetRandomRoom(
        ExitDirection enterDirection)
    {
        List<RoomController> validRooms =
            new List<RoomController>();

        foreach (var room in possibleRooms)
        {
            bool valid = false;

            switch (enterDirection)
            {
                case ExitDirection.Left:
                    valid = room.hasRight;
                    break;

                case ExitDirection.Right:
                    valid = room.hasLeft;
                    break;

                case ExitDirection.Up:
                    valid = room.hasDown;
                    break;

                case ExitDirection.Down:
                    valid = room.hasUp;
                    break;
            }

            if (valid)
            {
                validRooms.Add(room);
            }
        }

        return validRooms[
            Random.Range(0, validRooms.Count)
        ];
    }
}