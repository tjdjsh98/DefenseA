using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GirlAbility 
{
    Player _player;
    Character _creature;
    Character Creature
    {
        get
        {
            if (_creature == null)
            {
                _creature = Managers.GetManager<GameManager>().Creature;
                _creature.CharacterDeadHandler += OnCreatureDead; 
            }
            return _creature;
        }
    }
    CardManager _cardManager;

    Inventory _inventory;
    // 아이템 등으로 추가된 능력치
    public float IncreasedAttackPowerPercentage { set;get; }
    public float IncreasedAttackSpeedPercentage { set;get; }
    public float IncreasedReloadSpeedPercentage { set;get; }

    // 스킬
    Dictionary<CardName, Action<SkillSlot>> _skillDictionary = new Dictionary<CardName, Action<SkillSlot>>();

    // 송곳니
    public bool IsActiveCanine { get; set; }
    public float _canineElasepdTime;
    public float _canineDurationTime = 10;
    SkillSlot _canineSlot;

    #region 아이템 능력

    // 탁한 잎
    float _cludyLeafIncreaseAttackPowerPercentage;
    // 보이지 않는 손
    List<float> _invisbleHandElaspedTimeList = new List<float>();

    // 문들어진 송곳니
    int _crumbledCanineHuntingCount = 0;

    // 오발사 탄창

    // 부서진약지
    float _brokenRingFingerIncreaseAttackPowerPercentage = 30;

    #endregion

    public void Init(Player player)
    {
        _player = player;
        _player.Character.AttackHandler += OnAttack;
        _player.Character.AddtionalAttackHandler += OnAddtionalAttack;
        _player.Character.CharacterDeadHandler += OnPlayerDead;
        _player.Character.DamagedHandler += OnDamaged;
        _inventory = Managers.GetManager<GameManager>().Inventory;
        _cardManager = Managers.GetManager<CardManager>();
        RegistSkill();
    }

    private void OnDamaged(Character attacker, int dmg, float power, Vector3 direction, Vector3 point, float stumTime)
    {
        if (_inventory.GetItemCount(ItemName.피묻은뼈목걸이) > 0)
        {
            Creature.Hp += 10 * _inventory.GetItemCount(ItemName.피묻은뼈목걸이);
        }
        if (_inventory.GetItemCount(ItemName.부서진약지) > 0)
        {
            _inventory.IsActiveBrokenRingFinger = false;
        }
    }

    private void OnPlayerDead()
    {
        Managers.GetManager<GameManager>().Player.GirlAbility.IncreasedAttackPowerPercentage -= _cludyLeafIncreaseAttackPowerPercentage;
        _cludyLeafIncreaseAttackPowerPercentage = 0;
    }

    private void OnCreatureDead()
    {
        if (_inventory.GetItemCount(ItemName.탁한잎) > 0)
        {
            Managers.GetManager<GameManager>().Player.GirlAbility.IncreasedAttackPowerPercentage += 5;
            _cludyLeafIncreaseAttackPowerPercentage += 5;
        }
    }

    public void AbilityUpdate()
    {
        InvisibleHand();
        //FastReload();
        _player.Character.IncreasedHpRegeneration = GetHpRegeneration();

        //스킬: 송곳니
        if (IsActiveCanine)
        {
            _canineElasepdTime += Time.deltaTime;   
            if (_canineElasepdTime > _canineDurationTime)
            {
                _canineElasepdTime = 0;
                IsActiveCanine = false;
                _canineSlot.skillElapsed = 0;
                _canineSlot.isActive = false;
            }
        }
    }

    
    #region 스킬 관련
    void RegistSkill()
    {
        _skillDictionary.Add(CardName.송곳니, PlayCanine);
    }

    public void UseSkill(SkillSlot skillSlot)
    {
        if (skillSlot.card == null || skillSlot.card.cardData == null) return;

        if (_skillDictionary.TryGetValue(skillSlot.card.cardData.CardName, out var func))
        {
            func?.Invoke(skillSlot);
        }
    }

    void PlayCanine(SkillSlot slot)
    {
        if (slot.isActive) return;
        if (slot.skillElapsed < slot.skillCoolTime) return;

        slot.isActive = true;
        IsActiveCanine = true;
        _canineSlot = slot;
    }

    #endregion

    void OnAttack(Character target, int totalDamage, float power, Vector3 direction, Vector3 point, float stunTime)
    {
        CardManager manager = Managers.GetManager<CardManager>();

   
        if (_inventory.GetItemCount(ItemName.작은송곳니) > 0)
        {
            if(Random.Range(0,100) < 5)
                Creature.Hp += _inventory.GetItemCount(ItemName.작은송곳니) * 10;
        }
        if (_inventory.GetItemCount(ItemName.눈동자구슬) > 0)
        {
            if (Random.Range(0, 100) < 10)
                _player.Character.AddtionalAttack(target, totalDamage, 0, Vector3.zero, point+Vector3.up*1, 0);
        }

        // 타겟이 죽는다면 
        if (target == null || target.IsDead)
        {
            if (_inventory.GetItemCount(ItemName.문들어진송곳니) > 0)
            {
                _crumbledCanineHuntingCount++;
                if (_crumbledCanineHuntingCount > 5)
                {
                    _player.Character.Hp += _inventory.GetItemCount(ItemName.문들어진송곳니);
                }
            }

            if (_inventory.GetItemCount(ItemName.니트로글리세린) > 0)
            {
                if(Random.Range(0,100) < 2)
                {
                    Define.Range range = new Define.Range() { center = Vector3.zero,size = Vector3.one*10, angle= 0 ,figureType = Define.FigureType.Circle};
                    Effect effect = Managers.GetManager<ResourceManager>().Instantiate<Effect>((int)Define.EffectName.Explosion);
                    Util.RangeCastAll2D(point, range, Define.CharacterMask, (hit) =>
                    {
                        Character character = hit.collider.GetComponent<Character>();
                        if (character != null && !character.IsDead && character.CharacterType == Define.CharacterType.Enemy)
                        {
                            _player.Character.Attack(character, 20, 500, character.transform.position - point, hit.point, 0.3f);
                        }
                        return false;
                    });
                    effect.SetProperty("Radius", 10f);
                    effect.Play(point);
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

        //아이템 : 검은유리창파편
        if (_player.WeaponSwaper.CurrentWeapon != null)
        {
            if(_inventory.GetItemCount(ItemName.검은유리창파편) > 0)
                percentage += ((int)_player.WeaponSwaper.CurrentWeapon.MaxAmmo/10) * 5 * _inventory.GetItemCount(ItemName.검은유리창파편);
        }

        // 아이템 : 마지막탄환
        if (_player.WeaponSwaper.CurrentWeapon != null)
        {
            if (_player.WeaponSwaper.CurrentWeapon.CurrentAmmo == 1)
            {
                percentage += _inventory.GetItemCount(ItemName.마지막탄환) * 100f;
            }
        }

        // 아이템 : 부서진 약지
        if (_inventory.GetItemCount(ItemName.부서진약지) > 0)
        {
            if (_inventory.IsActiveBrokenRingFinger)
            {
                percentage += _brokenRingFingerIncreaseAttackPowerPercentage * _inventory.GetItemCount(ItemName.부서진약지);
            }
        }

        return percentage;
    }

    public float GetIncreasedAttackSpeedPercentage()
    {
        Character creature = Managers.GetManager<GameManager>().Creature;
        float percentage = 0;

        percentage += IncreasedAttackSpeedPercentage;

        // 아이템: 과충전배터리
        if (_inventory.GetItemCount(ItemName.과충전배터리) > 0)
        {
            percentage += _inventory.GetItemCount(ItemName.피뢰침) * 5;
            percentage += _inventory.GetItemCount(ItemName.부서진건전지) * 5;
        }

        return percentage;
    }
    public float GetIncreasedReloadSpeedPercentage()
    {
        float percentage = 0;

        percentage += IncreasedReloadSpeedPercentage;

     
        return percentage;
    }
  

    public float GetHpRegeneration()
    {
        float regen = 0;
        regen = _player.Character.IncreasedHpRegeneration;

        return regen;
    }
}

