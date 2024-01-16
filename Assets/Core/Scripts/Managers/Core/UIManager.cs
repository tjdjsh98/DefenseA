using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : ManagerBase
{
    const string _canvasName = "Canvas";
    GameObject _canvas;

    List<UIBase> _uiList = new List<UIBase>();

    public override void Init()
    {
        _canvas = GameObject.Find(_canvasName);

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
}
