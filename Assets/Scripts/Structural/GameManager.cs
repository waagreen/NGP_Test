using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Player settings")]
    [SerializeField] private Player playerPrefab;
    [SerializeField] private Transform initalSpawnPoint;

    private UIManager uI;
    private InputManager input;
    private EnemySpawner spawner;
    private InventoryManager inventory;
    private Player player;

    private void Awake()
    {
        input = FindFirstObjectByType<InputManager>();
        input.SetupActionMap();

        spawner = FindFirstObjectByType<EnemySpawner>();
        uI = FindFirstObjectByType<UIManager>();

        inventory = FindFirstObjectByType<InventoryManager>();
        input.OnInventoryToggle += inventory.ToggleSequence;

        player = Instantiate(playerPrefab, initalSpawnPoint);
        uI.Setup(player, spawner);
    }

    private void Start()
    {
        player.Setup(input);
    }

    private void OnDestroy()
    {
        input.OnInventoryToggle -= inventory.ToggleSequence;
    }
}
