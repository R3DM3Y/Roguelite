using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MinimapManager : MonoBehaviour
{
    public static MinimapManager Instance;

    public RectTransform mapRoot;
    public GameObject roomPrefab;
    public GameObject linePrefab;

    public float scale = 250f;

    private Dictionary<Vector2Int, RectTransform> rooms =
        new Dictionary<Vector2Int, RectTransform>();

    private Vector2Int currentPos;
    
    public float moveSpeed = 10f;
    private Vector2 targetPos;
    private List<RectTransform> lines = new();

    private void Awake()
    {
        Instance = this;
    }
    
    private void Update()
    {
        mapRoot.anchoredPosition =
            Vector2.Lerp(mapRoot.anchoredPosition, targetPos, Time.deltaTime * moveSpeed);
    }

    public void CreateRoom(Vector2Int gridPos)
    {
        if (rooms.ContainsKey(gridPos))
            return;

        GameObject obj = Instantiate(roomPrefab, mapRoot);
        RectTransform rt = obj.GetComponent<RectTransform>();

        rt.anchoredPosition = gridPos * (int)scale;

        rooms.Add(gridPos, rt);
    }

    public void DrawConnection(Vector2Int a, Vector2Int b)
    {
        if (!rooms.ContainsKey(a) || !rooms.ContainsKey(b))
            return;

        GameObject line = Instantiate(linePrefab, mapRoot);
        RectTransform rt = line.GetComponent<RectTransform>();

        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0f, 0.5f);
        rt.localScale = Vector3.one;

        Image img = rt.GetComponent<Image>();
        if (img != null)
            img.color = new Color(1f, 1f, 1f, 0.6f);

        Vector2 posA = rooms[a].anchoredPosition;
        Vector2 posB = rooms[b].anchoredPosition;

        Vector2 dir = (posB - posA).normalized;

        float roomSizeOffset = scale * 0.5f;

        Vector2 start = posA + dir * roomSizeOffset;
        Vector2 end = posB - dir * roomSizeOffset;

        Vector2 center = (start + end) / 2f;
        float length = Vector2.Distance(start, end);

        rt.anchoredPosition = center;
        rt.sizeDelta = new Vector2(length, 6f);
        rt.rotation = Quaternion.Euler(0, 0, Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg);

        lines.Add(rt);
    }

    public void SetCurrent(Vector2Int pos)
    {
        currentPos = pos;

        targetPos = -(Vector2)pos * scale;

        Refresh();
    }

    private void Refresh()
    {
        foreach (var line in lines)
        {
            Image img = line.GetComponent<Image>();
            if (img != null)
                img.color = new Color(1f, 1f, 1f, 0.3f);
        }

        if (rooms.TryGetValue(currentPos, out var current))
        {
            current.GetComponent<Image>().color = Color.white;
        }
    }
}