using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private float spawnRadius = 10f;
    [SerializeField] private float spawnInterval = 10f;
    [SerializeField] private int increasePerWave = 2;
    [SerializeField] private float initialTime = 60f;
    [SerializeField] private float timeBonus = 30f;

    [Header("Enemies settings")]
    [SerializeField] private List<Enemy> aviableEnemies;

    private int enemyCount;
    private int amountToSpawn;
    private float lastSpawnTime;
    private float currentTime;
    private bool gameOver = false;

    public float SpawnInterval => spawnInterval;
    public float LastSpawnTime => lastSpawnTime;
    public float CurrentTime => currentTime;

    public System.Action OnWaveClear;
    public System.Action OnGameOver;

    private void UpdateBodyCount(Enemy killedEnemy)
    {
        killedEnemy.OnDeath -= UpdateBodyCount;
        enemyCount = Mathf.Max(0, enemyCount - 1);

        if (enemyCount == 0)
        {
            // Adds time for a wave clear
            currentTime += timeBonus;

            OnWaveClear?.Invoke();
            Spawn(); // Immediately spawn next wave
        }
    }

    private void Spawn()
    {
        for (int i = 0; i < amountToSpawn; i++)
        {
            int randomIndex = Random.Range(0, aviableEnemies.Count - 1);
            Enemy enemyToSpawn = aviableEnemies[randomIndex];

            Vector3 position = spawnRadius * Random.insideUnitCircle;
            Enemy spawned = CompositeObjectPooler.Instance.GetObject(enemyToSpawn) as Enemy;
            spawned.transform.position = position;

            spawned.OnDeath += UpdateBodyCount;
        }

        enemyCount = amountToSpawn;
        amountToSpawn += increasePerWave;
        lastSpawnTime = Time.time;
    }

    private void Start()
    {
        amountToSpawn = increasePerWave;
        currentTime = initialTime;

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
            currentTime = 0;
            gameOver = true;
            OnGameOver?.Invoke();
            return;
        }

        // Spawn at regular intervals
        if (Time.time >= (lastSpawnTime + spawnInterval))
        {
            Spawn();
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}
