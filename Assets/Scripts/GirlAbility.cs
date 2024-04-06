using DuloGames.UI.Tweens;
using System;
using System.Collections.Generic;
using UnityEngine;

public class GirlAbility 
{
    Player _player;
    Inventory _inventory;
    // ������ ������ �߰��� �ɷ�ġ
    public float IncreasedAttackPowerPercentage { set;get; }
    public float IncreasedAttackSpeedPercentage { set;get; }
    public float IncreasedReloadSpeedPercentage { set;get; }

    // ��ų
    Dictionary<CardName, Action<SkillSlot>> _skillDictionary = new Dictionary<CardName, Action<SkillSlot>>();

    #region ī�� �ɷ�
    // �ڵ�����
    List<float> _autoReloadElaspedTimeList = new List<float>();

    // ���ָ�
    Character _hungerTarget;
    int _hungerHitCount = 0;

    // �Ļ�
    SkillSlot _diningSlot;
    private bool _dining;

    #endregion

    #region ������ �ɷ�

    // ��������â����
    float _preBlackShardsOfGlassHpRegen;
    float _blackShardsOfGlassCoefficient = 0.01f;
    // Ź�� ��
    float _cludyLeafTime;
    #endregion

    public void Init(Player player)
    {
        _player = player;
        _player.Character.AttackHandler += OnAttack;
        _player.Character.AddtionalAttackHandler += OnAddtionalAttack;
        _inventory = Managers.GetManager<GameManager>().Inventory;

        RegistSkill();
    }
    public void AbilityUpdate()
    {
        AutoReload();
        _player.Character.IncreasedHpRegeneration = GetHpRegeneration();
    }

    private void HandleCloudyLeaf()
    {
        
    }

    #region ��ų ����
    void RegistSkill()
    {
        _skillDictionary.Add(CardName.�Ļ�, Dining);
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
            if (GetIsHaveAbility(CardName.�Ŀ�))
            {
                Managers.GetManager<CardManager>().Predation += 1;
            }
            if (GetIsHaveAbility(CardName.�Ļ��غ�))
            {
                Card card = Managers.GetManager<CardManager>().GetCard(CardName.�Ļ��غ�);
                if (card != null) { }
                Managers.GetManager<CardManager>().Predation += (int)card.property;
            }
        }
        if (GetIsHaveAbility(CardName.�����Ѿ�))
        {
            if(UnityEngine.Random.Range(0,100) < manager.GetCard(CardName.�����Ѿ�).property)
                Managers.GetManager<CardManager>().AddBlackSphere(target.transform.position);
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
                Managers.GetManager<CardManager>().Predation += 5;

                if (target.IsNotInstantlyDie)
                {
                    _player.Character.AddtionalAttack(_player.Character, dmg * 5, 0, Vector3.zero, target.transform.position); 
                }
                else
                {
                    _player.Character.AddtionalAttack(_player.Character, target.MaxHp, 0, Vector3.zero, target.transform.position);
                }
            }


        }
        if (GetIsHaveAbility(CardName.����))
        {
            Card card = manager.GetCard(CardName.����);
            if (card != null)
            {
                manager.CurrentElectricity += card.property;
            }
        }
        if (_dining)
        {
            if (_diningSlot != null && _diningSlot.card != null && _diningSlot.card.cardData.CardName == CardName.�Ļ�)
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

    public void RevertCardAbility(Card card)
    {

    }
    public float GetIncreasedDamagePercentage()
    {
        Character creature = Managers.GetManager<GameManager>().Creature;
        float percentage = 0;

        percentage += IncreasedAttackPowerPercentage;
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

        // ������ : ��ũ
        percentage += (int)(Managers.GetManager<CardManager>().Predation/5) * _inventory.GetItemCount(ItemName.��ũ)* 2f;

        // ������ : Ź�� ��
        

        return percentage;
    }

    public float GetIncreasedAttackSpeedPercentage()
    {
        Character creature = Managers.GetManager<GameManager>().Creature;
        float percentage = 0;

        percentage += IncreasedAttackSpeedPercentage;
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
    public float GetIncreasedReloadSpeedPercentage()
    {
        float percentage = 0;

        percentage += IncreasedReloadSpeedPercentage;

        Card card = Managers.GetManager<CardManager>().GetCard(CardName.��������);
        if (card != null)
        {
            percentage += card.property;
        }

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
        _preBlackShardsOfGlassHpRegen = _blackShardsOfGlassCoefficient * _inventory.GetItemCount(ItemName.��������â����);
        regen += _preBlackShardsOfGlassHpRegen;
        
        return regen;
    }
}
