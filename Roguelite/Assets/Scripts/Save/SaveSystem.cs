using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private static string SavePath =>
        Application.persistentDataPath + "/save.json";

    public static void Save(SaveData data)
    {
        string json =
            JsonUtility.ToJson(data, true);

        File.WriteAllText(SavePath, json);

    }

    public static SaveData Load()
    {
        if (!File.Exists(SavePath))
            return null;

        string json =
            File.ReadAllText(SavePath);

        return JsonUtility.FromJson<SaveData>(json);
    }

    public static void DeleteRun()
    {
        if (File.Exists(SavePath))
            File.Delete(SavePath);
    }
}