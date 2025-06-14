using UnityEngine;
using System;
using System.IO;

public class DataFileHandler
{
    private readonly string dataDirectoryPath = "";
    private readonly string dataFileName = "";

    public DataFileHandler(string dataDirectoryPath, string dataFileName)
    {
        this.dataDirectoryPath = dataDirectoryPath;
        this.dataFileName = dataFileName;
    }

    public SaveData Load()
    {
        string fullPath = Path.Combine(dataDirectoryPath, dataFileName);
        SaveData loadedData = null;
        if (File.Exists(fullPath))
        {
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

                string dataToRead;
                using FileStream stream = new(fullPath, FileMode.Open);
                using StreamReader reader = new(stream);

                dataToRead = reader.ReadToEnd();

                loadedData = JsonUtility.FromJson<SaveData>(dataToRead);
            }
            catch (Exception e)
            {
                Debug.LogError("Couldn't properly read save data on: " + fullPath + "\n" + e);
            }
        }

        return loadedData;
    }

    public bool Save(SaveData data)
    {
        string fullPath = Path.Combine(dataDirectoryPath, dataFileName);

        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath));

            string dataToStore = JsonUtility.ToJson(data, prettyPrint: false);
            using FileStream stream = new(fullPath, FileMode.Create);
            using StreamWriter writer = new(stream);

            writer.Write(dataToStore);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("Couldn't properly store the data on: " + fullPath + "\n" + e);
            return false;
        }
    }

    public void Clear()
    {
        SaveData clearData = new();
        if (Save(clearData)) Debug.Log($"Data cleared <color=#A2E622>SUCCESSFULLY</color>!");
    }
}
