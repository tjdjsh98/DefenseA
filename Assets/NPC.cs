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

    /**대화창 프로퍼티
    0-9 : 흭득한 아이템 프로퍼티
    10-19 : 아이템 프로퍼티
    100 - 199 : 커스텀 프로퍼티
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

    // ,를 이용하여 구분하여 사용
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

    // ,를 이용하여 구분하여 사용
    // 첫번 째는 저장할 프로퍼티 인자, 두번 째는 나올 아이템 랭크
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