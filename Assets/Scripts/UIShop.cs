using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIShop : UIBase
{
    [SerializeField]TextMeshProUGUI _currentMoneyText;

    [SerializeField] List<Button> _slotList;
    List<Image> _slotImageList = new List<Image>();
    List<TextMeshProUGUI> _slotTextList = new List<TextMeshProUGUI>();
    [SerializeField] List<ShopItem> _selectionList = new List<ShopItem>();

    [SerializeField] List<Sprite> _spriteList;
    
    public override void Init()
    {
        gameObject.SetActive(false);

        int index = 0;
        foreach (var slot in _slotList)
        {
            _slotImageList.Add(slot.transform.Find("Frame").Find("Image").GetComponent<Image>());
            _slotTextList.Add(slot.transform.Find("Text").GetComponent<TextMeshProUGUI>());

            int tempIndex = index;
            slot.onClick.AddListener(() => {
                if (_selectionList.Count <= tempIndex) return;
                if (_selectionList[tempIndex].isSale) return;

                GameManager gameManager = Managers.GetManager<GameManager>();

                if (_selectionList[tempIndex].shopItemData.price <= gameManager.Girl.Mental)
                {
                    gameManager.Girl.Mental -= _selectionList[tempIndex].shopItemData.price;
                    ShopItem info = _selectionList[tempIndex];
                    info.isSale = true;
                    _selectionList[tempIndex] = info;

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
        if (!except)
            Managers.GetManager<UIManager>().Open(this);
        Refresh();
        Time.timeScale = 0;
        gameObject.SetActive(true);
    }

    public void Open(List<ShopItem> list)
    {
        Managers.GetManager<UIManager>().Open(this);
        _selectionList.Clear();
        _selectionList.AddRange(list);
        Refresh();
        Time.timeScale = 0;
        gameObject.SetActive(true);
    }
    public override void Close(bool except = false)
    {
        Time.timeScale = 1;
        gameObject.SetActive(false);

        if (!except)
            Managers.GetManager<UIManager>().Close(this);
    }

    void Refresh()
    {
        for (int i = 0; i < _selectionList.Count; i++)
        {
            if (_selectionList[i].shopItemData.image)
            {
                _slotImageList[i].rectTransform.sizeDelta = Util.GetFitSpriteSize(_selectionList[i].shopItemData.image, 130);
                _slotImageList[i].sprite = _selectionList[i].shopItemData.image;
            }
            else
            {
                _slotImageList[i].rectTransform.sizeDelta = Vector2.zero;
                _slotImageList[i].sprite = null;
            }
            if (!_selectionList[i].isSale)
            {
                _slotTextList[i].text = $"{_selectionList[i].shopItemData.description}\n";
               
                _slotTextList[i].text += $"{_selectionList[i].shopItemData.price.ToString()}";
            }
            else
            {
                _slotTextList[i].text = "판매완료";
            }
        }
    }

}


[System.Serializable]
public class ShopItem
{
    public ShopItemData shopItemData;
    public bool isSale;
    public SellType sellType=>shopItemData.sellType;
}

