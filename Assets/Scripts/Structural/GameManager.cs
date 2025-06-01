using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Player settings")]
    [SerializeField] private Player playerPrefab;
    [SerializeField] private Transform initalSpawnPoint;

    private InputManager input;
    private Player player;

    private void Awake()
    {
        input = FindFirstObjectByType<InputManager>();
        input.SetupActionMap();
    }

    private void Start()
    {
        player = Instantiate(playerPrefab, initalSpawnPoint);
        player.Setup(input);
    }
}
