using DuloGames.UI.Tweens;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;
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


    #region 아이템 능력

    // 검은유리창파편
    float _preBlackShardsOfGlassHpRegen;
    float _blackShardsOfGlassCoefficient = 2f;
    // 탁한 잎
    float _cludyLeafIncreaseAttackPowerPercentage = 40f;
    // 보이지 않는 손
    List<float> _invisbleHandElaspedTimeList = new List<float>();

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
        InvisibleHand();
        //FastReload();
        _player.Character.IncreasedHpRegeneration = GetHpRegeneration();
    }

    // 잠시 폐기 아이템으로 돌아옴
    void FastReload()
    {
        //if (!_cardManager.GetIsHaveAbility(CardName.빠른재장전)) return;
        //if (_player.WeaponSwaper.CurrentWeapon == null) return;

        //if (_preWeapon != _player.WeaponSwaper.CurrentWeapon)
        //{
        //    _preWeapon = _player.WeaponSwaper.CurrentWeapon;
        //    _preAmmoCount = _preWeapon.CurrentAmmo;
        //    return;
        //}

        //if(_preAmmoCount > 0 && _player.WeaponSwaper.CurrentWeapon.CurrentAmmo == 0)
        //{
        //    if(Random.Range(0,100) < _cardManager.GetCard(CardName.빠른재장전).Property)
        //        _player.WeaponSwaper.CurrentWeapon.CompleteFastReload(_player);
        //}

        //_preAmmoCount = _player.WeaponSwaper.CurrentWeapon.CurrentAmmo;

    }
    private void HandleCloudyLeaf()
    {
        
    }

    #region 스킬 관련
    void RegistSkill()
    {
        _skillDictionary.Add(CardName.부유, PlayFloating);
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

    void OnAttack(Character target, int totalDamage, float power, Vector3 direction, Vector3 point, float stunTime)
    {
        CardManager manager = Managers.GetManager<CardManager>();

        if (_inventory.GetItemCount(ItemName.어둠구체의3번째파편) > 0)
        {
            if (Random.Range(0, 100) < _inventory.GetItemCount(ItemName.어둠구체의3번째파편))
            {
                _cardManager.AddBlackSphere(point);
            }

        }

        // 타겟이 죽는다면 
        if (target == null || target.IsDead)
        {
            Managers.GetManager<CardManager>().Predation += Managers.GetManager<CardManager>().HuntingPredation;
            if (_inventory.GetItemCount(ItemName.검은눈동자구슬) > 0)
            {
                if (Random.Range(0, 100) < _inventory.GetItemCount(ItemName.검은눈동자구슬) * 5)
                {
                    _cardManager.AddBlackSphere(point);
                }
            }
        }
    }

    void OnAddtionalAttack(Character target, int totalDamage, float power, Vector3 direction, Vector3 point, float stunTime)
    {

    }

    private void InvisibleHand()
    {
        if (_inventory.GetItemCount(ItemName.보이지않는손) > 0)
        {

            for (int i = 0; i < _player.WeaponSwaper.GetWeaponCount(); i++)
            {
                if (_invisbleHandElaspedTimeList.Count <= i)
                    _invisbleHandElaspedTimeList.Add(0);
                if (_player.WeaponSwaper.WeaponIndex == i)
                {
                    _invisbleHandElaspedTimeList[i] = 0;
                    continue;
                }

                Weapon weapon = _player.WeaponSwaper.GetWeapon(i);
                if (weapon == null) continue;

                if (weapon.CurrentAmmo < weapon.MaxAmmo)
                {
                    if (_invisbleHandElaspedTimeList[i] > weapon.ReloadTime * 2)
                    {
                        Managers.GetManager<TextManager>().ShowText(_player.transform.position + Vector3.up * 5, $"{weapon.WeaponName.ToString()} 장전완료", 10, Color.green);
                        weapon.CompleteReload(false);
                        _invisbleHandElaspedTimeList[i] = 0;
                    }
                    else
                    {
                        _invisbleHandElaspedTimeList[i] += Time.deltaTime;
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

        // 잠시 폐기 아이템으로
        //// 넉넉한 총알
        //Card card = Managers.GetManager<CardManager>().GetCard(CardName.넉넉한총알);
        //if (card != null)
        //{
        //    Weapon weapon = _player.WeaponSwaper.CurrentWeapon;
        //    if (weapon != null)
        //    {
        //        percentage += card.Property * (weapon.MaxAmmo / 10);
        //    }
        //}

        //// 마지막 발악
        //if (creature.IsDead)
        //{
        //    card = Managers.GetManager<CardManager>().GetCard(CardName.마지막발악);
        //    if (card != null)
        //    {
        //        percentage += card.Property;
        //    }
        //}

        // 아이템 : 포크
        percentage += (int)(Managers.GetManager<CardManager>().Predation/20)*10* _inventory.GetItemCount(ItemName.포크);

        // 아이템 : 탁한 잎
        percentage += _inventory.CloudyLeafActiveCount * _cludyLeafIncreaseAttackPowerPercentage;

        // 아이템 : 검은 액체
        percentage += Mathf.Clamp(_cardManager.BlackSphereList.Count * 2,0,20) * _inventory.GetItemCount(ItemName.검은액체);

        // 아이템 : 마지막탄환
        if(_player.WeaponSwaper.CurrentWeapon != null)
        {
            if (_player.WeaponSwaper.CurrentWeapon.CurrentAmmo == 1)
            {
                percentage += _inventory.GetItemCount(ItemName.마지막탄환) * 100f;
            }
        }

        return percentage;
    }

    public float GetIncreasedAttackSpeedPercentage()
    {
        Character creature = Managers.GetManager<GameManager>().Creature;
        float percentage = 0;

        percentage += IncreasedAttackSpeedPercentage;

        // 잠시 폐기 아이템으로
        //// 마지막 발악
        //if (creature.IsDead)
        //{
        //    Card card = Managers.GetManager<CardManager>().GetCard(CardName.마지막발악);
        //    if (card != null)
        //    {
        //        percentage += card.Property;
        //    }
        //}

        // 아이템 : 검은유리창파편
        percentage += Mathf.Clamp(_blackShardsOfGlassCoefficient * _cardManager.BlackSphereList.Count,0,40) * _inventory.GetItemCount(ItemName.검은유리창파편);

        return percentage;
    }
    public float GetIncreasedReloadSpeedPercentage()
    {
        float percentage = 0;

        percentage += IncreasedReloadSpeedPercentage;

        // 아이템 : 검은 가루
        percentage += Mathf.Clamp(2* _cardManager.BlackSphereList.Count, 0, 20) * _inventory.GetItemCount(ItemName.검은가루);
        return percentage;
    }
  

    public float GetHpRegeneration()
    {
        float regen = 0;
        regen = _player.Character.IncreasedHpRegeneration;

        return regen;
    }
}

