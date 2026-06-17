using System;
using System.IO;
using UnityEngine;

[Serializable]
public class SurvivorsSaveData
{
    public bool shotgunSurvivorUnlocked;
    public string selectedCharacterId = SurvivorsCharacterIds.Pistol;
    public int bestKillCount;
}

public static class SurvivorsCharacterIds
{
    public const string Pistol = "pistol";
    public const string Shotgun = "shotgun";
}

public static class SurvivorsSaveSystem
{
    private const string FileName = "survivors_save.json";

    public static string SavePath => Path.Combine(Application.persistentDataPath, FileName);

    public static SurvivorsSaveData Load()
    {
        if (!File.Exists(SavePath))
            return new SurvivorsSaveData();

        try
        {
            var json = File.ReadAllText(SavePath);
            var data = JsonUtility.FromJson<SurvivorsSaveData>(json);
            return data ?? new SurvivorsSaveData();
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Failed to load survivors save data: " + exception.Message);
            return new SurvivorsSaveData();
        }
    }

    public static void Save(SurvivorsSaveData data)
    {
        if (data == null)
            data = new SurvivorsSaveData();

        try
        {
            var directory = Path.GetDirectoryName(SavePath);
            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            var json = JsonUtility.ToJson(data, true);
            File.WriteAllText(SavePath, json);
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Failed to save survivors data: " + exception.Message);
        }
    }
}
