using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class UIShop : UIBase
{
    [SerializeField]TextMeshProUGUI _currentMoneyText;

    [SerializeField] List<Button> _slotList;
    List<Image> _slotImageList = new List<Image>();
    List<TextMeshProUGUI> _slotNameList = new List<TextMeshProUGUI>();
    List<TextMeshProUGUI> _slotDescroptionList = new List<TextMeshProUGUI>();
    List<TextMeshProUGUI> _slotMoney = new List<TextMeshProUGUI>();

    [SerializeField] List<Sprite> _spriteList;

    [SerializeField] TextMeshProUGUI _restockText;

    IShop _openShop;

    public override void Init()
    {
        gameObject.SetActive(false);

        int index = 0;
        foreach (var slot in _slotList)
        {
            _slotImageList.Add(slot.transform.Find("Frame").Find("Image").GetComponent<Image>());
            _slotNameList.Add(slot.transform.Find("Text").GetComponent<TextMeshProUGUI>());
            _slotMoney.Add(slot.transform.Find("MoneyText").GetComponent<TextMeshProUGUI>());
            _slotDescroptionList.Add(slot.transform.Find("Description").GetComponent<TextMeshProUGUI>());

            int tempIndex = index;
            slot.onClick.AddListener(() => {
                if (_openShop.ShopItemList.Count <= tempIndex) return;
                if (_openShop.ShopItemList[tempIndex].isSale) return;

                GameManager gameManager = Managers.GetManager<GameManager>();

                if (_openShop.ShopItemList[tempIndex].shopItemData.Price <= gameManager.Money)
                {
                    gameManager.Money -= _openShop.ShopItemList[tempIndex].shopItemData.Price;
                    ShopItem info = _openShop.ShopItemList[tempIndex];
                    info.isSale = true;
                    _openShop.ShopItemList[tempIndex] = info;

                    if (info.sellType == SellType.Weapon)
                    {
                        WeaponShopItemData weaponShopItemData = info.shopItemData as WeaponShopItemData;
                        if (weaponShopItemData)
                            gameManager.Player.WeaponSwaper.ChangeNewWeapon((int)weaponShopItemData.weaponPosition, weaponShopItemData.weaponName);
                    }
                    else if (info.sellType == SellType.Ability)
                    {
                        Managers.GetManager<UIManager>().GetUI<UICardSelection>().Open();
                    }
                    else if (info.sellType == SellType.UpgradeWeapon)
                    {
                        Managers.GetManager<UIManager>().GetUI<UIUpgrade>().Open();
                    }
                    else if(info.sellType == SellType.StatusUp)
                    {
                        StatusUpShopItemData statusUpShopItemData = info.shopItemData as StatusUpShopItemData;
                        if (statusUpShopItemData)
                        {
                            statusUpShopItemData.Apply();
                        }
                    }
                    Refresh();
                }
            });
            index++;
        }

        _isInitDone = true;
    }

    private void Update()
    {
        if (!_isInitDone) return;

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
        gameObject.SetActive(true);
    }
    public override void Close(bool except = false)
    {
        Time.timeScale = 1;
        gameObject.SetActive(false);
        if (!except)
        {
            Managers.GetManager<UIManager>().Close(this);
            _openShop = null;
        }
    }

    void Refresh()
    {
        if (Managers.GetManager<GameManager>().Money < _openShop.RestockCost)
        {
            _restockText.text = $"<color=\"red\">재입고 불가</color>";
        }
        else
        {
            _restockText.text = $"재입고\n{_openShop.RestockCost}";
        }

        for (int i = 0; i < _openShop.ShopItemList.Count; i++)
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
            if (!_openShop.ShopItemList[i].isSale)
            {
                if (_openShop.ShopItemList[i].shopItemData.Price > Managers.GetManager<GameManager>().Money)
                    _slotMoney[i].text = $"<color=\"red\">{_openShop.ShopItemList[i].shopItemData.Price.ToString()}</color>";
                else
                    _slotMoney[i].text = $"{_openShop.ShopItemList[i].shopItemData.Price.ToString()}";

                _slotNameList[i].text = $"{_openShop.ShopItemList[i].shopItemData.name}\n";
                _slotDescroptionList[i].text = $"{_openShop.ShopItemList[i].shopItemData.Description}\n";
               
            }
            else
            {
                _slotNameList[i].text = "판매완료";
            }
        }
    }

    public void ReStockItems()
    {
        if (_openShop.RestockCost > Managers.GetManager<GameManager>().Money) return;

        Managers.GetManager<GameManager>().Money -= _openShop.RestockCost;
        _openShop.RestockShopItems();
        _openShop.RestockCost = _openShop.RestockCost * 2;
        Refresh();
    }

}


[System.Serializable]
public class ShopItem
{
    public ShopItemData shopItemData;
    public bool isSale;
    public SellType sellType=>shopItemData.SellType;
}

