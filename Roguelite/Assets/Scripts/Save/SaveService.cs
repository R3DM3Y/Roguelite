using System.IO;
using UnityEngine;

public class SaveService : MonoBehaviour
{
    public static SaveService Instance;

    private string path;

    private void Awake()
    {
        Instance = this;
        path = Application.persistentDataPath + "/save.json";
    }

    public void Save(SaveData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(path, json);
    }

    public SaveData Load()
    {
        if (!File.Exists(path))
            return null;

        string json = File.ReadAllText(path);
        return JsonUtility.FromJson<SaveData>(json);
    }
}