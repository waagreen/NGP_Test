using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private Actions actionMap;

    private Vector2 movementInput;
    private Vector2 shootInput;

    public Vector2 Shoot => shootInput;
    public Vector2 Movement => movementInput;

    private void UpdateMovementInput(InputAction.CallbackContext ctx)
    {
        movementInput = ctx.ReadValue<Vector2>();

        // Preserve analog movement and normalize keyboard input
        Vector2.ClampMagnitude(movementInput, 1f);
    }

    private void UpdateShootInput(InputAction.CallbackContext ctx)
    {
        // Shoot direction is always normalized, no analog suport
        shootInput = ctx.ReadValue<Vector2>().normalized;
    }

    public void SetupActionMap()
    {
        actionMap = new();
        actionMap.Enable();

        actionMap.Player.Move.performed += UpdateMovementInput;
        actionMap.Player.Move.started += UpdateMovementInput;
        actionMap.Player.Move.canceled += UpdateMovementInput;

        actionMap.Player.Look.performed += UpdateShootInput;
        actionMap.Player.Look.started += UpdateShootInput;
        actionMap.Player.Look.canceled += UpdateShootInput;
    }

    private void OnDestroy()
    {
        actionMap.Player.Move.performed -= UpdateMovementInput;
        actionMap.Player.Move.started -= UpdateMovementInput;
        actionMap.Player.Move.canceled -= UpdateMovementInput;

        actionMap.Player.Look.performed -= UpdateShootInput;
        actionMap.Player.Look.started -= UpdateShootInput;
        actionMap.Player.Look.canceled -= UpdateShootInput;

        actionMap.Disable();
        actionMap = null;
    }
}
