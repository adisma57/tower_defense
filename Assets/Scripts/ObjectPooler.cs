using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private int poolSize = 5;

    private List<GameObject> _pool;

    void Start()
    {
        //create pool   
        _pool = new List<GameObject>();
        for (int i = 0; i < poolSize; i++)
        {
            CreateNewObject();
        }
    }

    private GameObject CreateNewObject()
    {
        GameObject obj = Instantiate(prefab, transform);
        obj.SetActive(false);
        _pool.Add(obj);
        return obj;
    }
    public GameObject GetPooledObject()
    {
        foreach (var obj in _pool)
        {
            if (!obj.activeSelf)
            {
                return obj;
            }
        }
        // Optionally expand pool if no inactive objects are available
        return CreateNewObject();
    }
}
