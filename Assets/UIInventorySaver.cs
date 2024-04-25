using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class UIInventorySaver : UIBase
{
    [SerializeField] UIItemDescription _uiItemDescription;
    [SerializeField] GameObject _itemFolder;

    List<GameObject> _itemSlotList = new List<GameObject>();
    List<TextMeshProUGUI> _itemTextList = new List<TextMeshProUGUI>();

    List<ItemInfo> _itemList = new List<ItemInfo>();

    
    public override void Init()
    {
        gameObject.SetActive(false);
    }

    public override void Open(bool except = false)
    {
        if (!except)
            Managers.GetManager<UIManager>().Open(this);

        Managers.GetManager<InputManager>().UIMouseDownHandler += OnUIMouseDown;
        Managers.GetManager<InputManager>().UIMouseHoverHandler += OnUIMouseHover;


        Refresh();
        Time.timeScale = 0;
        gameObject.SetActive(true);
    }
    public override void Close(bool except = false)
    {
        Time.timeScale = 1;
        gameObject.SetActive(false);

        Managers.GetManager<InputManager>().UIMouseDownHandler -= OnUIMouseDown;
        Managers.GetManager<InputManager>().UIMouseHoverHandler -= OnUIMouseHover;



        if (!except)
            Managers.GetManager<UIManager>().Close(this);
    }

    void Refresh()
    {
        _itemList = Managers.GetManager<GameManager>().Inventory.GetItemInfoList((info) =>
        {
            if (info.ItemData.ItemType == ItemType.Weapon) return false;
            return true;
        });

        int index = 0;
        foreach (var info in _itemList)
        {
            GameObject slot = null;
            TextMeshProUGUI text = null;
            if (_itemSlotList.Count <= index)
            {
                slot = Managers.GetManager<ResourceManager>().Instantiate("Prefabs/UI/UI_ItemSlot");
                slot.transform.SetParent(_itemFolder.transform);
                slot.transform.localScale = Vector3.one;

                _itemSlotList.Add(slot);
                _itemTextList.Add(slot.transform.Find("Text").GetComponent<TextMeshProUGUI>());
            }
            slot = _itemSlotList[index];
            text = _itemTextList[index];
            if (Managers.GetManager<GameManager>().SaveItemList.Contains(info))
                text.text = $"<color=green>{info.ItemData.ItemName}</color>";
            else
                text.text = $"{info.ItemData.ItemName}";
            slot.gameObject.SetActive(true);
            index++;
        }

        for (; _itemSlotList.Count > index; index++)
        {
            _itemSlotList[index].gameObject.SetActive(false);
        }
    }

    void OnUIMouseDown(List<GameObject> list)
    {
        for (int i = 0; i < _itemSlotList.Count; i++)
        {
            if (list.Contains(_itemSlotList[i]))
            {
                if (Managers.GetManager<GameManager>().SaveItemList.Contains(_itemList[i]))
                {
                    Managers.GetManager<GameManager>().SaveItemList.Remove(_itemList[i]);
                    Refresh();
                    return;
                }
                else
                {
                    if(Managers.GetManager<GameManager>().SaveItemList.Count < Managers.GetManager<GameManager>().SaveItemCount)
                        Managers.GetManager<GameManager>().SaveItemList.Add(_itemList[i]);
                    Refresh();
                }
            }
        }
    }
    void OnUIMouseHover(List<GameObject> list)
    {
        bool isHover = false;

        for (int i = 0; i < _itemList.Count; i++)
        {
            if (list.Contains(_itemSlotList[i].gameObject))
            {
                isHover = true;
                if (_itemSlotList[i].transform.transform.position.x < 0)
                    _uiItemDescription.Show(_itemSlotList[i].transform.position + Vector3.right * 10, _itemList[i].ItemData);
                else
                    _uiItemDescription.Show(_itemSlotList[i].transform.position - Vector3.right * 10, _itemList[i].ItemData);
            }
        }
        if (!isHover)
        {
            _uiItemDescription.Hide();
        }
    }
}
