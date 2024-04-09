using DuloGames.UI.Tweens;
using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GirlAbility 
{
    Player _player;
    CardManager _cardManager;

    Inventory _inventory;
    // 아이템 등으로 추가된 능력치
    public float IncreasedAttackPowerPercentage { set;get; }
    public float IncreasedAttackSpeedPercentage { set;get; }
    public float IncreasedReloadSpeedPercentage { set;get; }

    // 스킬
    Dictionary<CardName, Action<SkillSlot>> _skillDictionary = new Dictionary<CardName, Action<SkillSlot>>();

    #region 카드 능력
    // 자동장전
    List<float> _autoReloadElaspedTimeList = new List<float>();

    // 굶주림
    Character _hungerTarget;
    int _hungerHitCount = 0;

    // 식사
    SkillSlot _diningSlot;
    private bool _dining;


    //빠른재장전
    Weapon _preWeapon;
    int _preAmmoCount;
    #endregion

    #region 아이템 능력

    // 검은유리창파편
    float _preBlackShardsOfGlassHpRegen;
    float _blackShardsOfGlassCoefficient = 0.01f;
    // 탁한 잎
    float _cludyLeafIncreaseAttackPowerPercentage = 20f;
    #endregion

    public void Init(Player player)
    {
        _player = player;
        _player.Character.AttackHandler += OnAttack;
        _player.Character.AddtionalAttackHandler += OnAddtionalAttack;
        _inventory = Managers.GetManager<GameManager>().Inventory;
        _cardManager = Managers.GetManager<CardManager>();
        RegistSkill();
    }
    public void AbilityUpdate()
    {
        AutoReload();
        FastReload();
        _player.Character.IncreasedHpRegeneration = GetHpRegeneration();
    }

    void FastReload()
    {
        if (!_cardManager.GetIsHaveAbility(CardName.빠른재장전)) return;
        if (_player.WeaponSwaper.CurrentWeapon == null) return;

        if (_preWeapon != _player.WeaponSwaper.CurrentWeapon)
        {
            _preWeapon = _player.WeaponSwaper.CurrentWeapon;
            _preAmmoCount = _preWeapon.CurrentAmmo;
            return;
        }

        if(_preAmmoCount > 0 && _player.WeaponSwaper.CurrentWeapon.CurrentAmmo == 0)
        {
            if(Random.Range(0,100) < _cardManager.GetCard(CardName.빠른재장전).Property)
                _player.WeaponSwaper.CurrentWeapon.CompleteFastReload(_player);
        }

        _preAmmoCount = _player.WeaponSwaper.CurrentWeapon.CurrentAmmo;

    }
    private void HandleCloudyLeaf()
    {
        
    }

    #region 스킬 관련
    void RegistSkill()
    {
        _skillDictionary.Add(CardName.식사, Dining);
    }

    public void UseSkill(SkillSlot skillSlot)
    {
        if (skillSlot.card == null || skillSlot.card.cardData == null) return;

        if (_skillDictionary.TryGetValue(skillSlot.card.cardData.CardName, out var func))
        {
            func?.Invoke(skillSlot);
        }
    }

    #endregion

    void OnAttack(Character target, int dmg)
    {
        CardManager manager = Managers.GetManager<CardManager>();

        if (target == null || target.IsDead)
        {
            Managers.GetManager<CardManager>().Predation += Managers.GetManager<CardManager>().HuntingPredation;
        }
        if (GetIsHaveAbility(CardName.굶주림))
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

            if (_hungerHitCount >= (int)Managers.GetManager<CardManager>().GetCard(CardName.굶주림).Property)
            {
                _hungerTarget = null;
                _hungerHitCount = 0;
                Managers.GetManager<CardManager>().Predation += 5;

                if (target.IsNotInstantlyDie)
                {
                    _player.Character.AddtionalAttack(target, dmg * 5, 0, Vector3.zero, target.transform.position); 
                }
                else
                {
                    _player.Character.AddtionalAttack(target, target.MaxHp, 0, Vector3.zero, target.transform.position);
                }
            }


        }
        if (GetIsHaveAbility(CardName.충전))
        {
            Card card = manager.GetCard(CardName.충전);
            if (card != null)
            {
                manager.CurrentElectricity += card.Property;
            }
        }
        if (_dining)
        {
            if (_diningSlot != null && _diningSlot.card != null && _diningSlot.card.cardData.CardName == CardName.식사)
            {
                target.Damage(_player.Character, target.MaxHp, 0, Vector3.zero, target.transform.position, 0);
                _diningSlot.skillTime = 0;
                _diningSlot.isActive = false;
            }
            _dining = false;
            _diningSlot = null;
        }
    }

    void OnAddtionalAttack(Character target, int dmg)
    {

    }
    private void AutoReload()
    {
        if (GetIsHaveAbility(CardName.자동장전))
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
                    if (_autoReloadElaspedTimeList[i] > weapon.ReloadTime*2)
                    {
                        Managers.GetManager<TextManager>().ShowText(_player.transform.position + Vector3.up * 5,$"{weapon.WeaponName.ToString()} 장전완료" , 10, Color.green);
                        weapon.CompleteReload(false);
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

    // 추가 될 떄 변경되는 능력치 반영
    public void ApplyCardAbility(Card card)
    {
        if (card != null && card.cardData != null)
        {
            switch (card.cardData.CardName)
            {
                   
            }
        }
    }

    public void RevertCardAbility(Card card)
    {

    }
    public float GetIncreasedAttackPowerPercentage()
    {
        Character creature = Managers.GetManager<GameManager>().Creature;
        float percentage = 0;

        percentage += IncreasedAttackPowerPercentage;
        // 넉넉한 총알
        Card card = Managers.GetManager<CardManager>().GetCard(CardName.넉넉한총알);
        if (card != null)
        {
            Weapon weapon = _player.WeaponSwaper.CurrentWeapon;
            if (weapon != null)
            {
                percentage += card.Property * (weapon.MaxAmmo / 10);
            }
        }

        // 마지막 발악
        if (creature.IsDead)
        {
            card = Managers.GetManager<CardManager>().GetCard(CardName.마지막발악);
            if (card != null)
            {
                percentage += card.Property;
            }
        }

        // 아이템 : 포크
        percentage += (int)(Managers.GetManager<CardManager>().Predation/10) * _inventory.GetItemCount(ItemName.포크)* 3f;

        // 아이템 : 탁한 잎
        percentage += _inventory.CloudyLeafActiveCount * _cludyLeafIncreaseAttackPowerPercentage;

        return percentage;
    }

    public float GetIncreasedAttackSpeedPercentage()
    {
        Character creature = Managers.GetManager<GameManager>().Creature;
        float percentage = 0;

        percentage += IncreasedAttackSpeedPercentage;
        // 마지막 발악
        if (creature.IsDead)
        {
            Card card = Managers.GetManager<CardManager>().GetCard(CardName.마지막발악);
            if (card != null)
            {
                percentage += card.Property;
            }
        }


        return percentage;
    }
    public float GetIncreasedReloadSpeedPercentage()
    {
        float percentage = 0;

        percentage += IncreasedReloadSpeedPercentage;

      
        return percentage;
    }
    public void Dining(SkillSlot slot)
    {
        if (slot.isActive) return;
        if (slot.skillCoolTime > slot.skillTime) return;
        if (Managers.GetManager<CardManager>().Predation < 20) return;

        Managers.GetManager<CardManager>().Predation -= 20;
        slot.isActive = true;
        _dining = true;
        _diningSlot = slot;
    }

    public float GetHpRegeneration()
    {
        float regen = 0;
        regen = _player.Character.IncreasedHpRegeneration;

        regen -= _preBlackShardsOfGlassHpRegen;
        _preBlackShardsOfGlassHpRegen = _blackShardsOfGlassCoefficient * _inventory.GetItemCount(ItemName.검은유리창파편);
        regen += _preBlackShardsOfGlassHpRegen;
        
        return regen;
    }
}

