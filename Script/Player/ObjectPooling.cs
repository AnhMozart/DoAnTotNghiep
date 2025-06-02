using System.Collections.Generic;
using UnityEngine;

public class ObjectPooling : MonoBehaviour
{
    [SerializeField] private int poolSize = 5;
    [SerializeField] private GameObject _prefab;
    [SerializeField] private Queue<GameObject> pool; //HÀNG CHỜ


    private void Awake()
    {
        pool = new Queue<GameObject>();
        for(int i = 0; i < poolSize;i++)
        {
            GameObject obj = Instantiate(_prefab);
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }


    public GameObject GetObject()
    {
        if (pool.Count > 0)
        {
            GameObject obj = pool.Dequeue();
            obj.SetActive(true);
            return obj;
        }
        else
        {
            GameObject obj = Instantiate(_prefab);
            return obj;
        }
    }


    public void ReturnGameObject(GameObject _obj)
    {
        _obj.SetActive(false);
        pool.Enqueue(_obj);
    }
}
