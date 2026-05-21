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

    private RoomController currentRoom;
    
    private CameraFollow2D cameraFollow;

    private HashSet<string> killedEnemies =
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
            nextRoom =
                possibleRooms[
                    Random.Range(0, possibleRooms.Length)
                ];

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
        switch (enterDirection)
        {
            case ExitDirection.Left:
                return room.spawnRight;

            case ExitDirection.Right:
                return room.spawnLeft;

            case ExitDirection.Up:
                return room.spawnDown;

            case ExitDirection.Down:
                return room.spawnUp;
        }

        return room.spawnLeft;
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
}