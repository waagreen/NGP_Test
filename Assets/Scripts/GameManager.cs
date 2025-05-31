using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Player settings")]
    [SerializeField] private Player playerPrefab;
    [SerializeField] private Transform initalSpawnPoint;

    private InputManager input;
    private Player player;

    // Only script on the scene using awake 
    private void Awake()
    {
        input = FindFirstObjectByType<InputManager>();
        input.SetupActionMap();

        player = Instantiate(playerPrefab, initalSpawnPoint);
        player.Setup(input);
    }
}
