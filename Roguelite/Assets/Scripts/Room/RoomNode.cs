using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class RoomNode
{
    public string uniqueID;
    public RoomController prefab;
    public Vector2Int gridPosition;

    public bool visited;

    // 🔥 связи с другими комнатами
    public List<Vector2Int> connections = new List<Vector2Int>();
}