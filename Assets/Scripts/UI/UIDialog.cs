using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UIDialog : UIBase
{
    Action action = null;

    Canvas _canvas;
    public Canvas Canvas=> _canvas;
    public Camera Camera=> _canvas?_canvas.worldCamera?_canvas.worldCamera:null:null;

    [SerializeField] UIDialogText _dialogText;

    Dictionary<int, UnityEvent> _selectionActionHanlders = new Dictionary<int, UnityEvent>();

    Dialog[] _dialogs;

    int _dialogIndex = 0;

    public override void Init()
    {
        _canvas = Managers.GetManager<UIManager>().Canvas;

        _dialogText.Init(this);

        if (_dialogText)
            _dialogText.DownCallback = SelectDialog;


        gameObject.SetActive(false);
    }

    public void AssginDialog(Dialog[] dialogs)
    {
        _dialogs = dialogs;
        _dialogIndex = 0;
    }
    public override void Open(bool except = false)
    {
        if (!except)
            Managers.GetManager<UIManager>().Open(this);

        Time.timeScale = 0;
        RefreshDialog();

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
        int index = int.Parse(id);

        if (_selectionActionHanlders.ContainsKey(index))
        {
            _selectionActionHanlders[index].Invoke();
        }
    }

    public void MoveDialog(int index)
    {
        _dialogIndex= index;
        RefreshDialog();
    }
    public void NextDialog()
    {
        if (gameObject.activeSelf)
        {
            if (_dialogs[_dialogIndex].isEndDialog)
            {
                Close();
                return;
            }
            _dialogIndex++;
            RefreshDialog();
        }
    }

    void RefreshDialog()
    {
        if (_dialogs.Length <= _dialogIndex)
        {
            Close();
        }
        else
        {
            Dialog dialog = _dialogs[_dialogIndex];

            _dialogText.SetText(dialog.dialog);
            _selectionActionHanlders.Clear();
            int selectionIndex = 1;
            foreach (var selection in dialog.selectionDialogs)
            {
                _dialogText.AddSelectText(selectionIndex, selection.dialog);
                _selectionActionHanlders.Add(selectionIndex, selection.unityEvent);
                selectionIndex++;
            }
        }
    }
}
