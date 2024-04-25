using DuloGames.UI.Tweens;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIShop : UIBase
{
    [SerializeField]TextMeshProUGUI _currentMoneyText;

    [SerializeField] List<GameObject> _slotList;
    [SerializeField] List<Button> _purchaseButtonList;
    List<Image> _slotImageList = new List<Image>();
    List<Image> _slotFrameList = new List<Image>();
    List<TextMeshProUGUI> _slotNameList = new List<TextMeshProUGUI>();
    List<TextMeshProUGUI> _slotMoney = new List<TextMeshProUGUI>();

    [SerializeField] List<Sprite> _spriteList;

    [SerializeField] GameObject _restockButton;
    [SerializeField] TextMeshProUGUI _restockText;

    [SerializeField] UIItemDescription _itemDescription;

    IShop _openShop;
    public override void Init()
    {
        gameObject.SetActive(false);

   

        int index = 0;
        foreach (var slot in _slotList)
        {
            _slotFrameList.Add(slot.transform.Find("Frame").GetComponent<Image>());
            _slotImageList.Add(slot.transform.Find("Frame").Find("Image").GetComponent<Image>());
            _slotNameList.Add(slot.transform.Find("Text").GetComponent<TextMeshProUGUI>());
            _purchaseButtonList.Add(slot.transform.Find("PurchaseButton").GetComponent<Button>());
            _slotMoney.Add(slot.transform.Find("PurchaseButton").Find("MoneyText").GetComponent<TextMeshProUGUI>());

            int tempIndex = index;
            _purchaseButtonList[_purchaseButtonList.Count-1].onClick.AddListener(() => {
                if (_openShop.ShopItemList.Count <= tempIndex) return;

                if (_openShop.ShopItemList[tempIndex].isSaled) return;
                _openShop.SellItem(_openShop.ShopItemList[tempIndex]);
                Refresh();
            });
            index++;
        }

        Managers.GetManager<InputManager>().UIMouseHoverHandler += OnUIMouseHover;

        _isInitDone = true;
    }

    public override void Open(bool except = false)
    {
        if (_openShop == null)
        {
            Close();
            return;
        }
        if (!except)
            Managers.GetManager<UIManager>().Open(this);

        Refresh();
        Time.timeScale = 0;
        gameObject.SetActive(true);
    }

    public void Open(IShop shop)
    {
        if(shop == null) return;
        _openShop = shop;

        Managers.GetManager<UIManager>().Open(this);
        Refresh();
        Time.timeScale = 0;
        _itemDescription.gameObject.SetActive(false);
        gameObject.SetActive(true);
    }
    public override void Close(bool except = false)
    {
        Time.timeScale = 1;
        gameObject.SetActive(false);
        _itemDescription.gameObject.SetActive(false);
        if (!except)
        {
            Managers.GetManager<UIManager>().Close(this);
            _openShop = null;
        }
    }

    void Refresh()
    {
        if (_openShop.RestockCount < Managers.GetManager<GameManager>().EnableRestockCount)
        {
            _restockButton.gameObject.SetActive(true);
        }else
        {
            _restockButton.gameObject.SetActive(false);
        }
     
        for (int i = 0; i < _openShop.ShopItemList.Count; i++)
        {
            if (_openShop.ShopItemList[i].shopItemData == null)
            {
                _slotImageList[i].rectTransform.sizeDelta = Vector2.zero;
                _slotImageList[i].sprite = null;
                _slotMoney[i].text = "";
                _slotNameList[i].text = "";
            }
            else
            {
                if (_openShop.ShopItemList[i].shopItemData.Image)
                {
                    _slotImageList[i].rectTransform.sizeDelta = Util.GetFitSpriteSize(_openShop.ShopItemList[i].shopItemData.Image, 130);
                    _slotImageList[i].sprite = _openShop.ShopItemList[i].shopItemData.Image;
                }
                else
                {
                    _slotImageList[i].rectTransform.sizeDelta = Vector2.zero;
                    _slotImageList[i].sprite = null;
                }
                if (!_openShop.ShopItemList[i].isSaled)
                {
                    if (_openShop.ShopItemList[i].Price > Managers.GetManager<GameManager>().Money)
                        _slotMoney[i].text = $"<color=\"red\">{_openShop.ShopItemList[i].Price.ToString()}</color>";
                    else
                        _slotMoney[i].text = $"{_openShop.ShopItemList[i].Price.ToString()}";

                    _slotNameList[i].text = $"{_openShop.ShopItemList[i].shopItemData.name}\n";

                }
                else
                {
                    _slotNameList[i].text = "�ǸſϷ�";
                }
            }
        }
    }

    public void ReStockItems()
    {
        _openShop.RestockShopItems();
        _openShop.RestockCount++;
        Refresh();
    }


    void OnUIMouseHover(List<GameObject> list)
    {
        bool isHover = false;

        for(int i = 0; i < _slotFrameList.Count; i++)
        {
            if (list.Contains(_slotFrameList[i].gameObject))
            {
                isHover = true;
                _itemDescription.gameObject.SetActive(true);
                if (_slotList[i].transform.transform.position.x < 0)
                    _itemDescription.Show(_slotList[i].transform.position + Vector3.right * 10, _openShop.ShopItemList[i].shopItemData);
                else
                    _itemDescription.Show(_slotList[i].transform.position - Vector3.right * 10, _openShop.ShopItemList[i].shopItemData);
                return;
            }
        }

        if (!isHover)
        {
            _itemDescription.Hide();
        }
    }
}




