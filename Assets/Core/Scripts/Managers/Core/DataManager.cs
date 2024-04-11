using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

public class DataManager : ManagerBase
{
    Dictionary<Type,Dictionary<int, Object>> _data = new Dictionary<Type, Dictionary<int, Object>>();

    public override void Init()
    {
        LoadData<CardData>("Datas/Card");
        LoadData<ItemData>("Datas/Item");
        LoadData<EnemyNameDefine>("Prefabs/Enemy");
        LoadData<Effect>("Prefabs/Effect");
        LoadData<Weapon>("Prefabs/Weapon");
        LoadData<Projectile>("Prefabs/Projectile");
    }

    public override void ManagerUpdate()
    {
    }

    void LoadData<T>(string path) where T : Object, ITypeDefine
    {
       
        T[] list = Resources.LoadAll<T>(path);

        if (!_data.ContainsKey(typeof(T)))
            _data.Add(typeof(T), new Dictionary<int, Object>());

        foreach (T t in list)
        {
            _data[typeof(T)].Add(t.GetEnumToInt(), t);

          
        }
    }

    public T GetData<T>(int type) where T : Object, ITypeDefine
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

    public List<T> GetDataList<T>(Func<T,bool> condition = null) where T : Object, ITypeDefine
    {
        List<T> list = new List<T>();

        if (_data.ContainsKey(typeof(T)))
        {
            foreach(var value in _data[typeof(T)].Values)
            {
                if(condition == null || condition.Invoke(value as T))
                {
                    list.Add(value as T);
                }
            }
        }

        return list;

    }

    public override void Destroy()
    {

    }
}
