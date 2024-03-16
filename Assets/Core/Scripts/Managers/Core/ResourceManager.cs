using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using UnityEngine;

public class ResourceManager : ManagerBase
{
    Dictionary<string, List<Poolable>> _pool = new Dictionary<string, List<Poolable>>();
    GameObject _poolFolder;
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
            Debug.LogWarning($"{path}에 해당하는 리소스가 없습니다.");
            return null;
        }
        Poolable pool = null;
        // 풀링 가능한 오브젝트라면 풀링된 오브젝트를 찾습니다.
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

        // 풀링 가능한 오브젝트가 없다면 새로 만들어줍니다.
        if (result == null)
        {
            result = Object.Instantiate(origin);
            // 풀링 가능하다면 풀링해줍니다.
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
            Debug.LogWarning($"{path}에 해당하는 리소스가 없습니다.");
            return null;
        }
        Poolable pool = null;
        // 풀링 가능한 오브젝트라면 풀링된 오브젝트를 찾습니다.
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

        // 풀링 가능한 오브젝트가 없다면 새로 만들어줍니다.
        if (result == null)
        {
            result = Object.Instantiate<T>(origin);
            // 풀링 가능하다면 풀링해줍니다.
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
            Debug.LogWarning($"{origin}에 해당하는 리소스가 없습니다.");
            return null;
        }
        Poolable pool = null;
        // 풀링 가능한 오브젝트라면 풀링된 오브젝트를 찾습니다.
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

        // 풀링 가능한 오브젝트가 없다면 새로 만들어줍니다.
        if (result == null)
        {
            result = Object.Instantiate<T>(origin);
            // 풀링 가능하다면 풀링해줍니다.
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
            Debug.LogWarning($"{origin}에 해당하는 리소스가 없습니다.");
            return null;
        }
        Poolable pool = null;
        // 풀링 가능한 오브젝트라면 풀링된 오브젝트를 찾습니다.
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

        // 풀링 가능한 오브젝트가 없다면 새로 만들어줍니다.
        if (result == null)
        {
            result = Object.Instantiate<T>(origin);
            // 풀링 가능하다면 풀링해줍니다.
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
}
