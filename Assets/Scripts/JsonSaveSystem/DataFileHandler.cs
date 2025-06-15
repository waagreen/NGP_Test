using UnityEngine;
using System;
using System.IO;

public class DataFileHandler
{
    private readonly string dataDirectoryPath = "";
    private readonly string dataFileName = "";
    private const string codeWord = "gamesarehardtomake";
    private readonly bool useEncryption;

    public DataFileHandler(string dataDirectoryPath, string dataFileName, bool useEnscryption)
    {
        this.dataDirectoryPath = dataDirectoryPath;
        this.dataFileName = dataFileName;
        this.useEncryption = useEnscryption;
    }

    public SaveData Load()
    {
        string fullPath = Path.Combine(dataDirectoryPath, dataFileName);
        SaveData loadedData = null;
        if (File.Exists(fullPath))
        {
            try
            {
                string dataToRead = "";

                using FileStream stream = new(fullPath, FileMode.Open);
                using StreamReader reader = new(stream);

                dataToRead = reader.ReadToEnd();
                if (useEncryption) dataToRead = EncryptDecrypt(dataToRead);

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
            if (useEncryption) dataToStore = EncryptDecrypt(dataToStore);

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

    private string EncryptDecrypt(string data)
    {
        string modifiedData = "";

        for (int i = 0; i < data.Length; i++)
        {
            modifiedData += (char)(data[i] ^ codeWord[i % codeWord.Length]);
        }

        return modifiedData; 
    }
}
