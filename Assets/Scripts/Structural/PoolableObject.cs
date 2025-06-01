using UnityEngine;

public class PoolableObject : MonoBehaviour
{
    private void OnDisable()
    {
        Return();
    }

    public void Return()
    {
        if (!isActiveAndEnabled) return;
        CompositeObjectPooler.Instance.ReturnObject(this);
    }
}
