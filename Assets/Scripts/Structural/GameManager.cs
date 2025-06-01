using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Player settings")]
    [SerializeField] private Player playerPrefab;
    [SerializeField] private Transform initalSpawnPoint;

    private UIManager uI;
    private InputManager input;
    private Player player;

    private void Awake()
    {
        input = FindFirstObjectByType<InputManager>();
        input.SetupActionMap();

        uI = FindFirstObjectByType<UIManager>();

        player = Instantiate(playerPrefab, initalSpawnPoint);
        uI.Setup(player);
    }

    private void Start()
    {
        player.Setup(input);
    }
}
