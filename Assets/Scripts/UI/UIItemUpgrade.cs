using MoreMountains.Tools;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public class UIItemUpgrade : UIBase
{
    [SerializeField] UIItemDescription _uiItemDescription;
    [SerializeField] Image _originImage;
    [SerializeField] TextMeshProUGUI _originName;
    [SerializeField] Image _typeAImage;
    [SerializeField] TextMeshProUGUI _typeAName;
    [SerializeField] Image _typeBImage;
    [SerializeField] TextMeshProUGUI _typeBName;


    [SerializeField] TextMeshProUGUI _moneyText;

    [SerializeField] GameObject _itemFolder;

    List<GameObject> _itemSlotList = new List<GameObject>();
    List<TextMeshProUGUI> _itemTextList = new List<TextMeshProUGUI>();

    List<ItemInfo> _itemList = new List<ItemInfo>();

    ItemInfo _selectedItemInfo;
    ItemName _selectedItemName;
    ItemName _typeAItemName;
    ItemName _typeBItemName;

    [SerializeField]float _upgradeCost = 150;

    public override void Init()
    {
        _moneyText.text = "";
        gameObject.SetActive(false);
    }

    public override void Open(bool except = false)
    {
        if(!except)
            Managers.GetManager<UIManager>().Open(this);

        Managers.GetManager<InputManager>().UIMouseDownHandler += OnUIMouseDown;
        Managers.GetManager<InputManager>().UIMouseHoverHandler += OnUIMouseHover;

        _selectedItemInfo = null;
        _selectedItemName = ItemName.None;
        _typeAItemName = ItemName.None;
        _typeBItemName = ItemName.None;

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
        RefreshItemList();
        RefreshUpgradeItem();
    }

    void RefreshItemList()
    {
        _itemList = Managers.GetManager<GameManager>().Inventory.GetItemInfoList((info)=>
        {
            if (info.ItemData.ItemType == ItemType.Weapon) return false;
            return !info.ItemData.ItemName.ToString().Contains('_');
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
            text.text = $"{info.ItemData.ItemName}";
            slot.gameObject.SetActive(true);
            index++;
        }

        for (; _itemSlotList.Count > index; index++)
        {
            _itemSlotList[index].gameObject.SetActive(false);
        }
    }

    void RefreshUpgradeItem()
    {
        if (_selectedItemInfo == null)
        {
            _originImage.sprite = null;
            _typeAImage.sprite = null;
            _typeBImage.sprite = null;
            _originName.text = "";
            _typeAName.text = "";
            _typeBName.text = "";
        }
        else
        {
            _selectedItemName = _selectedItemInfo.ItemData.ItemName;
            _typeAItemName = Util.ConvertStringToEnum<ItemName>($"{_selectedItemName}_A");
            _typeBItemName = Util.ConvertStringToEnum<ItemName>($"{_selectedItemName}_B");

            _originName.text = _selectedItemName.ToString();
            _typeAName.text = _typeAItemName.ToString();
            _typeBName.text = _typeBItemName.ToString();

            int rank = _selectedItemInfo.ItemData.Rank;

            if (rank == 0)
                _upgradeCost = Define.ItemRank0Price;
            if (rank == 1)
                _upgradeCost = Define.ItemRank1Price;
            if (rank == 2)
                _upgradeCost = Define.ItemRank2Price;
            if (rank == 3)
                _upgradeCost = Define.ItemRank3Price;
            _moneyText.text = $"필요금액:{_upgradeCost}";
        }
    }

    public void BuyTypeA()
    {
        if (Managers.GetManager<GameManager>().Money < _upgradeCost) return;

        Inventory inventory = Managers.GetManager<GameManager>().Inventory;
        if (inventory != null)
        {
            if (Managers.GetManager<GameManager>().SaveItemList.Contains(_selectedItemInfo))
                Managers.GetManager<GameManager>().SaveItemList.Remove(_selectedItemInfo);
            if (inventory.RemoveItem(_selectedItemName))
            {
                inventory.AddItem(_typeAItemName);
            }
        }

        _selectedItemInfo = null;
        _selectedItemName = ItemName.None;
        _typeAItemName = ItemName.None;
        _typeBItemName = ItemName.None;
        Refresh();
    }

    public void BuyTypeB()
    {
        if (Managers.GetManager<GameManager>().Money < _upgradeCost) return;

        Inventory inventory = Managers.GetManager<GameManager>().Inventory;
        if (inventory != null)
        {
            if (Managers.GetManager<GameManager>().SaveItemList.Contains(_selectedItemInfo))
                Managers.GetManager<GameManager>().SaveItemList.Remove(_selectedItemInfo);
            if (inventory.RemoveItem(_selectedItemName))
            {
                inventory.AddItem(_typeBItemName);
            }
        }

        _selectedItemInfo = null;
        _selectedItemName = ItemName.None;
        _typeAItemName = ItemName.None;
        _typeBItemName = ItemName.None;
        Refresh();
    }

    void OnUIMouseDown(List<GameObject> list)
    {
        for (int i = 0; i < _itemSlotList.Count; i++)
        {
            if (list.Contains(_itemSlotList[i]))
            {
                _selectedItemInfo = _itemList[i];
                RefreshUpgradeItem();
                return;
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

        if (_selectedItemName != ItemName.None && list.Contains(_originImage.gameObject))
        {
            _uiItemDescription.Show(_originImage.transform.position - Vector3.right * 10, _selectedItemName);
            isHover = true;
        }

        if (_typeAItemName != ItemName.None && list.Contains(_typeAImage.gameObject))
        {
            _uiItemDescription.Show(_typeAImage.transform.position - Vector3.right * 10, _typeAItemName);
            isHover = true;
        }
        if (_typeBItemName != ItemName.None && list.Contains(_typeBImage.gameObject))
        {
            _uiItemDescription.Show(_typeBImage.transform.position - Vector3.right * 10, _typeBItemName);
            isHover = true;
        }
        if (!isHover)
        {
            _uiItemDescription.Hide();
        }
    }
}
