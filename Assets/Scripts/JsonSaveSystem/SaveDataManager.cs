using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class SaveDataManager : MonoBehaviour
{
    [Header("Json storage settings")]
    [SerializeField] private string fileName;
    [SerializeField] private bool useEncryption;

    private static SaveDataManager _instance;
    public static SaveDataManager Instance
    {
        get
        {
            // If instance is null, try to find one in the scene
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<SaveDataManager>();

                // If that fails, create one
                if (_instance == null)
                {
                    GameObject singletonObject = new(typeof(SaveDataManager).Name);
                    _instance = singletonObject.AddComponent<SaveDataManager>();
                }
            }
            return _instance;
        }
    }

    private SaveData data;
    private List<ISaveData> saveDataObjects;
    private DataFileHandler handler;

    private void CreateHandler()
    {
        handler = new DataFileHandler(Application.persistentDataPath, fileName, useEncryption);
    }

    private void Awake()
    {
        Debug.Assert(_instance == null, "More than one instance of SAVE DATA MANAGER", this);
    }

    private void Start()
    {
        CreateHandler();
        saveDataObjects = FindAllSaveObjects();

        // Always load data on start
        LoadGame();
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

    public void NewGameData()
    {
        data = new();
    }

    public void ClearGameData()
    {
        if (handler == null) CreateHandler();
        handler.Clear();
    }

    public void LoadGame()
    {
        data = handler.Load();

        if (data == null)
        {
            Debug.LogWarning("New game. Using default data values.");
            NewGameData();
        }

        foreach (ISaveData save in saveDataObjects)
        {
            save.LoadData(data);
        }
    }

    public void SaveGame()
    {
        foreach (ISaveData save in saveDataObjects)
        {
            save.SaveData(ref data);
        }

        handler.Save(data);
    }

    private List<ISaveData> FindAllSaveObjects()
    {
        IEnumerable<ISaveData> objects = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ISaveData>();
        return new List<ISaveData>(objects);
    }
}
