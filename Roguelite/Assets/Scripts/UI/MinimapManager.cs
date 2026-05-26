using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinimapManager : MonoBehaviour
{
    public static MinimapManager Instance;

    public RectTransform mapRoot;
    public GameObject roomPrefab;
    public GameObject linePrefab;

    [Header("Настройки карты")]
    public float baseSpacing = 50f;     // Базовое расстояние между центрами
    public float spacingPadding = 10f;  // Дополнительный отступ между краями комнат
    public float maxRoomSize = 40f;
    public float minRoomSize = 15f;
    public float lineWidth = 4f;

    private Dictionary<Vector2Int, RectTransform> rooms = new();
    private Dictionary<Vector2Int, Vector2> roomSizes = new();
    private Dictionary<Vector2Int, Vector2> roomPositions = new(); // Фактические позиции на карте
    private Vector2Int currentPos;
    
    public float moveSpeed = 10f;
    private Vector2 targetPos;
    private List<RectTransform> lines = new();
    private HashSet<(Vector2Int, Vector2Int)> drawnConnections = new();

    private void Awake()
    {
        Instance = this;
    }
    
    private void Update()
    {
        mapRoot.anchoredPosition =
            Vector2.Lerp(mapRoot.anchoredPosition, targetPos, Time.deltaTime * moveSpeed);
    }

    public void CreateRoom(Vector2Int gridPos, Vector2 worldSize)
    {
        if (rooms.ContainsKey(gridPos))
        {
            Debug.LogWarning($"Room at {gridPos} already exists!");
            return;
        }

        // Нормализуем размер иконки
        Vector2 mapSize = NormalizeRoomSize(worldSize);
        
        // Вычисляем позицию с учётом реальных размеров соседей
        Vector2 mapPosition = CalculateRoomPosition(gridPos, mapSize);
        
        Debug.Log($"Creating minimap room at grid={gridPos}, world={worldSize}, map={mapSize}, pos={mapPosition}");

        GameObject obj = Instantiate(roomPrefab, mapRoot);
        RectTransform rt = obj.GetComponent<RectTransform>();

        rt.anchoredPosition = mapPosition;
        rt.sizeDelta = mapSize;

        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);

        roomSizes[gridPos] = mapSize;
        roomPositions[gridPos] = mapPosition;
        rooms.Add(gridPos, rt);

        // Обновляем позиции существующих комнат, если нужно
        UpdateExistingRoomPositions(gridPos);
        
        // Перерисовываем все соединения
        RedrawAllConnections();

        if (rooms.Count == 1)
        {
            SetCurrent(gridPos);
        }
    }

    // Вычисляет позицию комнаты так, чтобы она не накладывалась на соседей
    private Vector2 CalculateRoomPosition(Vector2Int gridPos, Vector2 roomSize)
    {
        Vector2 position = Vector2.zero;
        
        // Проверяем соседей и вычисляем отступы
        Vector2Int[] directions = {
            Vector2Int.left, Vector2Int.right,
            Vector2Int.up, Vector2Int.down
        };
        
        foreach (var dir in directions)
        {
            Vector2Int neighborPos = gridPos + dir;
            if (roomPositions.ContainsKey(neighborPos))
            {
                Vector2 neighborSize = roomSizes[neighborPos];
                Vector2 neighborPosition = roomPositions[neighborPos];
                
                // Вычисляем желаемое расстояние между центрами
                float halfCurrentSize = (dir.x != 0) ? roomSize.x * 0.5f : roomSize.y * 0.5f;
                float halfNeighborSize = (dir.x != 0) ? neighborSize.x * 0.5f : neighborSize.y * 0.5f;
                float spacing = halfCurrentSize + halfNeighborSize + spacingPadding;
                
                // Обновляем позицию по нужной оси
                if (dir.x != 0)
                    position.x = neighborPosition.x + dir.x * spacing;
                if (dir.y != 0)
                    position.y = neighborPosition.y + dir.y * spacing;
            }
        }
        
        // Если нет соседей — используем базовую сетку
        if (position.magnitude < 0.01f)
        {
            position = new Vector2(gridPos.x * baseSpacing, gridPos.y * baseSpacing);
        }
        
        return position;
    }

    // Обновляет позиции комнат, которые могли сместиться
    private void UpdateExistingRoomPositions(Vector2Int newRoomPos)
    {
        // Рекурсивно обновляем позиции, начиная с новой комнаты
        HashSet<Vector2Int> visited = new HashSet<Vector2Int>();
        UpdateRoomPositionRecursive(newRoomPos, visited);
    }

    private void UpdateRoomPositionRecursive(Vector2Int pos, HashSet<Vector2Int> visited)
    {
        if (!rooms.ContainsKey(pos) || visited.Contains(pos))
            return;
            
        visited.Add(pos);
        
        Vector2Int[] directions = {
            Vector2Int.left, Vector2Int.right,
            Vector2Int.up, Vector2Int.down
        };
        
        foreach (var dir in directions)
        {
            Vector2Int neighborPos = pos + dir;
            if (rooms.ContainsKey(neighborPos) && !visited.Contains(neighborPos))
            {
                // Пересчитываем позицию соседа относительно текущей комнаты
                Vector2 currentSize = roomSizes[pos];
                Vector2 currentPosition = roomPositions[pos];
                Vector2 neighborSize = roomSizes[neighborPos];
                
                float halfCurrentSize = (dir.x != 0) ? currentSize.x * 0.5f : currentSize.y * 0.5f;
                float halfNeighborSize = (dir.x != 0) ? neighborSize.x * 0.5f : neighborSize.y * 0.5f;
                float spacing = halfCurrentSize + halfNeighborSize + spacingPadding;
                
                Vector2 newPosition = currentPosition;
                if (dir.x != 0)
                    newPosition.x = currentPosition.x + dir.x * spacing;
                if (dir.y != 0)
                    newPosition.y = currentPosition.y + dir.y * spacing;
                
                roomPositions[neighborPos] = newPosition;
                rooms[neighborPos].anchoredPosition = newPosition;
                
                // Рекурсивно обновляем дальше
                UpdateRoomPositionRecursive(neighborPos, visited);
            }
        }
    }

    // Перерисовывает все соединения
    private void RedrawAllConnections()
    {
        // Удаляем старые линии
        foreach (var line in lines)
        {
            if (line != null)
                Destroy(line.gameObject);
        }
        lines.Clear();
        drawnConnections.Clear();
        
        // Рисуем заново все соединения
        var connections = new HashSet<(Vector2Int, Vector2Int)>();
        
        foreach (var roomA in rooms.Keys)
        {
            foreach (var roomB in rooms.Keys)
            {
                if (roomA == roomB) continue;
                
                // Проверяем, являются ли комнаты соседями по сетке
                Vector2Int diff = roomB - roomA;
                if (Mathf.Abs(diff.x) + Mathf.Abs(diff.y) == 1) // Манхэттенское расстояние = 1
                {
                    var conn1 = (roomA, roomB);
                    var conn2 = (roomB, roomA);
                    
                    if (!connections.Contains(conn1) && !connections.Contains(conn2))
                    {
                        connections.Add(conn1);
                        DrawConnectionLine(roomA, roomB);
                    }
                }
            }
        }
    }

    // Рисует линию между двумя комнатами
    private void DrawConnectionLine(Vector2Int a, Vector2Int b)
    {
        if (!rooms.ContainsKey(a) || !rooms.ContainsKey(b))
            return;

        Vector2 posA = roomPositions[a];
        Vector2 posB = roomPositions[b];
        
        Vector2 diff = posB - posA;
        float distance = diff.magnitude;
        
        if (distance < 0.001f)
            return;

        Vector2 dir = diff.normalized;

        GameObject lineObj = Instantiate(linePrefab, mapRoot);
        RectTransform rt = lineObj.GetComponent<RectTransform>();

        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0f, 0.5f);
        rt.localScale = Vector3.one;

        Image img = rt.GetComponent<Image>();
        if (img != null)
        {
            img.color = new Color(1f, 1f, 1f, 0.8f);
            img.raycastTarget = false;
        }

        Vector2 sizeA = roomSizes[a];
        Vector2 sizeB = roomSizes[b];
        
        float offsetA = GetRoomEdgeDistance(sizeA, dir);
        float offsetB = GetRoomEdgeDistance(sizeB, -dir);
        
        Vector2 start = posA + dir * offsetA;
        Vector2 end = posB - dir * offsetB;
        
        Vector2 lineDiff = end - start;
        float length = lineDiff.magnitude;

        if (length <= 0.01f)
        {
            Destroy(lineObj);
            return;
        }

        float angle = Mathf.Atan2(lineDiff.y, lineDiff.x) * Mathf.Rad2Deg;

        rt.anchoredPosition = start;
        rt.sizeDelta = new Vector2(length, lineWidth);
        rt.localRotation = Quaternion.Euler(0, 0, angle);

        rt.SetAsFirstSibling();
        lines.Add(rt);
    }

    // Оригинальный DrawConnection теперь вызывает RedrawAllConnections
    public void DrawConnection(Vector2Int a, Vector2Int b)
    {
        // Этот метод оставлен для обратной совместимости
        // Новые соединения будут добавлены при следующем CreateRoom
        var connection1 = (a, b);
        var connection2 = (b, a);
    
        if (!drawnConnections.Contains(connection1) && !drawnConnections.Contains(connection2))
        {
            drawnConnections.Add(connection1);
            DrawConnectionLine(a, b);
        }
    }

    private float GetRoomEdgeDistance(Vector2 roomSize, Vector2 direction)
    {
        if (direction.magnitude < 0.001f) return 0;
        
        float halfWidth = roomSize.x * 0.5f;
        float halfHeight = roomSize.y * 0.5f;
        
        float tx = Mathf.Abs(direction.x) > 0.001f ? halfWidth / Mathf.Abs(direction.x) : float.MaxValue;
        float ty = Mathf.Abs(direction.y) > 0.001f ? halfHeight / Mathf.Abs(direction.y) : float.MaxValue;
        
        return Mathf.Min(tx, ty);
    }

    private Vector2 NormalizeRoomSize(Vector2 worldSize)
    {
        float maxDimension = Mathf.Max(worldSize.x, worldSize.y);
        float standardRoomSize = 20f;
        float scaleFactor = maxRoomSize / standardRoomSize;
        
        Vector2 mapSize = worldSize * scaleFactor;
        
        mapSize.x = Mathf.Clamp(mapSize.x, minRoomSize, maxRoomSize * 2f);
        mapSize.y = Mathf.Clamp(mapSize.y, minRoomSize, maxRoomSize * 2f);
        
        return mapSize;
    }

    public void SetCurrent(Vector2Int pos)
    {
        if (!rooms.ContainsKey(pos))
        {
            Debug.LogError($"Cannot set current room: {pos} doesn't exist");
            return;
        }

        currentPos = pos;
        targetPos = -roomPositions[pos];
        Refresh();
    }

    private void Refresh()
    {
        lines.RemoveAll(line => line == null);

        foreach (var line in lines)
        {
            if (line == null) continue;
            Image img = line.GetComponent<Image>();
            if (img != null)
                img.color = new Color(1f, 1f, 1f, 0.3f);
        }

        foreach (var kvp in rooms)
        {
            if (kvp.Value == null) continue;
            Image img = kvp.Value.GetComponent<Image>();
            if (img != null)
            {
                if (kvp.Key == currentPos)
                    img.color = Color.white;
                else
                    img.color = new Color(0.5f, 0.5f, 0.5f, 0.7f);
            }
        }
    }
}