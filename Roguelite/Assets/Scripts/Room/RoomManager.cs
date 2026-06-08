using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomManager : MonoBehaviour
{
    [Header("Rooms")]
    [SerializeField] private RoomController startRoomPrefab;
    [SerializeField] private RoomController[] possibleRoomPrefabs;
    [SerializeField] private RoomController preBossRoomPrefab;
    [SerializeField] private RoomController bossRoomPrefab;
    [SerializeField] private int minRoomsBeforeBoss = 7;
    [SerializeField] private int maxRooms = 12;
    
    private Vector2Int bossRoomPos = new Vector2Int(-999, -999);

    private bool bossRoomPlaced;
    private List<RoomController> activeRooms = new();
    
    public static RoomManager Instance;
    
    public PlayerController playerController;

    public RoomController startRoom;
    public RoomController[] possibleRooms;
    public Transform player;

    private CameraFollow2D cameraFollow;
    private RoomNode currentNode;
    private bool loadedFromSave;

    private Dictionary<Vector2Int, RoomNode> generated =
        new Dictionary<Vector2Int, RoomNode>();
    
    private HashSet<string> openedChests =
        new HashSet<string>();

    [SerializeField] private float difficultyMultiplier = 1f;

    public float DifficultyMultiplier => difficultyMultiplier;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        cameraFollow = Camera.main.GetComponent<CameraFollow2D>();

        if (GameBootstrap.LoadSave)
        {
            LoadGame();
            return;
        }

        // Создаём стартовую комнату только для новой игры
        RoomController startRoom = Instantiate(startRoomPrefab);
        activeRooms.Add(startRoom);

        RoomNode startNode = new RoomNode
        {
            uniqueID = System.Guid.NewGuid().ToString(),
            prefab = startRoom,
            gridPosition = Vector2Int.zero
        };

        generated.Add(startNode.gridPosition, startNode);
        currentNode = startNode;

        Vector2 startRoomSize = GetRoomDimensions(startRoom);
        MinimapManager.Instance.CreateRoom(startNode.gridPosition, startRoomSize);
        MinimapManager.Instance.RevealRoom(Vector2Int.zero);
        MinimapManager.Instance.SetCurrent(startNode.gridPosition);

        cameraFollow.SetBounds(startRoom.cameraBounds);
        cameraFollow.InstantSnap();

        currentNode.prefab.ResetRoom();
        bossRoomPlaced = false;
    }

    public void ChangeRoom(ExitDirection dir)
    {
        Vector2Int target = currentNode.gridPosition + GetOffset(dir);
        RoomNode nextNode;

        if (!generated.TryGetValue(target, out nextNode))
        {
            RoomController prefab = GetRandomRoomPrefab(dir, target);
            if (prefab == null) return;

            RoomController newRoom = Instantiate(prefab);
            activeRooms.Add(newRoom);

            nextNode = new RoomNode
            {
                uniqueID = System.Guid.NewGuid().ToString(),
                prefab = newRoom,
                gridPosition = target
            };

            generated.Add(target, nextNode);
            MinimapManager.Instance.CreateRoom(target, GetRoomDimensions(newRoom));
            GenerateNeighbors(nextNode);
        }

        MinimapManager.Instance.DrawConnection(currentNode.gridPosition, target);

        // Выключаем старую комнату
        currentNode.prefab.gameObject.SetActive(false);

        // Включаем новую
        nextNode.prefab.gameObject.SetActive(true);
        nextNode.prefab.ResetRoom();

        currentNode = nextNode;
        currentNode.visited = true;
        MinimapManager.Instance.RevealRoom(target);
        MinimapManager.Instance.SetCurrent(target);

        Transform spawn = GetSpawnPoint(currentNode.prefab, dir);
        player.position = spawn.position;

        cameraFollow.SetBounds(currentNode.prefab.cameraBounds);
        cameraFollow.InstantSnap();

        StartCoroutine(SaveAfterFrame(dir));
    }
    
    private void GenerateNeighbors(RoomNode node)
    {
        if (generated.Count >= maxRooms) return;

        Vector2Int[] directions = { Vector2Int.left, Vector2Int.right, Vector2Int.up, Vector2Int.down };
        ExitDirection[] exitDirs = { ExitDirection.Left, ExitDirection.Right, ExitDirection.Up, ExitDirection.Down };
        bool[] hasExit = { node.prefab.hasLeft, node.prefab.hasRight, node.prefab.hasUp, node.prefab.hasDown };
    
        ExitDirection[] oppositeDirs = { ExitDirection.Right, ExitDirection.Left, ExitDirection.Down, ExitDirection.Up };

        for (int i = 0; i < 4; i++)
        {
            if (!hasExit[i]) continue;

            Vector2Int neighborPos = node.gridPosition + directions[i];
            if (generated.ContainsKey(neighborPos)) continue;
            if (generated.Count >= maxRooms) return;

            RoomController prefab = GetRandomRoomPrefab(exitDirs[i], neighborPos);
            if (prefab == null) continue;

            // ВАЖНО: проверяем что у префаба есть встречный выход
            bool hasOppositeExit = i switch
            {
                0 => prefab.hasRight, // мы идём влево → сосед должен иметь выход вправо
                1 => prefab.hasLeft,  // мы идём вправо → сосед должен иметь выход влево
                2 => prefab.hasDown,  // мы идём вверх → сосед должен иметь выход вниз
                3 => prefab.hasUp,    // мы идём вниз → сосед должен иметь выход вверх
                _ => false
            };

            if (!hasOppositeExit) continue; // ← ВОТ ЭТО ДОБАВЬ

            RoomController newRoom = Instantiate(prefab);
            activeRooms.Add(newRoom);
            newRoom.gameObject.SetActive(false);

            RoomNode neighborNode = new RoomNode
            {
                uniqueID = System.Guid.NewGuid().ToString(),
                prefab = newRoom,
                gridPosition = neighborPos
            };

            generated.Add(neighborPos, neighborNode);
            MinimapManager.Instance.CreateRoom(neighborPos, GetRoomDimensions(newRoom));
        }
    }
    
    private IEnumerator SaveAfterFrame(ExitDirection dir)
    {
        yield return new WaitForEndOfFrame();
        SaveGame();
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
                return size;
            }
        }
    
        // Запасной вариант — фиксированный размер
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

    private RoomController GetRandomRoomPrefab(ExitDirection dir, Vector2Int gridPos)
    {
        if (!bossRoomPlaced && generated.Count >= minRoomsBeforeBoss)
        {
            bossRoomPlaced = true;
            return preBossRoomPrefab;
        }

        List<RoomController> valid = new();

        ExitDirection oppositeDir = dir switch
        {
            ExitDirection.Left => ExitDirection.Right,
            ExitDirection.Right => ExitDirection.Left,
            ExitDirection.Up => ExitDirection.Down,
            ExitDirection.Down => ExitDirection.Up,
            _ => ExitDirection.Left
        };

        foreach (var r in possibleRoomPrefabs)
        {
            bool hasEntrance = oppositeDir switch
            {
                ExitDirection.Left => r.hasLeft,
                ExitDirection.Right => r.hasRight,
                ExitDirection.Up => r.hasUp,
                ExitDirection.Down => r.hasDown,
                _ => false
            };

            if (!hasEntrance) continue;

            bool alreadyGenerated = false;
            foreach (var room in generated)
            {
                if (room.Value.prefab.name.Replace("(Clone)", "") == r.name)
                {
                    alreadyGenerated = true;
                    break;
                }
            }
            if (alreadyGenerated) continue;

            if (!CanPlaceRoom(r, gridPos)) continue;

            valid.Add(r);
        }

        if (valid.Count == 0) return null;
        return valid[Random.Range(0, valid.Count)];
    }
    
    private bool CanPlaceRoom(RoomController room, Vector2Int pos)
    {
        // Проверяем что комната не будет иметь выходов в занятые клетки с несовместимыми комнатами
        Vector2Int[] dirs = { Vector2Int.left, Vector2Int.right, Vector2Int.up, Vector2Int.down };
        bool[] exits = { room.hasLeft, room.hasRight, room.hasUp, room.hasDown };

        for (int i = 0; i < 4; i++)
        {
            if (!exits[i]) continue;

            Vector2Int neighborPos = pos + dirs[i];
            if (!generated.ContainsKey(neighborPos)) continue; // пустая клетка — ок

            // Там уже есть комната — проверяем что у неё есть встречный выход
            RoomNode neighbor = generated[neighborPos];
            bool hasMatchingExit = i switch
            {
                0 => neighbor.prefab.hasRight, // мы смотрим влево → сосед должен иметь выход вправо
                1 => neighbor.prefab.hasLeft,  // мы смотрим вправо → сосед должен иметь выход влево
                2 => neighbor.prefab.hasDown,  // мы смотрим вверх → сосед должен иметь выход вниз
                3 => neighbor.prefab.hasUp,    // мы смотрим вниз → сосед должен иметь выход вверх
                _ => false
            };

            if (!hasMatchingExit) return false; // выход упирается в стену соседа
        }

        return true;
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
    
    public void SaveGame()
{
    SaveData data = new SaveData();

    data.hasRun = true;

    // coins
    data.coins = CoinManager.Instance.Coins;

    // chest
    data.openedChests = new List<string>(openedChests);

    // room
    data.currentRoomX = currentNode.gridPosition.x;
    data.currentRoomY = currentNode.gridPosition.y;

    // player HP
    data.playerHP = playerController != null ? playerController.CurrentHealth : 0;

    // player position
    data.playerX = player.position.x;
    data.playerY = player.position.y;

    // enemies
    data.killedEnemies = new List<string>(killedEnemies);

    // rooms
    data.generatedRooms.Clear();
    data.roomPrefabs.Clear();

    foreach (var room in generated)
    {
        data.generatedRooms.Add($"{room.Key.x};{room.Key.y}");
        data.roomPrefabs.Add(room.Value.prefab.name.Replace("(Clone)", ""));
    }
    
    Debug.Log($"[SaveGame] Saved {data.generatedRooms.Count} rooms");
    for (int i = 0; i < data.generatedRooms.Count; i++)
    {
        Debug.Log($"[SaveGame] Room {i}: pos={data.generatedRooms[i]}, prefab={data.roomPrefabs[i]}");
    }
    Debug.Log($"[SaveGame] Current room: {data.currentRoomX}, {data.currentRoomY}");
    Debug.Log($"[SaveGame] Player pos: {data.playerX}, {data.playerY}");

    if (UpgradeManager.Instance != null)
    {
        data.upgrades.swordDamageLevel = playerController.bonusDamage;
        data.upgrades.airDamageLevel = playerController.bonusAirDamage;
        data.upgrades.airBounceLevel = Mathf.RoundToInt(playerController.bonusBounceForce);
        data.upgrades.hpLevel = playerController.bonusHealth;
        data.upgrades.speedLevel = Mathf.RoundToInt(playerController.bonusSpeed * 10);
        data.upgrades.shieldLevel = Mathf.RoundToInt(playerController.bonusShieldReduction * 50);
        data.upgrades.shieldEfficiencyLevel = Mathf.RoundToInt(playerController.bonusShieldDrainReduction * 2);
        data.upgrades.jumpLevel = playerController.extraJumps;
        data.upgrades.dashUnlocked = playerController.canDash;
    }

    SaveSystem.Save(data);
}
    
    public void LoadGame()
{
    SaveData data = SaveSystem.Load();

    if (data == null)
    {
        Debug.Log("[LoadGame] No save data found");
        return;
    }

    if (!data.hasRun)
    {
        Debug.Log("[LoadGame] hasRun is false");
        return;
    }

    Debug.Log($"[LoadGame] Loading {data.generatedRooms.Count} rooms");
    Debug.Log($"[LoadGame] Current room from save: {data.currentRoomX}, {data.currentRoomY}");
    Debug.Log($"[LoadGame] Player pos from save: {data.playerX}, {data.playerY}");

    loadedFromSave = true;

    CoinManager.Instance.SetCoins(data.coins);
    killedEnemies = new HashSet<string>(data.killedEnemies);
    openedChests = new HashSet<string>(data.openedChests);

    Vector2Int roomPos = new Vector2Int(data.currentRoomX, data.currentRoomY);

    // Восстанавливаем комнаты
    for (int i = 0; i < data.generatedRooms.Count; i++)
    {
        string[] split = data.generatedRooms[i].Split(';');
        Vector2Int pos = new Vector2Int(int.Parse(split[0]), int.Parse(split[1]));

        if (generated.ContainsKey(pos)) continue;

        string prefabName = data.roomPrefabs[i];
        RoomController prefab = null;

        if (prefabName == startRoomPrefab.name)
            prefab = startRoomPrefab;
        else if (prefabName == preBossRoomPrefab.name)
            prefab = preBossRoomPrefab;
        else if (prefabName == bossRoomPrefab.name)
            prefab = bossRoomPrefab;
        else
        {
            foreach (var p in possibleRoomPrefabs)
            {
                if (p.name == prefabName)
                {
                    prefab = p;
                    break;
                }
            }
        }

        if (prefab == null) continue;

        RoomController newRoom = Instantiate(prefab);
        newRoom.gameObject.SetActive(false);
        activeRooms.Add(newRoom);

        RoomNode node = new RoomNode
        {
            uniqueID = System.Guid.NewGuid().ToString(),
            prefab = newRoom,
            gridPosition = pos
        };

        generated.Add(pos, node);
        MinimapManager.Instance.CreateRoom(pos, GetRoomDimensions(newRoom));
    }
    
    bossRoomPlaced = false;

    foreach (var room in generated.Values)
    {
        if (room.prefab.name.Replace("(Clone)", "") == preBossRoomPrefab.name)
        {
            bossRoomPlaced = true;
            break;
        }
    }

    // Устанавливаем текущую комнату
    if (!generated.TryGetValue(roomPos, out currentNode))
    {
        Debug.LogError($"[LoadGame] Room {roomPos} not found!");
        return;
    }

    // Выключаем все комнаты
    foreach (var r in generated)
        r.Value.prefab.gameObject.SetActive(false);

    // Включаем текущую
    currentNode.prefab.gameObject.SetActive(true);
    currentNode.prefab.ResetRoom();

    cameraFollow.SetBounds(currentNode.prefab.cameraBounds);
    cameraFollow.InstantSnap();

    MinimapManager.Instance.SetCurrent(roomPos);
    MinimapManager.Instance.RevealRoom(roomPos);

    // Отрисовка соединений
    foreach (var room in generated)
    {
        Vector2Int pos = room.Key;
        Vector2Int[] dirs = { Vector2Int.left, Vector2Int.right, Vector2Int.up, Vector2Int.down };

        foreach (var dir in dirs)
        {
            Vector2Int neighbour = pos + dir;
            if (generated.ContainsKey(neighbour))
                MinimapManager.Instance.DrawConnection(pos, neighbour);
        }
    }
    
    foreach (var room in generated)
    {
        MinimapManager.Instance.RevealRoom(room.Key);
    }

    // Позиция игрока
    player.position = new Vector3(data.playerX, data.playerY, 0);

    // Улучшения
    if (data.upgrades != null)
    {
        playerController.bonusDamage = data.upgrades.swordDamageLevel;
        playerController.bonusAirDamage = data.upgrades.airDamageLevel;
        playerController.bonusBounceForce = data.upgrades.airBounceLevel;
        playerController.bonusHealth = data.upgrades.hpLevel;
        playerController.bonusSpeed = data.upgrades.speedLevel / 10f;
        playerController.bonusShieldReduction = data.upgrades.shieldLevel / 50f;
        playerController.bonusShieldDrainReduction = data.upgrades.shieldEfficiencyLevel / 2f;
        playerController.extraJumps = data.upgrades.jumpLevel;
        playerController.canDash = data.upgrades.dashUnlocked;
    }

    // HP
    if (playerController != null)
        playerController.SetHealth(data.playerHP);
}
    
    public void OnPlayerDeath()
    {
        Debug.Log($"[OnPlayerDeath] Saving coins: {CoinManager.Instance.Coins}");
        // Сохраняем монеты в мета-прогрессию
        SaveManager.SaveCoins(CoinManager.Instance.Coins);
    
        // Сохраняем run с пометкой "мёртв", чтобы Continue был доступен
        SaveData data = new SaveData();
        data.hasRun = true;
        data.playerIsDead = true;
        data.coins = CoinManager.Instance.Coins;
        // Остальные поля не важны — всё равно новый забег
    
        SaveSystem.Save(data);
    }
    
    public void MarkChestOpened(string id)
    {
        openedChests.Add(id);
    }

    public bool IsChestOpened(string id)
    {
        return openedChests.Contains(id);
    }
    
    public RoomController GetRoom(Vector2Int pos)
    {
        if (generated.TryGetValue(pos, out RoomNode node))
            return node.prefab;

        return null;
    }
}