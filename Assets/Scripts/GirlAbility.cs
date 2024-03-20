using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class GirlAbility 
{
    Player _player;

    // 언락된 능력
    Dictionary<GirlAbilityName, bool> _abilityUnlocks = new Dictionary<GirlAbilityName, bool>();
    
    // 자동장전
    List<float> _autoReloadElaspedTimeList = new List<float>();

    // 굶주림
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
        if (GetIsHaveAbility(GirlAbilityName.BlackBullet))
        {
            if(UnityEngine.Random.Range(0,100) < 5)
                Managers.GetManager<AbilityManager>().AddBlackSphere(target.transform.position);
        }

        if (GetIsHaveAbility(GirlAbilityName.MealPreparation))
        {
            if(target == null || target.IsDead)
            {
                Managers.GetManager<AbilityManager>().AddPredation(1);
            }
        }
        if (GetIsHaveAbility(GirlAbilityName.Hunger))
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

            if (_hungerHitCount >= 10)
            {
                _hungerTarget = null;
                _hungerHitCount = 0;
                Managers.GetManager<AbilityManager>().AddPredation(5);

                target.Damage(_player.Character, target.MaxHp, 0, Vector3.zero, 0);
            }


        }
    }
    private void AutoReload()
    {
        if (GetIsHaveAbility(GirlAbilityName.AutoReload))
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
                    if (_autoReloadElaspedTimeList[i] > weapon.ReloadDelay)
                    {
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
    public bool GetIsHaveAbility(GirlAbilityName girlAbility)
    {
        if (_abilityUnlocks.TryGetValue(girlAbility, out bool value) && value)
            return value;

        return false;
    }

    public void AddGirlAbility(GirlAbilityName girlAbilityName)
    {
        if ((int)girlAbilityName > (int)GirlAbilityName.None && (int)girlAbilityName < (int)GirlAbilityName.END)
        {
            bool turnTrue = false;

            if (_abilityUnlocks.ContainsKey(girlAbilityName))
            {
                turnTrue = true;    
                _abilityUnlocks[girlAbilityName] = true;
            }
            else
            {
                turnTrue = true;    
                _abilityUnlocks.Add(girlAbilityName, true);
            }

            if(turnTrue)
            {
                switch (girlAbilityName)
                {
                   
                }
            }
        }
    }

    public float GetIncreasedDamagePercentage()
    {
        Character wall = Managers.GetManager<GameManager>().Wall;
        Character creature = Managers.GetManager<GameManager>().Creature;

        float percentage = 0;

        //if (AbilityUnlocks.TryGetValue(GirlAbilityName.LastStruggle, out bool value) && value)
        //{
        //    if ((wall == null || wall.IsDead) && (creature == null || creature.IsDead))
        //        percentage += 100f;
        //}

        return percentage;
    }

    public float GetIncreasedAttackSpeedPercentage()
    {
        Character wall = Managers.GetManager<GameManager>().Wall;
        Character creature = Managers.GetManager<GameManager>().Creature;

        float percentage = 0;

        //if (AbilityUnlocks.TryGetValue(GirlAbilityName.LastStruggle, out bool value) && value)
        //{
        //    if ((wall == null || wall.IsDead) && (creature == null || creature.IsDead))
        //        percentage += 100f;
        //}

        return percentage;
    }
}
