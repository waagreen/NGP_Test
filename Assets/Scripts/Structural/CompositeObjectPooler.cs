using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CompositeObjectPooler : MonoBehaviour
{
    private readonly Dictionary<string, Queue<PoolableObject>> pools = new();

    private static CompositeObjectPooler _instance;
    public static CompositeObjectPooler Instance => _instance;

    private void Awake()
    {
        if ((_instance != null) && (_instance != this))
        {
            Debug.Log("More than one instance of COMPOSITE OBJECT POOLER. <color=#F03C32>Destroying it.</color>");
            Destroy(this);
        }
        else if (_instance == null)
        {
            _instance = this;
        }
    }

    private PoolableObject CreateNewObject(PoolableObject obj)
    {
        PoolableObject newObject = Instantiate(obj, transform);
        newObject.name = obj.name;
        return newObject;
    }

    public List<PoolableObject> GetNewPool(PoolableObject obj, int amount)
    {
        List<PoolableObject> list = new();

        for (int i = 0; i < amount; i++)
        {
            PoolableObject newObject = CreateNewObject(obj);
            ReturnObject(newObject);
            list.Add(newObject);
        }
        
        return list;
    }

    public void InitializeNewQueue(PoolableObject obj, int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            PoolableObject newObject = CreateNewObject(obj);
            ReturnObject(newObject);
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
