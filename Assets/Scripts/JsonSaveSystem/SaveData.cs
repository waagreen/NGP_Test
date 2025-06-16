using UnityEngine;

[System.Serializable]
public class ItemData
{
    public string id;
    public int slotIndex;
    public int amount;
}

[System.Serializable]
public class SaveData
{
    public ItemData[] inventory;
    public float remainingTime;
    public int currentWave;
    public int playerHealth;


    public SaveData()
    {
        inventory = new ItemData[0]; // Empty array by default
        remainingTime = 0f;
        currentWave = 0;
        playerHealth = 0;
    }
}
