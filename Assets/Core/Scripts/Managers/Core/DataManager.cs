using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.VFX;

public class DataManager : ManagerBase
{
    Dictionary<Type,Dictionary<int, MonoBehaviour>> _data = new Dictionary<Type, Dictionary<int, MonoBehaviour>>();

    public override void Init()
    {
        LoadData<Effect>("Prefabs/Effect");
    }

    public override void ManagerUpdate()
    {
        
    }

    void LoadData<T>(string path) where T : MonoBehaviour, TypeDefine
    {
        T[] list = Resources.LoadAll<T>(path);

        if (!_data.ContainsKey(typeof(T)))
            _data.Add(typeof(T), new Dictionary<int, MonoBehaviour>());

        foreach (T t in list)
        {
            _data[typeof(T)].Add(t.GetEnumToInt(), t);
        }
    }

    public T GetData<T>(int type) where T : MonoBehaviour, TypeDefine
    {
        if (_data.ContainsKey(typeof(T)))
        {
            if(_data[typeof(T)].ContainsKey(type))
            {
                return _data[typeof(T)][type] as T;
            }
        }
        
        return null;
    }
}
