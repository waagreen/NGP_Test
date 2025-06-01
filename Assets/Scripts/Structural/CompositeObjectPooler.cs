using System.Collections.Generic;
using UnityEngine;

public class CompositeObjectPooler : MonoBehaviour
{
    private Dictionary<string, Queue<PoolableObject>> pools = new();

    // Only one instance of the pooler can exist at time
    private static CompositeObjectPooler _instance;
    
    public static CompositeObjectPooler Instance
    {
        get
        {
            // Se não existe instância, tenta encontrar uma na cena
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<CompositeObjectPooler>();
                
                // Se ainda não existe, cria uma nova
                if (_instance == null)
                {
                    GameObject singletonObject = new (typeof(CompositeObjectPooler).Name);
                    _instance = singletonObject.AddComponent<CompositeObjectPooler>();
                }
            }
            return _instance;
        }
    }
    
    private void Awake()
    {
        // Make sure only one instance exists
        if (_instance != null && _instance != this)
        {
            Destroy(this);
        }
        else
        {
            _instance = this;
        }
    }

    private PoolableObject CreateNewObject(PoolableObject obj)
    {
        PoolableObject newObject = Instantiate(obj);
        newObject.name = obj.name;
        return newObject;
    }

    public void InitializeNewQueue(PoolableObject obj, int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            PoolableObject newObject = CreateNewObject(obj);
            newObject.gameObject.SetActive(false);
        }
    }

    public PoolableObject GetObject(PoolableObject obj)
    {
        if (pools.TryGetValue(obj.name, out Queue<PoolableObject> objectQueue))
        {
            if (objectQueue.Count < 1)
            {
                return CreateNewObject(obj);
            }
            else
            {
                PoolableObject _obj = objectQueue.Dequeue();
                _obj.gameObject.SetActive(true);
                return _obj;
            }
        }
        else return CreateNewObject(obj);
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

        obj.gameObject.SetActive(false);
    }
    
}
