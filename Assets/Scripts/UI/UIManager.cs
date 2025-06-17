using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private LifeDisplay lifeDisplay;
    [SerializeField] private TMP_Text mainTimer;
    [SerializeField] private Image spawnIntervalTime;
    [SerializeField] private GameObject deathScreen;

    private PlayerHealth player;
    private EnemySpawner spawner;
    private bool gameEnded;

    public void Setup(PlayerHealth player, EnemySpawner spawner)
    {
        this.player = player;
        this.spawner = spawner;
        gameEnded = false;

        lifeDisplay.UpdateLives(player.Health);
        player.OnHurt += lifeDisplay.UpdateLives;
        player.OnHurt += ShowDeathScreen;
        spawner.OnGameOver += ShowDeathScreen;
    }

    private void UpdateMainTimer()
    {
        string minutes = Mathf.Floor(spawner.CurrentTime / 60).ToString("00");
        string seconds = Mathf.Floor(spawner.CurrentTime % 60).ToString("00");
        mainTimer.SetText(minutes + ":" + seconds);
    }

    private void UpdateIntervalTimer()
    {
        float timeUntilNextSpawn = spawner.LastSpawnTime + spawner.SpawnInterval - Time.time;
        float progress = 1 - (timeUntilNextSpawn / spawner.SpawnInterval);
        progress = Mathf.Clamp(progress, 0f, 1f);
        spawnIntervalTime.fillAmount = progress;
    }

    public void ShowDeathScreen(int health)
    {
        if (health > 0) return;
        ShowDeathScreen();
    }

    private void ShowDeathScreen()
    {
        gameEnded = true;
        deathScreen.SetActive(true);
    }

    private void Update()
    {
        if ((spawner == null) || gameEnded) return;

        UpdateIntervalTimer();
        UpdateMainTimer();
    }

    private void OnDestroy()
    {
        if (player != null)
        {
            player.OnHurt -= lifeDisplay.UpdateLives;
            player.OnHurt -= ShowDeathScreen;
        }
        if (spawner != null) spawner.OnGameOver -= ShowDeathScreen;
    }
}
