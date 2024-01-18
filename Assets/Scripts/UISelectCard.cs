using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UISelectCard : UIBase
{
    [SerializeField] List<TextMeshProUGUI> _textList;
    public override void Init()
    { 
        Close();
        _isInitDone = true;

    }
    public override void Open()
    {
        gameObject.SetActive(true);
    }

    public override void Close()
    {
        gameObject.SetActive(false);
    }


    public void SelectCard(int index)
    {
        Player player = Managers.GetManager<GameManager>().Player;
        Character building = Managers.GetManager<GameManager>().Building;
        if (index == 0)
        {
            player.WeaponSwaper.CurrentWeapon.SetDamage(player.WeaponSwaper.CurrentWeapon.Damage + 1);
        }
        if (index == 1)
        {
            player.WeaponSwaper.CurrentWeapon.SetPower(player.WeaponSwaper.CurrentWeapon.Power + 1);
        }
        if(index == 2)
        {
            building.SetHp(building.MaxHp);
        }

        Close();
    }
}
