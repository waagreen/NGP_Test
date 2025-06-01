using UnityEngine;

[System.Serializable]
public class ItemData
{
    public string id;
    public int amount;
}

[System.Serializable]
public class SaveData
{
    public ItemData[] inventory;
}
