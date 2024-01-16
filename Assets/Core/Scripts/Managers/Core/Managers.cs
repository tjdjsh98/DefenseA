using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Managers : MonoBehaviour
{
    static Managers _instance;


    // Core
    static Dictionary<Define.CoreManagers, Manager>  _coreManagers = new Dictionary<Define.CoreManagers, Manager>();

    // Content
    static Dictionary<Define.ContentManagers, Manager> _contentManagers = new Dictionary<Define.ContentManagers, Manager>();

    public static T GetManager<T>() where T : Manager
    {
        if (_instance == null)
            Init();

        foreach(var manager in _coreManagers.Values)
        {
            if(manager as T)
            {
                return manager as T;
            }
        }
        foreach (var manager in _contentManagers.Values)
        {
            if (manager as T)
            {
                return manager as T;
            }
        }

        return null;
    }

    static void Init()
    {
        _instance = Util.FindOrCreate("Manager").GetOrAddComponent<Managers>();

        for(int i = 0; i < Define.CORE_MANAGER_COUNT; i++)
        {
            AddCoreManager((Define.CoreManagers)i);
        }
        for (int i = 0; i < Define.CONTENT_MANAGER_COUNT; i++)
        {
            AddContentManager((Define.ContentManagers)i);
        }
    }

    private void Update()
    {
        foreach(var manager in _coreManagers.Values)
        {
            manager.ManagerUpdate();
        }
        foreach (var manager in _contentManagers.Values)
        {
            manager.ManagerUpdate();
        }
    }

    static void AddCoreManager(Define.CoreManagers managerName)
    {
        Manager manager = null;
        switch (managerName)
        {
            case Define.CoreManagers.Data:
                manager = _instance.gameObject.GetOrAddComponent<InputManager>();
                break;
            case Define.CoreManagers.Input:
                manager = _instance.gameObject.GetOrAddComponent<DataManager>();
                break;
        }

        if (manager)
        {
            manager.Init();
            _coreManagers.Add(managerName,manager);
        }
    }
    static void AddContentManager(Define.ContentManagers managerName)
    {
        Manager manager = null;
        switch (managerName)
        {
            case Define.ContentManagers.None:
                break;
            case Define.ContentManagers.Game:
                manager = _instance.gameObject.GetOrAddComponent<GameManager>();
                break;
            case Define.ContentManagers.END:
                break;
        }

        if (manager)
        {
            manager.Init();
            _contentManagers.Add(managerName, manager);
        }
    }
}
