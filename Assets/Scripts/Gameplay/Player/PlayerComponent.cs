using UnityEngine;

public class PlayerComponent : MonoBehaviour
{
    protected InputManager input;
    protected bool isSet = false;

    public virtual void Setup(InputManager input)
    {
        if (isSet) return;
        this.input = input;
    }
}
