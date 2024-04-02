using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class GirlAbility 
{
    Player _player;

    // �ڵ�����
    List<float> _autoReloadElaspedTimeList = new List<float>();

    // ���ָ�
    Character _hungerTarget;
    int _hungerHitCount = 0;

    public void Init(Player player)
    {
        _player = player;
        _player.Character.AttackHandler += OnAttack;
    }
    public void AbilityUpdate()
    {
        AutoReload();
    }

    void PlentyOfBullets()
    {
        //if (_weaponSwaper.CurrentWeapon)
        //{
        //    if (GetIsHaveAbility(GirlAbilityName.PlentyOfBullets))
        //    {
        //        int amount = _weaponSwaper.CurrentWeapon.MaxAmmo / 10;
        //        _plentyOfBulletsIncreasedAttackPointPercentage = amount * 10f;
        //    }
        //}
    }

    void OnAttack(Character target, int dmg)
    {
        CardManager manager = Managers.GetManager<CardManager>();
        if (GetIsHaveAbility(CardName.�����Ѿ�))
        {
            if(UnityEngine.Random.Range(0,100) < manager.GetCard(CardName.�����Ѿ�).property)
                Managers.GetManager<CardManager>().AddBlackSphere(target.transform.position);
        }
        if (GetIsHaveAbility(CardName.�Ŀ�))
        {
            if (target == null || target.IsDead)
            {
                Managers.GetManager<CardManager>().AddPredation(1);
            }
        }
        if (GetIsHaveAbility(CardName.�Ļ��غ�))
        {
            if(target == null || target.IsDead)
            {
                Card card = Managers.GetManager<CardManager>().GetCard(CardName.�Ļ��غ�);
                if(card != null) { }
                    Managers.GetManager<CardManager>().AddPredation((int)card.property);
            }
        }
        if (GetIsHaveAbility(CardName.���ָ�))
        {
            if (_hungerTarget == null)
            {
                _hungerTarget = target;
                _hungerHitCount = 1;
            }
            else
            {
                if (_hungerTarget != target)
                {
                    _hungerTarget = target;
                    _hungerHitCount = 1;
                }
                else
                {
                    _hungerHitCount++;
                }
            }

            if (_hungerHitCount >= (int)Managers.GetManager<CardManager>().GetCard(CardName.���ָ�).property)
            {
                _hungerTarget = null;
                _hungerHitCount = 0;
                Managers.GetManager<CardManager>().AddPredation(5);

                if (target.IsNotInstantlyDie)
                {
                    target.Damage(_player.Character, dmg*5, 0, Vector3.zero, target.transform.position);
                }
                else
                {
                    target.Damage(_player.Character, target.MaxHp, 0, Vector3.zero, target.transform.position);
                }
            }


        }
        if (GetIsHaveAbility(CardName.����))
        {
            Card card = manager.GetCard(CardName.����);
            if (card != null)
            {
                manager.AddElectricity(card.property);
            }
        }
    }
    private void AutoReload()
    {
        if (GetIsHaveAbility(CardName.�ڵ�����))
        {

            for (int i = 0; i < _player.WeaponSwaper.GetWeaponCount(); i++)
            {
                if (_autoReloadElaspedTimeList.Count <= i)
                    _autoReloadElaspedTimeList.Add(0);
                if (_player.WeaponSwaper.WeaponIndex == i)
                {
                    _autoReloadElaspedTimeList[i] = 0;
                    continue;
                }

                Weapon weapon = _player.WeaponSwaper.GetWeapon(i);
                if (weapon == null) continue;

                if (weapon.CurrentAmmo < weapon.MaxAmmo)
                {
                    if (_autoReloadElaspedTimeList[i] > weapon.ReloadTime)
                    {
                        Managers.GetManager<TextManager>().ShowText(_player.transform.position + Vector3.up * 5,$"{weapon.WeaponName.ToString()} �����Ϸ�" , 10, Color.green);
                        weapon.CompleteReload();
                        _autoReloadElaspedTimeList[i] = 0;
                    }
                    else
                    {
                        _autoReloadElaspedTimeList[i] += Time.deltaTime;
                    }
                }
            }
        }
    }
    public bool GetIsHaveAbility(CardName cardName)
    {
        Card card = Managers.GetManager<CardManager>().GetCard(cardName);
        
        return card != null;
    }

    // �߰� �� �� ����Ǵ� �ɷ�ġ �ݿ�
    public void ApplyCardAbility(Card card)
    {
        if (card != null && card.cardData != null)
        {
            switch (card.cardData.CardName)
            {
                   
            }
        }
    }

    public float GetIncreasedDamagePercentage()
    {
        Character creature = Managers.GetManager<GameManager>().Creature;
        float percentage = 0;

        // �˳��� �Ѿ�
        Card card = Managers.GetManager<CardManager>().GetCard(CardName.�˳����Ѿ�);
        if (card != null)
        {
            Weapon weapon = _player.WeaponSwaper.CurrentWeapon;
            if (weapon != null)
            {
                percentage += card.property * (weapon.MaxAmmo / 10);
            }
        }

        // ������ �߾�
        if (creature.IsDead)
        {
            card = Managers.GetManager<CardManager>().GetCard(CardName.�������߾�);
            if (card != null)
            {
                percentage += card.property;
            }
        }

        return percentage;
    }

    public float GetIncreasedAttackSpeedPercentage()
    {
        Character creature = Managers.GetManager<GameManager>().Creature;
        float percentage = 0;

        // ������ �߾�
        if (creature.IsDead)
        {
            Card card = Managers.GetManager<CardManager>().GetCard(CardName.�������߾�);
            if (card != null)
            {
                percentage += card.property;
            }
        }


        return percentage;
    }
}
