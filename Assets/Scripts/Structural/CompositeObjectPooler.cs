using System.Collections.Generic;
using UnityEngine;

public class CompositeObjectPooler : MonoBehaviour
{
    private readonly Dictionary<string, Queue<PoolableObject>> pools = new();

    // Only one instance of the pooler can exist at time
    private static CompositeObjectPooler _instance;
    
    public static CompositeObjectPooler Instance
    {
        get
        {
            // If instance is null, try to find one in the scene
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<CompositeObjectPooler>();
                
                // If that fails, create one
                if (_instance == null)
                {
                    GameObject singletonObject = new (typeof(CompositeObjectPooler).Name);
                    _instance = singletonObject.AddComponent<CompositeObjectPooler>();
                }
            }
            return _instance;
        }
    }

    private PoolableObject CreateNewObject(PoolableObject obj)
    {
        PoolableObject newObject = Instantiate(obj, transform);
        newObject.name = obj.name;
        return newObject;
    }

    public void InitializeNewQueue(PoolableObject obj, int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            PoolableObject newObject = CreateNewObject(obj);
            newObject.transform.SetParent(transform);
            newObject.gameObject.SetActive(false);
        }
    }

    public PoolableObject GetObject(PoolableObject obj)
    {
        PoolableObject _obj;
        if (pools.TryGetValue(obj.name, out Queue<PoolableObject> objectQueue))
        {
            if (objectQueue.Count < 1)
            {
                _obj = CreateNewObject(obj);
            }
            else
            {
                _obj = objectQueue.Dequeue();
            }
        }
        else _obj = CreateNewObject(obj);

        _obj.gameObject.SetActive(true);
        _obj.transform.SetParent(null);
        return _obj;
    }

    public void ReturnObject(PoolableObject obj)
    {
        if (pools.TryGetValue(obj.name, out Queue<PoolableObject> objectQueue))
        {
            objectQueue.Enqueue(obj);
        }
        else
        {
            Queue<PoolableObject> newQueue = new();
            newQueue.Enqueue(obj);
            pools[obj.name] = newQueue;
        }

        obj.transform.SetParent(transform);
        obj.gameObject.SetActive(false);
    }
    
}
