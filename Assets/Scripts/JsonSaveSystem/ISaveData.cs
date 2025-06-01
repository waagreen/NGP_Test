public interface ISaveData
{
    // Passing data as refeernce so the method can modify it
    public void SaveData(ref SaveData data);
    public void LoadData(SaveData data);
}
