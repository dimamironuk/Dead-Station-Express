using System.IO;
using UnityEngine;

public class SaveManager
{
    private readonly string _filePath = Path.Combine(Application.persistentDataPath, "train_save.json");

    public void Save(SaveData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(_filePath, json);
    }

    public SaveData Load()
    {
        if (!HasSave()) return new SaveData();

        string json = File.ReadAllText(_filePath);
        return JsonUtility.FromJson<SaveData>(json);
    }

    public bool HasSave() => File.Exists(_filePath);

    public void ClearSave()
    {
        if (File.Exists(_filePath))
        {
            File.Delete(_filePath);
        }
    }
}