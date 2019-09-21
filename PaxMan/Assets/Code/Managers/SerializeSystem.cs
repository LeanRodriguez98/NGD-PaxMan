using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
public static class SerializeSystem
{
    private static bool IsSavedFile()
    {
        return Directory.Exists(Application.persistentDataPath + "/game_save");
    }

    public static void SaveGame(ScriptableObject _soToSave)
    {
        if (!IsSavedFile())
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/game_save");
        }
        if (!Directory.Exists(Application.persistentDataPath + "/game_save/" + _soToSave.name + "_data"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/game_save/" + _soToSave.name + "_data");
        }
        BinaryFormatter bf = new BinaryFormatter();
        FileStream file = File.Create(Application.persistentDataPath + "/game_save/" + _soToSave.name + "_data/" + _soToSave.name + "_save.txt");
        var json = JsonUtility.ToJson(_soToSave);
        bf.Serialize(file, json);
        file.Close();
    }

    public static void LoadGame(ScriptableObject _soToLoad)
    {
        if (!Directory.Exists(Application.persistentDataPath + "/game_save/" + _soToLoad.name + "_data"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/game_save/" + _soToLoad.name + "_data");
        }
        BinaryFormatter bf = new BinaryFormatter();
        if (File.Exists(Application.persistentDataPath + "/game_save/" + _soToLoad.name + "_data/" + _soToLoad.name + "_save.txt"))
        {
            FileStream file = File.Open(Application.persistentDataPath + "/game_save/" + _soToLoad.name + "_data/" + _soToLoad.name + "_save.txt", FileMode.Open);
            JsonUtility.FromJsonOverwrite((string)bf.Deserialize(file), _soToLoad);
            file.Close();
        }
    }
}
