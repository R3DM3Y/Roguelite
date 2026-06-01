using System;
using System.Collections.Generic;
using Save;

[Serializable]
public class SaveData
{
    public bool hasRun;

    // игрок
    public int coins;
    public int playerHP;

    // текущая комната
    public int currentRoomX;
    public int currentRoomY;
    public float playerX;
    public float playerY;

    // карта
    public List<string> generatedRooms =
        new();

    public List<string> roomPrefabs =
        new();

    public List<string> discoveredRooms =
        new();

    // враги
    public List<string> killedEnemies =
        new();

    // сундуки
    public List<string> openedChests =
        new();

    // улучшения
    public UpgradeSaveData upgrades =
        new UpgradeSaveData();
}