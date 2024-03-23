using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIDialog : UIBase
{
    Action action = null;

    Canvas _canvas;
    public Canvas Canvas=> _canvas;
    public Camera Camera=> _canvas?_canvas.worldCamera?_canvas.worldCamera:null:null;

    [SerializeField] UIDialogText _dialogText;

    Action _selection1SelectedHandler;
    Action _selection2SelectedHandler;

    public override void Init()
    {
        _canvas = Managers.GetManager<UIManager>().Canvas;

        _dialogText.Init(this);

        if (_dialogText)
            _dialogText.DownCallback = SelectDialog;


        gameObject.SetActive(false);
    }

    public void AssginDialog(string text)
    {
        _dialogText.SetText(text);
    }
    public void AssginSelection1(string text, Action action)
    {
        _dialogText.AddSelectText(1,text);
        _selection1SelectedHandler = action;
    } 
    public void AssginSelection2(string text, Action action)
    {
        _dialogText.AddSelectText(2,text);
        _selection2SelectedHandler = action;
    }


    public override void Open(bool except = false)
    {
        if (!except)
            Managers.GetManager<UIManager>().Open(this);

        Time.timeScale = 0;

        gameObject.SetActive(true);

    }
    public override void Close(bool except = false)
    {
        gameObject.SetActive(false);

        Time.timeScale = 1;

        if (!except)
            Managers.GetManager<UIManager>().Close(this);
    }

    void SelectDialog(string id, Vector2 postion)
    {
        Debug.Log(id + postion);
        if (id.Equals("1"))
            _selection1SelectedHandler?.Invoke();
        if (id.Equals("2"))
            _selection2SelectedHandler?.Invoke();
    }
}
