using UnityEngine;

public class PoolableObject : MonoBehaviour
{
    private void OnDisable()
    {
        CompositeObjectPooler.Instance.ReturnObject(this);
    }
}
