using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UICardSelection : UIBase
{
    [SerializeField] List<TextMeshProUGUI> _cardText;
    List<CardSelectionInfo> _cardInfoList = new List<CardSelectionInfo>();

    public override void Init()
    {
        Close();
        _isInitDone = true;
    }
    public override void Open()
    {
        Time.timeScale = 0;
        Managers.GetManager<GameManager>().IsStopWave = true;
        _cardInfoList.Clear();
        _cardInfoList.Add(Managers.GetManager<GameManager>().CardSelectionInfoList.GetRandom());
        _cardInfoList.Add(Managers.GetManager<GameManager>().CardSelectionInfoList.GetRandom());
        _cardInfoList.Add(Managers.GetManager<GameManager>().CardSelectionInfoList.GetRandom());
        Refresh();

        gameObject.SetActive(true);
    }

    public override void Close()
    {
        Time.timeScale = 1;
        gameObject.SetActive(false);
        Managers.GetManager<GameManager>().IsStopWave = false;
    }

    void Refresh()
    {
        for(int i =0; i < _cardText.Count; i++)
        {
            _cardText[i].text = _cardInfoList[i].selectionName;
        }
    }

    public void SelectCard(int cardIndex)
    {
        switch (_cardInfoList[cardIndex].selectionName)
        {
            case "무기 데미지 증가":
                Managers.GetManager<GameManager>().Player.WeaponSwaper.CurrentWeapon.SetDamage(Managers.GetManager<GameManager>().Player.WeaponSwaper.CurrentWeapon.Damage + 1);
                break;
            case "장전 탄약 증가":
                Managers.GetManager<GameManager>().Player.WeaponSwaper.CurrentWeapon.IncreaseMaxAmmo(2);
                break;
            case "재장전 속도 증가":
                Managers.GetManager<GameManager>().Player.WeaponSwaper.CurrentWeapon.DecreaseReloadDelay(0.1f);
                break;
            case "체력 회복":
                Managers.GetManager<GameManager>().Player.Character.SetHp(Managers.GetManager<GameManager>().Player.Character.MaxHp);
                break;
            case "스피어 해제":
                Managers.GetManager<GameManager>().Familiar.IsUnlockSpear = true;
                break;
        }
        Close();
    }
}

[System.Serializable]
public class CardSelectionInfo
{
    public string selectionName;
}
