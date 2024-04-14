using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class NPC : MonoBehaviour,IInteractable
{
    Player _player;
    Player Player { get { if (_player == null) _player = Managers.GetManager<GameManager>().Player; return _player; } }

    [SerializeField] Object _arg;

    [SerializeField] bool _isDisposable;
    bool _isEndUse;

    [SerializeField] GameObject _bubble;
    [SerializeField] Dialog[] _dialogs;

    /**��ȭâ ������Ƽ
    0-9 : ŉ���� ������ ������Ƽ
    10-19 : ������ ������Ƽ
    100 - 199 : Ŀ���� ������Ƽ
    **/
    public ItemData[] ItemDataProperties = new ItemData[10];

    private void Awake()
    {
        
        HideBubble();
    }
 
    public void ShowBubble()
    {
        if (_isDisposable && _isEndUse) return;
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
        Managers.GetManager<UIManager>().GetUI<UIDialog>().AssginDialog(this,_dialogs);
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
        WeaponUpgrader weaponUpgrader = GetComponent<WeaponUpgrader>();
        if (weaponUpgrader != null)
            Managers.GetManager<UIManager>().GetUI<UIUpgrade>().Open(weaponUpgrader);
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
        if (_isDisposable && _isEndUse) return;
        OpenDialog();
        _isEndUse = true;
        HideBubble();
    }
    public void MoveLevel()
    {
        MapData mapData = _arg as MapData;
        if (mapData)
        {
            Managers.GetManager<GameManager>().LoadScene(mapData);
            CloseDialog();
        }
    }

    // ,�� �̿��Ͽ� �����Ͽ� ���
    public void MoveRandomDialog(string indexText)
    {
        string[] words = indexText.Split(',');
        List<int> indices = new List<int>();
        foreach(var word in words)
        {
            if (int.TryParse(word, out int index))
            {
                indices.Add(index);
            }
        }
        if (indices.Count <= 0) return;

        Managers.GetManager<UIManager>().GetUI<UIDialog>().MoveDialog(indices.GetRandom());
    }

    // ,�� �̿��Ͽ� �����Ͽ� ���
    // ù�� °�� ������ ������Ƽ ����, �ι� °�� ���� ������ ��ũ
    public void GetRandomItem(string indexNRank)
    {
        string[] words = indexNRank.Split(',');
        int propertyIndex = int.Parse(words[0]);
        int rank = int.Parse(words[1]);

        ItemData itemData = Managers.GetManager<GameManager>().RankItemDataList[rank].GetRandom();

        ItemDataProperties[propertyIndex] = itemData;

        Managers.GetManager<GameManager>().Inventory.AddItem(itemData);
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