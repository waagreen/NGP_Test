using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private float spawnRadius = 10f;
    [SerializeField] private float spawnInterval = 3f;
    [SerializeField] private int increasePerWave = 2;
    [SerializeField] private List<Enemy> aviableEnemies;

    private const int kPooledAmount = 30;

    private int enemyCount;
    private int amountToSpawn;
    private float lastSpawnTime;

    public System.Action OnWaveClear;

    private void UpdateBodyCount(Enemy killedEnemy)
    {
        killedEnemy.OnDeath -= UpdateBodyCount;
        enemyCount = Mathf.Max(0, enemyCount - 1);
        if (enemyCount == 0) OnWaveClear.Invoke();
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

        foreach (Enemy e in aviableEnemies)
        {
            CompositeObjectPooler.Instance.InitializeNewQueue(e, kPooledAmount);
        }

        Spawn();
    }

    private void Update()
    {
        if (Time.time < (lastSpawnTime + spawnInterval)) return;

        Spawn();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}
