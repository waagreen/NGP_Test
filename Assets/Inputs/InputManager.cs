using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    private Actions actionMap;

    private Vector2 movementInput;

    public Vector2 Movement => movementInput;

    private void UpdateMovementInput(InputAction.CallbackContext ctx)
    {
        movementInput = ctx.ReadValue<Vector2>();

        // Preserve analog movement and normalize keyboard input
        Vector2.ClampMagnitude(movementInput, 1f);
    }

    public void SetupActionMap()
    {
        actionMap = new();
        actionMap.Enable();

        actionMap.Player.Move.performed += UpdateMovementInput;
        actionMap.Player.Move.started += UpdateMovementInput;
        actionMap.Player.Move.canceled += UpdateMovementInput;
    }

    private void OnDestroy()
    {
        actionMap.Player.Move.performed -= UpdateMovementInput;
        actionMap.Player.Move.started -= UpdateMovementInput;
        actionMap.Player.Move.canceled -= UpdateMovementInput;
    }
}
