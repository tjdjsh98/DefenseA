using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : ManagerBase
{
    const string _uiFolderName = "[UI]-----------";
    const string _canvasName = "Canvas";
    GameObject _uiFolder;
    Canvas _canvas;
    public Canvas Canvas => _canvas;
    List<UIBase> _uiList = new List<UIBase>();

    List<UIBase> _stackList = new List<UIBase>();
    public override void Init()
    {
        _uiFolder = GameObject.Find(_uiFolderName);
        if (_uiFolder == null)
        {
            _uiFolder = Managers.GetManager<ResourceManager>().Instantiate("Prefabs/" + _uiFolderName);
        }
        DontDestroyOnLoad(_uiFolder);
        _canvas = GameObject.Find(_canvasName).GetComponent<Canvas>();

        _canvas.transform.SetParent(_uiFolder.transform);
        for (int i = 0; i < _canvas.transform.childCount; i++)
        {
            UIBase ui = _canvas.transform.GetChild(i).GetComponent<UIBase>();

            if (ui != null)
            {
                ui.Init();
                _uiList.Add(ui);
            }
        }
    }

    public override void ManagerUpdate()
    {
        if(_canvas.worldCamera == null)
            _canvas.worldCamera = Camera.main;
    }

    public T GetUI<T>() where T : UIBase
    {
        foreach(var t in _uiList)
        {
            if(t as T)
            {
                return t as T;
            }
        }

        return null;
    }

    public void Open(UIBase ui)
    {
        if (_stackList.Count > 0)
            _stackList[_stackList.Count - 1].Close(true);
    
        _stackList.Add(ui);
    }

    public void Close(UIBase ui)
    {
        _stackList.Remove(ui);
        if (_stackList.Count > 0)
        {
            _stackList[_stackList.Count - 1].Open(true);
        }
    }

}
