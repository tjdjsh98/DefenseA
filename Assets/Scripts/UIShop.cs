using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class UIShop : UIBase
{
    [SerializeField]TextMeshProUGUI _currentMoneyText;

    [SerializeField] List<Button> _slotList;
    [SerializeField] List<Image> _slotImageList = new List<Image>();
    [SerializeField] List<TextMeshProUGUI> _slotTextList = new List<TextMeshProUGUI>();
    [SerializeField] List<ShopItem> _selectionList = new List<ShopItem>();

    [SerializeField] List<Sprite> _spriteList;
    
    public override void Init()
    {
        gameObject.SetActive(false);

        int index = 0;
        foreach (var slot in _slotList)
        {
            _slotImageList.Add(slot.transform.Find("Image").GetComponent<Image>());
            _slotTextList.Add(slot.transform.Find("Text").GetComponent<TextMeshProUGUI>());

            int tempIndex = index;
            slot.onClick.AddListener(() => {
                if (_selectionList[tempIndex].isSale) return;

                GameManager gameManager = Managers.GetManager<GameManager>();

                if (_selectionList[tempIndex].price <= gameManager.Daughter.Mental)
                {
                    gameManager.Daughter.Mental -= _selectionList[tempIndex].price;
                    ShopItem info = _selectionList[tempIndex];
                    info.isSale = true;
                    _selectionList[tempIndex] = info;

                    if (info.character.Equals("Daughter"))
                    {
                        gameManager.Player.WeaponSwaper.ChangeNewWeapon((int)info.weaponPosition, info.weaponName);
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

        _currentMoneyText.text = Managers.GetManager<GameManager>().Money.ToString();
    }

    public override void Open()
    {
        _selectionList.Clear();
        Refresh();
        gameObject.SetActive(true);
    }

    public void Open(List<ShopItem> list)
    {
        _selectionList.Clear();
        _selectionList.AddRange(list);
        Refresh();
        gameObject.SetActive(true);
    }
    public override void Close()
    {
        gameObject.SetActive(false);
    }

    void Refresh()
    {
        for (int i = 0; i < _selectionList.Count; i++)
        {
            if (!_selectionList[i].isSale)
            {
                _slotTextList[i].text = $"{_selectionList[i].weaponPosition} : {_selectionList[i].weaponName.ToString()}\n";
                _slotTextList[i].text += $"{_selectionList[i].price.ToString()}";

            }
            else
                _slotTextList[i].text = "판매완료";
        }
    }
}

[System.Serializable]
public class ShopItem
{
    public string character;
    public Define.WeaponName weaponName;
    public Define.WeaponPosition weaponPosition;
    public int price;
    public bool isSale;
}
