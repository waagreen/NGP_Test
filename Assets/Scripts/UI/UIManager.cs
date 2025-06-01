using UnityEngine;

public class UIManager : MonoBehaviour
{
    private Player player;
    [SerializeField] private LifeDisplay lifeDisplay;

    public void Setup(Player player)
    {
        this.player = player;

        player.OnHurt += lifeDisplay.UpdateLives;
    }

    private void OnDestroy()
    {
        player.OnHurt -= lifeDisplay.UpdateLives;
    }
}
