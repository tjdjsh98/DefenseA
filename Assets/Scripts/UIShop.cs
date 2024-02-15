using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIShop : UIBase
{
    [SerializeField]TextMeshProUGUI _currentMoneyText;

    [SerializeField] List<Button> _slotList;
    [SerializeField] List<Image> _slotImageList = new List<Image>();
    [SerializeField] List<TextMeshProUGUI> _slotTextList = new List<TextMeshProUGUI>();
    [SerializeField] List<SelectionInfo> _selectionList = new List<SelectionInfo>();

    [SerializeField] List<Sprite> _spriteList;
    
    public override void Init()
    {
        gameObject.SetActive(false);

        int index = 0;
        foreach (var slot in _slotList)
        {
            _slotImageList.Add(slot.transform.Find("Image").GetComponent<Image>());
            _slotTextList.Add(slot.transform.Find("Text").GetComponent<TextMeshProUGUI>());
            _selectionList.Add(new SelectionInfo());

            int tempIndex = index;
            slot.onClick.AddListener(() => {
                if (_selectionList[tempIndex].isSale) return;

                GameManager gameManager = Managers.GetManager<GameManager>();

                if (_selectionList[tempIndex].price <= gameManager.Money)
                {
                    gameManager.Money -= _selectionList[tempIndex].price;
                    SelectionInfo info = _selectionList[tempIndex];
                    info.isSale = true;
                    _selectionList[tempIndex] = info;

                    if (_selectionList[tempIndex].index == 0)
                    {
                        gameManager.Player.WeaponSwaper.CurrentWeapon.SetDamage(gameManager.Player.WeaponSwaper.CurrentWeapon.Damage + 1);
                    }
                    if (_selectionList[tempIndex].index == 1)
                    {
                        gameManager.Player.WeaponSwaper.CurrentWeapon.SetPower(gameManager.Player.WeaponSwaper.CurrentWeapon.Power + 100);
                    }
                    if (_selectionList[tempIndex].index == 2)
                    {
                        gameManager.Player.WeaponSwaper.CurrentWeapon.DecreaseReloadDelay(0.1f);
                    }
                    if (_selectionList[tempIndex].index == 3)
                    {
                        gameManager.Player.WeaponSwaper.CurrentWeapon.IncreaseMaxAmmo(3);
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
        for(int i =0; i < _selectionList.Count; i++)
        {
            _selectionList[i] = new SelectionInfo { index = Random.Range(0,4), price= Random.Range(5,20), isSale =false};
        }
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
            _slotImageList[i].sprite = _spriteList[_selectionList[i].index];
            if (!_selectionList[i].isSale)
            {

                _slotTextList[i].text = $"{_selectionList[i].price.ToString()}\\ \n";
                string addDescription="";
                if (_selectionList[i].index == 0)
                {
                    addDescription = "무기 공격력을 1 증가";
                }
                if (_selectionList[i].index == 1)
                {
                    addDescription = "무기 밀치는 힘 100 증가";
                }
                if (_selectionList[i].index == 2)
                {
                    addDescription = "재장전 딜레이 0.1초 감소";
                }
                if (_selectionList[i].index == 3)
                {
                    addDescription = "무기 탄창 수 3 증가";
                }

                _slotTextList[i].text += addDescription;

            }
            else
                _slotTextList[i].text = "판매완료";
        }
    }

}

public struct SelectionInfo
{
    public int index;
    public int price;
    public bool isSale;
}
