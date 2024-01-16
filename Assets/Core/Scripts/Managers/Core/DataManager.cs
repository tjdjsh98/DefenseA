using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : Manager
{
    Dictionary<Type,Dictionary<int, MonoBehaviour>> _data = new Dictionary<Type, Dictionary<int, MonoBehaviour>>();

    public override void Init()
    {
        
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
}
