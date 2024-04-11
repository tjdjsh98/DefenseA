using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class ResourceManager : ManagerBase
{
    Dictionary<string, List<Poolable>> _pool = new Dictionary<string, List<Poolable>>();
    GameObject _poolFolder;

    public Action<GameObject> GameObjectDestroyedHandler;

    public override void Init()
    {
        _poolFolder = new GameObject("[Pool]----------");
        DontDestroyOnLoad(_poolFolder);
    }

    public override void ManagerUpdate()
    {
    }

    public GameObject Instantiate(string path)
    {
        GameObject result = null;

        GameObject origin = Resources.Load<GameObject>(path);

        if (origin == null)
        {
            Debug.LogWarning($"{path}�� �ش��ϴ� ���ҽ��� �����ϴ�.");   
            return null;
        }
        Poolable pool = null;
        // Ǯ�� ������ ������Ʈ��� Ǯ���� ������Ʈ�� ã���ϴ�.
        if ((pool = origin.GetComponent<Poolable>()) != null)
        {
            if (!_pool.ContainsKey(origin.name))
                _pool.Add(origin.name, new List<Poolable>());
            if (_pool[origin.name] != null)
            {
                foreach (var p in _pool[origin.name])
                {
                    if (!p.IsUse)
                    {
                        result = p.gameObject;
                        p.IsUse = true;
                        break;
                    }
                }

            }
        }

        // Ǯ�� ������ ������Ʈ�� ���ٸ� ���� ������ݴϴ�.
        if (result == null)
        {
            result = Object.Instantiate(origin);
            // Ǯ�� �����ϴٸ� Ǯ�����ݴϴ�.
            if (pool != null)
            {
                if (!_pool.ContainsKey(origin.name))
                    _pool.Add(origin.name, new List<Poolable>());

                _pool[origin.name].Add(result.GetComponent<Poolable>());
                _pool[origin.name][_pool[origin.name].Count - 1].IsUse = true;
            }
        }

        result.transform.SetParent(null);
        result.gameObject.SetActive(true);

        return result;
    }
    public T Instantiate<T>(string path) where T : MonoBehaviour
    {
        T result = null;

        T origin = Resources.Load<T>(path);

        if (origin == null)
        {
            Debug.LogWarning($"{path}�� �ش��ϴ� ���ҽ��� �����ϴ�.");
            return null;
        }
        Poolable pool = null;
        // Ǯ�� ������ ������Ʈ��� Ǯ���� ������Ʈ�� ã���ϴ�.
        if ((pool = origin.GetComponent<Poolable>()) != null )
        {
            if (!_pool.ContainsKey(origin.name))
                _pool.Add(origin.name, new List<Poolable>());
            if(_pool[origin.name] != null)
            {
                foreach (var p in _pool[origin.name])
                {
                    if (!p.IsUse)
                    {
                        result = p.GetComponent<T>();
                        p.IsUse = true;
                        break;
                    }
                }

            }
        }

        // Ǯ�� ������ ������Ʈ�� ���ٸ� ���� ������ݴϴ�.
        if (result == null)
        {
            result = Object.Instantiate<T>(origin);
            // Ǯ�� �����ϴٸ� Ǯ�����ݴϴ�.
            if (pool != null)
            {
                if (!_pool.ContainsKey(origin.name))
                    _pool.Add(origin.name, new List<Poolable>());

                _pool[origin.name].Add(result.GetComponent<Poolable>());
                _pool[origin.name][_pool[origin.name].Count - 1].IsUse = true;
            }
        }
        result.transform.SetParent(null);
        result.gameObject.SetActive(true);

        return result;
    }

    public GameObject Instantiate(GameObject origin) 
    {
        GameObject result = null;

        if (origin == null)
        {
            Debug.LogWarning($"{origin}�� �ش��ϴ� ���ҽ��� �����ϴ�."); 
            return null;
        }
        Poolable pool = null;
        // Ǯ�� ������ ������Ʈ��� Ǯ���� ������Ʈ�� ã���ϴ�.
        if ((pool = origin.GetComponent<Poolable>()) != null)
        {
            if (!_pool.ContainsKey(origin.name))
                _pool.Add(origin.name, new List<Poolable>());
            if (_pool[origin.name] != null)
            {
                foreach (var p in _pool[origin.name])
                {
                    if (!p.IsUse)
                    {
                        result = p.gameObject;
                        p.IsUse = true;
                        break;
                    }
                }

            }
        }

        // Ǯ�� ������ ������Ʈ�� ���ٸ� ���� ������ݴϴ�.
        if (result == null)
        {
            result = Object.Instantiate(origin);
            // Ǯ�� �����ϴٸ� Ǯ�����ݴϴ�.
            if (pool != null)
            {
                if (!_pool.ContainsKey(origin.name))
                    _pool.Add(origin.name, new List<Poolable>());

                _pool[origin.name].Add(result.GetComponent<Poolable>());
                _pool[origin.name][_pool[origin.name].Count - 1].IsUse = true;
            }
        }
        result.transform.SetParent(null);
        result.gameObject.SetActive(true);

        return result;
    }
    public new T Instantiate<T>(T origin) where T : MonoBehaviour
    {
        T result = null;

        if (origin == null)
        {
            Debug.LogWarning($"{origin}�� �ش��ϴ� ���ҽ��� �����ϴ�.");
            return null;
        }
        Poolable pool = null;
        // Ǯ�� ������ ������Ʈ��� Ǯ���� ������Ʈ�� ã���ϴ�.
        if ((pool = origin.GetComponent<Poolable>()) != null)
        {
            if (!_pool.ContainsKey(origin.name))
                _pool.Add(origin.name, new List<Poolable>());
            if (_pool[origin.name] != null)
            {
                foreach (var p in _pool[origin.name])
                {
                    if (!p.IsUse)
                    {
                        result = p.GetComponent<T>();
                        p.IsUse = true;
                        break;
                    }
                }

            }
        }

        // Ǯ�� ������ ������Ʈ�� ���ٸ� ���� ������ݴϴ�.
        if (result == null)
        {
            result = Object.Instantiate<T>(origin);
            // Ǯ�� �����ϴٸ� Ǯ�����ݴϴ�.
            if (pool != null)
            {
                if (!_pool.ContainsKey(origin.name))
                    _pool.Add(origin.name, new List<Poolable>());

                _pool[origin.name].Add(result.GetComponent<Poolable>());
                _pool[origin.name][_pool[origin.name].Count - 1].IsUse = true;
            }
        }
        result.transform.SetParent(null);
        result.gameObject.SetActive(true);

        return result;
    }
    public T Instantiate<T>(int index) where T : MonoBehaviour,ITypeDefine
    {
        T result = null;
        T origin = Managers.GetManager<DataManager>().GetData<T>(index);

        if (origin == null)
        {
            Debug.LogWarning($"{origin}�� �ش��ϴ� ���ҽ��� �����ϴ�.");
            return null;
        }
        Poolable pool = null;
        // Ǯ�� ������ ������Ʈ��� Ǯ���� ������Ʈ�� ã���ϴ�.
        if ((pool = origin.GetComponent<Poolable>()) != null)
        {
            if (!_pool.ContainsKey(origin.name))
                _pool.Add(origin.name, new List<Poolable>());
            if (_pool[origin.name] != null)
            {
                foreach (var p in _pool[origin.name])
                {
                    if (!p.IsUse)
                    {
                        result = p.GetComponent<T>();
                        p.IsUse = true;
                        break;
                    }
                }

            }
        }

        // Ǯ�� ������ ������Ʈ�� ���ٸ� ���� ������ݴϴ�.
        if (result == null)
        {
            result = Object.Instantiate<T>(origin);
            // Ǯ�� �����ϴٸ� Ǯ�����ݴϴ�.
            if (pool != null)
            {
                if (!_pool.ContainsKey(origin.name))
                    _pool.Add(origin.name, new List<Poolable>());

                _pool[origin.name].Add(result.GetComponent<Poolable>());
                _pool[origin.name][_pool[origin.name].Count - 1].IsUse = true;
            }
        }
        result.transform.SetParent(null);
        result.gameObject.SetActive(true);

        return result;
    }
    public void Destroy(GameObject gameObject)
    {
        Poolable pool = null;
        GameObjectDestroyedHandler?.Invoke(gameObject);
        if (gameObject)
        {
            if (pool = gameObject.GetComponent<Poolable>())
            {
                pool.IsUse = false;
                gameObject.SetActive(false);
                gameObject.transform.parent = _poolFolder.transform;
            }
            else
            {
                Object.Destroy(gameObject);
            }
        }
    }

    public override void Destroy()
    {
        foreach(var itemList in _pool.Values)
        {
            foreach (var item in itemList)
            {
                Destroy(item.gameObject);
            }
        }
        Destroy(_poolFolder);  
    }
}
