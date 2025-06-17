using Unity.VisualScripting;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Player settings")]
    [SerializeField] private Player playerPrefab;
    [SerializeField] private Transform initalSpawnPoint;
    [SerializeField] private CameraPlayerFollow principalCamera;
    [SerializeField] private UIManager uiManagerPrefab;
    [SerializeField] private InputManager inputManagerPrefab;
    [SerializeField] private EnemySpawner spawnerPrefab;
    [SerializeField] private InventoryManager inventoryManagerPrefab;

    private UIManager _ui;
    private InputManager _input;
    private EnemySpawner _spawner;
    private InventoryManager _inventory;
    private CameraPlayerFollow _camera;
    private Player _player;

    private void Awake()
    {
        _input = Instantiate(inputManagerPrefab, transform);
        _spawner = Instantiate(spawnerPrefab, transform);
        _ui = Instantiate(uiManagerPrefab, transform);
        _inventory = Instantiate(inventoryManagerPrefab, transform);
        _camera = Instantiate(principalCamera, transform);
        _player = Instantiate(playerPrefab, initalSpawnPoint);
    }

    private void Start()
    {
        // Setup all managers and core components
        _input.SetupActionMap();
        _player.InitializeComponents(_input);

        _ui.gameObject.SetActive(true);
        _ui.Setup(_player.Health, _spawner);

        _input.OnInventoryToggle += _inventory.ToggleSequence;
        _player.Health.OnHurt += _camera.Shake;

        _camera.Setup(_player.transform);
    }

    private void OnDestroy()
    {
        _input.OnInventoryToggle -= _inventory.ToggleSequence;
        _player.Health.OnHurt -= _camera.Shake;
    }
}
