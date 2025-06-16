using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private PlayerHealth life;
    [SerializeField] private PlayerMovement movement;
    [SerializeField] private PlayerAttack attack;

    public PlayerHealth Health => life;

    public void InitializeComponents(InputManager inputManager)
    {
        life.Setup(inputManager);
        movement.Setup(inputManager);
        attack.Setup(inputManager);
    }
}
