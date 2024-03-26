using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TestNPC : MonoBehaviour,IInteractable
{
    Player _player;
    Player Player { get { if (_player == null) _player = Managers.GetManager<GameManager>().Player; return _player; } }

    [SerializeField] GameObject _bubble;

    [SerializeField] Dialog[] _dialogs;
    private void Awake()
    {
        HideBubble();
    }
 
    public void ShowBubble()
    {
        if (_bubble == null) return;

        _bubble.SetActive(true);
    }

    public void HideBubble()
    {
        if (_bubble == null) return;

        _bubble.SetActive(false);
    }

    void OpenDialog()
    {
        Managers.GetManager<UIManager>().GetUI<UIDialog>().AssginDialog(_dialogs);
        Managers.GetManager<UIManager>().GetUI<UIDialog>().Open();
    }

    public void MoveDialog(int index)
    {
        Managers.GetManager<UIManager>().GetUI<UIDialog>().MoveDialog(index);
    }
    public void CloseDialog()
    {
        Managers.GetManager<UIManager>().GetUI<UIDialog>().Close();
    }
    public void OpenUpgradeUI()
    {
        Managers.GetManager<UIManager>().GetUI<UIUpgrade>().Open();
    }
    public void OpenShopUI()
    {
        Managers.GetManager<UIManager>().GetUI<UIShop>().Open();
    }

    public void AddMaxHp(int value)
    {
        Managers.GetManager<GameManager>().Girl.AddMaxHp(value);
    }

    public void RecoverHp(int hp)
    {
        Managers.GetManager<GameManager>().Girl.Hp += hp;
    }

    public void Interact()
    {
        Debug.Log(gameObject);
        OpenDialog();
    }
}

[System.Serializable]
public struct Dialog
{
    [TextArea]public string dialog;
    public SelectionDialog[] selectionDialogs;
    public bool isEndDialog;
}

[System.Serializable]
public struct SelectionDialog
{
    public string dialog;
    public UnityEvent unityEvent;
}
