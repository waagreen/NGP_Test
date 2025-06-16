using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour, ISaveData
{
    [SerializeField] private float spawnRadius = 10f;
    [SerializeField] private float spawnInterval = 10f;
    [SerializeField] private int increasePerWave = 2;
    [SerializeField] private float initialTime = 60f;
    [SerializeField] private float timeBonus = 30f;

    [Header("Enemies settings")]
    [SerializeField] private List<Enemy> aviableEnemies;

    private int currentWave;
    private int enemyCount;
    private int amountToSpawn;
    private float remainingTimeAtWaveStart;
    private float lastSpawnTime;
    private float currentTime;
    private bool gameOver = false;

    public float SpawnInterval => spawnInterval;
    public float LastSpawnTime => lastSpawnTime;
    public float CurrentTime => currentTime;

    public event System.Action OnWaveClear;
    public event System.Action OnGameOver;

    private void UpdateBodyCount(Enemy killedEnemy)
    {
        killedEnemy.OnDeath -= UpdateBodyCount;
        enemyCount = Mathf.Max(0, enemyCount - 1);

        // Anticipate next wave
        if (enemyCount == 0)
        {
            // Adds time for a wave clear
            currentTime += timeBonus;
            currentWave++;

            OnWaveClear?.Invoke();
            Spawn();
        }
    }

    private void Spawn()
    {
        amountToSpawn = increasePerWave * currentWave;

        for (int i = 0; i < amountToSpawn; i++)
        {
            int randomIndex = Random.Range(0, aviableEnemies.Count - 1);
            Enemy enemyToSpawn = aviableEnemies[randomIndex];

            Vector3 position = spawnRadius * Random.insideUnitCircle;
            Enemy spawned = CompositeObjectPooler.Instance.GetObject(enemyToSpawn) as Enemy;
            spawned.transform.position = position;

            spawned.OnDeath += UpdateBodyCount;
        }
        
        remainingTimeAtWaveStart = currentTime;
        enemyCount = amountToSpawn;
        lastSpawnTime = Time.time;
    }

    private void EndGame()
    {
        gameOver = true;
        currentTime = 0;
        currentWave = 0;
        remainingTimeAtWaveStart = 0f;

        OnGameOver?.Invoke();
    }

    private void Start()
    {
        foreach (Enemy e in aviableEnemies)
        {
            CompositeObjectPooler.Instance.InitializeNewQueue(e, amount: 30);
        }

        Spawn();
    }

    private void Update()
    {
        if (gameOver) return;

        currentTime -= Time.deltaTime;

        if (currentTime <= 0)
        {
            EndGame();
            return;
        }

        // Spawn wave at the regular interval
        if (Time.time >= (lastSpawnTime + spawnInterval))
        {
            currentWave++;
            Spawn();
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }

    public void SaveData(ref SaveData data)
    {
        data.currentWave = currentWave;
        data.remainingTime = remainingTimeAtWaveStart;
    }

    public void LoadData(SaveData data)
    {
        currentWave = Mathf.Max(1, data.currentWave);
        currentTime = (data.remainingTime > 0f) ? data.remainingTime : initialTime;
    }
}
