using DuloGames.UI.Tweens;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using Random = UnityEngine.Random;

public class GirlAbility 
{
    Player _player;
    CardManager _cardManager;

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

  
    //����������
    Weapon _preWeapon;
    int _preAmmoCount;
    #endregion

    #region ������ �ɷ�

    // ��������â����
    float _preBlackShardsOfGlassHpRegen;
    float _blackShardsOfGlassCoefficient = 2f;
    // Ź�� ��
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
        //FastReload();
        _player.Character.IncreasedHpRegeneration = GetHpRegeneration();
    }

    // ��� ��� ���������� ���ƿ�
    void FastReload()
    {
        //if (!_cardManager.GetIsHaveAbility(CardName.����������)) return;
        //if (_player.WeaponSwaper.CurrentWeapon == null) return;

        //if (_preWeapon != _player.WeaponSwaper.CurrentWeapon)
        //{
        //    _preWeapon = _player.WeaponSwaper.CurrentWeapon;
        //    _preAmmoCount = _preWeapon.CurrentAmmo;
        //    return;
        //}

        //if(_preAmmoCount > 0 && _player.WeaponSwaper.CurrentWeapon.CurrentAmmo == 0)
        //{
        //    if(Random.Range(0,100) < _cardManager.GetCard(CardName.����������).Property)
        //        _player.WeaponSwaper.CurrentWeapon.CompleteFastReload(_player);
        //}

        //_preAmmoCount = _player.WeaponSwaper.CurrentWeapon.CurrentAmmo;

    }
    private void HandleCloudyLeaf()
    {
        
    }

    #region ��ų ����
    void RegistSkill()
    {
        _skillDictionary.Add(CardName.����, PlayFloating);
    }

    public void UseSkill(SkillSlot skillSlot)
    {
        if (skillSlot.card == null || skillSlot.card.cardData == null) return;

        if (_skillDictionary.TryGetValue(skillSlot.card.cardData.CardName, out var func))
        {
            func?.Invoke(skillSlot);
        }
    }

    void PlayFloating(SkillSlot slot)
    {
        if (slot.isActive) return;
        if (slot.skillCoolTime > slot.skillElapsed) return;

        slot.isActive = true;
        _player.StartCoroutine(CorFloating(slot));
    }

    IEnumerator CorFloating(SkillSlot slot)
    {
        float elasedTime = 0;

        _player.Character.ChangeEnableFly(true);
        while (elasedTime < 3)
        {
            elasedTime += Time.deltaTime;
            yield return null;
        }

        _player.Character.ChangeEnableFly(false);

        slot.isActive = false;
        slot.skillElapsed = 0;
    }
    #endregion

    void OnAttack(Character target, int dmg)
    {
        CardManager manager = Managers.GetManager<CardManager>();

        // Ÿ���� �״´ٸ� 
        if (target == null || target.IsDead)
        {
            Managers.GetManager<CardManager>().Predation += Managers.GetManager<CardManager>().HuntingPredation;
        }
    }

    void OnAddtionalAttack(Character target, int dmg)
    {

    }

    // ��� ��� ���������� 
    private void AutoReload()
    {
        //if (GetIsHaveAbility(CardName.�ڵ�����))
        //{

        //    for (int i = 0; i < _player.WeaponSwaper.GetWeaponCount(); i++)
        //    {
        //        if (_autoReloadElaspedTimeList.Count <= i)
        //            _autoReloadElaspedTimeList.Add(0);
        //        if (_player.WeaponSwaper.WeaponIndex == i)
        //        {
        //            _autoReloadElaspedTimeList[i] = 0;
        //            continue;
        //        }

        //        Weapon weapon = _player.WeaponSwaper.GetWeapon(i);
        //        if (weapon == null) continue;

        //        if (weapon.CurrentAmmo < weapon.MaxAmmo)
        //        {
        //            if (_autoReloadElaspedTimeList[i] > weapon.ReloadTime*2)
        //            {
        //                Managers.GetManager<TextManager>().ShowText(_player.transform.position + Vector3.up * 5,$"{weapon.WeaponName.ToString()} �����Ϸ�" , 10, Color.green);
        //                weapon.CompleteReload(false);
        //                _autoReloadElaspedTimeList[i] = 0;
        //            }
        //            else
        //            {
        //                _autoReloadElaspedTimeList[i] += Time.deltaTime;
        //            }
        //        }
        //    }
        //}
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
    public float GetIncreasedAttackPowerPercentage()
    {
        Character creature = Managers.GetManager<GameManager>().Creature;
        float percentage = 0;

        percentage += IncreasedAttackPowerPercentage;

        // ��� ��� ����������
        //// �˳��� �Ѿ�
        //Card card = Managers.GetManager<CardManager>().GetCard(CardName.�˳����Ѿ�);
        //if (card != null)
        //{
        //    Weapon weapon = _player.WeaponSwaper.CurrentWeapon;
        //    if (weapon != null)
        //    {
        //        percentage += card.Property * (weapon.MaxAmmo / 10);
        //    }
        //}

        //// ������ �߾�
        //if (creature.IsDead)
        //{
        //    card = Managers.GetManager<CardManager>().GetCard(CardName.�������߾�);
        //    if (card != null)
        //    {
        //        percentage += card.Property;
        //    }
        //}

        // ������ : ��ũ
        percentage += (int)Mathf.Clamp((Managers.GetManager<CardManager>().Predation/5),0,40)*5* _inventory.GetItemCount(ItemName.��ũ);

        // ������ : Ź�� ��
        percentage += _inventory.CloudyLeafActiveCount * _cludyLeafIncreaseAttackPowerPercentage;

        // ������ : ���� ��ü
        percentage += Mathf.Clamp(_cardManager.BlackSphereList.Count * 2,0,20) * _inventory.GetItemCount(ItemName.������ü);


        return percentage;
    }

    public float GetIncreasedAttackSpeedPercentage()
    {
        Character creature = Managers.GetManager<GameManager>().Creature;
        float percentage = 0;

        percentage += IncreasedAttackSpeedPercentage;

        // ��� ��� ����������
        //// ������ �߾�
        //if (creature.IsDead)
        //{
        //    Card card = Managers.GetManager<CardManager>().GetCard(CardName.�������߾�);
        //    if (card != null)
        //    {
        //        percentage += card.Property;
        //    }
        //}

        // ������ : ��������â����
        percentage += Mathf.Clamp(_blackShardsOfGlassCoefficient * _cardManager.BlackSphereList.Count,0,40) * _inventory.GetItemCount(ItemName.��������â����);

        return percentage;
    }
    public float GetIncreasedReloadSpeedPercentage()
    {
        float percentage = 0;

        percentage += IncreasedReloadSpeedPercentage;

        // ������ : ���� ����
        percentage += Mathf.Clamp(2* _cardManager.BlackSphereList.Count, 0, 20) * _inventory.GetItemCount(ItemName.��������);
        return percentage;
    }
  

    public float GetHpRegeneration()
    {
        float regen = 0;
        regen = _player.Character.IncreasedHpRegeneration;

        return regen;
    }
}

