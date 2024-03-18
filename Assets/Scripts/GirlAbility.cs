using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class GirlAbility 
{
    Player _player;

    // 언락된 능력
    Dictionary<GirlAbilityName, bool> _abilityUnlocks = new Dictionary<GirlAbilityName, bool>();

    // 검은구체
    List<BlackSphere> _blackSphereList = new List<BlackSphere>();
    public List<BlackSphere> BlackSphereList => _blackSphereList;
    int _maxBlackSphereCount = 10;

    // 자동장전
    List<float> _autoReloadElaspedTimeList = new List<float>();

    public void Init(Player player)
    {
        _player = player;
        _player.Character.AttackHandler += OnAttack;
    }

    public void AbilityUpdate()
    {
        AutoReload();
        for (int i = _blackSphereList.Count - 1; i >= 0; i--)
        {
            if (_blackSphereList[i] == null)
                _blackSphereList.RemoveAt(i);
        }
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
            if(Random.Range(0,100) < 5)
                AddBlackSphere(target.transform.position);
        }
    }

    public void AddBlackSphere(Vector3 position)
    {
        GameObject go = Managers.GetManager<ResourceManager>().Instantiate("Prefabs/BlackSphere");
        go.transform.position = position;
        BlackSphere blackSphere = go.GetComponent<BlackSphere>();
        blackSphere.Init(_player.Character, new Vector3(-3, 5));
        _blackSphereList.Add(blackSphere);
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
            if (_abilityUnlocks.ContainsKey(girlAbilityName))
            {
                _abilityUnlocks[girlAbilityName] = true;
            }
            else
            {
                _abilityUnlocks.Add(girlAbilityName, true);
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
