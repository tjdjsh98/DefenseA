using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Managers : MonoBehaviour
{
    static Managers _instance;
    // Core
    static Dictionary<Define.CoreManagers, ManagerBase>  _coreManagers = new Dictionary<Define.CoreManagers, ManagerBase>();
    // Content
    static Dictionary<Define.ContentManagers, ManagerBase> _contentManagers = new Dictionary<Define.ContentManagers, ManagerBase>();

    public static bool IsQuit { private set; get; } 

    public static T GetManager<T>() where T : ManagerBase
    {
        if(IsQuit) return null;

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
        if (IsQuit) return;

        _instance = Util.FindOrCreate("[Manager]------------").GetOrAddComponent<Managers>();
        DontDestroyOnLoad(_instance);
        _coreManagers.Clear();
        _contentManagers.Clear();

        for(int i = 0; i < Define.CORE_MANAGER_COUNT; i++)
        {
            AddCoreManager((Define.CoreManagers)i);
        }
        foreach (var core in _coreManagers.Values)
        {
            core.Init();
        }

        for (int i = 0; i < Define.CONTENT_MANAGER_COUNT; i++)
        {
            AddContentManager((Define.ContentManagers)i);
        }
        foreach (var content in _contentManagers.Values)
        {
            content.Init();
        }

    }

    private void Update()
    {
        if (_instance == null) return;
        if (_instance.gameObject != gameObject)
        {
            Destroy(gameObject);
            return;
        }
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
        ManagerBase manager = null;
        switch (managerName)
        {
            case Define.CoreManagers.Data:
                manager = new GameObject("DataManager").GetOrAddComponent<DataManager>();
                break;
            case Define.CoreManagers.Input:
                manager = new GameObject("InputManager").GetOrAddComponent<InputManager>();
                break;
            case Define.CoreManagers.Resource:
                manager = new GameObject("ResourceManager").GetOrAddComponent<ResourceManager>();
                break;
        }

        if (manager)
        {
            manager.transform.parent = _instance.transform;
            _coreManagers.Add(managerName,manager);
        }
    }
    static void AddContentManager(Define.ContentManagers managerName)
    {
        ManagerBase manager = null;
     
        switch (managerName)
        {
            case Define.ContentManagers.None:
                break;
            case Define.ContentManagers.Game:
                manager = Util.FindOrCreate("GameManager").GetOrAddComponent<GameManager>();
                manager.transform.parent = _instance.gameObject.transform;
                break;
            case Define.ContentManagers.UI:
                manager = Util.FindOrCreate("UIManager").GetOrAddComponent<UIManager>();
                manager.transform.parent = _instance.gameObject.transform;
                break;
            case Define.ContentManagers.Effect:
                manager = Util.FindOrCreate("EffectManager").GetOrAddComponent<EffectManager>();
                manager.transform.parent = _instance.gameObject.transform;
                break;
            case Define.ContentManagers.Text:
                manager = Util.FindOrCreate("TextManager").GetOrAddComponent<TextManager>();
                manager.transform.parent = _instance.gameObject.transform;
                break;
            case Define.ContentManagers.Ability:
                manager = Util.FindOrCreate("AbilityManager").GetOrAddComponent<CardManager>();
                manager.transform.parent = _instance.gameObject.transform;
                break;
            case Define.ContentManagers.END:
                break;  
        }

        if (manager)
        {
            manager.transform.parent = _instance.transform;
            _contentManagers.Add(managerName, manager);
        }
    }

    private void OnApplicationQuit()
    {
        IsQuit = true;
    }
}
