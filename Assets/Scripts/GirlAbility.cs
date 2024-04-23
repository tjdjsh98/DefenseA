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
    CreatureAI _creatureAI;
    CreatureAI CreatureAI
    {
        get
        {
            if (_creatureAI == null)
            {
                _creatureAI = Managers.GetManager<GameManager>().CreatureAI;
            }
            return _creatureAI;
        }
    }
    CardManager _cardManager;

    Inventory _inventory;
    // 아이템 등으로 추가된 능력치
    public float IncreasedAttackPowerPercentage { set; get; }
    public float IncreasedAttackSpeedPercentage { set; get; }
    public float IncreasedReloadSpeedPercentage { set; get; }


    float _fireTime = 0;

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
    int _cludyLeafIncreaseMaxHp;

    // 보이지 않는 손
    List<float> _invisbleHandElaspedTimeList = new List<float>();

    // 문들어진 송곳니
    int _crumbledCanineHuntingCount = 0;

    // 오발사 탄창

    // 부서진약지
    float _brokenRingFingerIncreaseAttackPowerPercentage = 50;

    // 피뢰침_B
    bool _lightingRod_B_Active = false;

    // 망각의 서
    float _bookOfOblivionLoseAttackPowerPercentage = 0;

    // 피묻은뼈목걸이_B
    int _bloodyBoneNecklessIncreasedAttackPoint = 0;

    // 작은송곳니_B
    int _littleCanineLostHp = 0;
    float _littleCanineTime = 0;

    public bool LightingRod_B_Active
    {
        set {
            _lightingRod_B_ElasepdTime = 0;
            _lightingRod_B_Active = value; }
        get => _lightingRod_B_Active;
    }
    float _lightingRod_B_DurationTime = 5;
    float _lightingRod_B_ElasepdTime;
    float _lightingRod_B_Coefficient = 100f;

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
        // 아이템 : 피묻은뼈목걸이
        {
            int hpAmount = 0;
            if (_inventory.GetIsHaveItem(ItemName.피묻은뼈목걸이))
            {
                hpAmount = 30;
            }
            if (_inventory.GetIsHaveItem(ItemName.피묻은뼈목걸이_A))
            {
                hpAmount = 70;
            }
            if (_inventory.GetIsHaveItem(ItemName.피묻은뼈목걸이_B))
            {
                hpAmount = 30;
                Creature.AttackPower += 1;
                _bloodyBoneNecklessIncreasedAttackPoint += 1;
            }
            Creature.Hp += hpAmount;
        }

        //아이템: 망각의 서
        {
            if (_inventory.GetIsHaveItem(ItemName.망각의서))
            {
                _bookOfOblivionLoseAttackPowerPercentage += 5;
            }
            else if (_inventory.GetIsHaveItem(ItemName.망각의서_A))
            {
                _bookOfOblivionLoseAttackPowerPercentage += 2;
            }
            else if (_inventory.GetIsHaveItem(ItemName.망각의서_B))
            {
                _bookOfOblivionLoseAttackPowerPercentage += 10;
            }
            else
            {
                _bookOfOblivionLoseAttackPowerPercentage = 0;
            }
        }
        // 아이템: 에너지바_B
        if (_inventory.GetIsHaveItem(ItemName.에너지바_B))
        {
            if (Random.Range(0, 100) < 50)
            {
                _player.Character.IgnoreDamageCount++;
            }
        }
        // 아이템: 부서진 약지
        if (dmg > 0 && _inventory.IsActiveBrokenRingFinger)
            _inventory.IsActiveBrokenRingFinger = false;
    }

    private void OnPlayerDead()
    {
    }

    private void OnCreatureDead()
    {
        //탁한잎
        {
            float attackPercentage = 0;
            int maxHp = 0;
            if (_inventory.GetIsHaveItem(ItemName.탁한잎_A))
            {
                attackPercentage = 15;
            }
            if (_inventory.GetIsHaveItem(ItemName.탁한잎_B))
            {
                attackPercentage = 8;
                maxHp = 10;
            }
            if (_inventory.GetIsHaveItem(ItemName.탁한잎))
            {
                attackPercentage = 8;
            }
            _cludyLeafIncreaseAttackPowerPercentage += attackPercentage;
            _cludyLeafIncreaseMaxHp += maxHp;
            _player.Character.AddMaxHp(maxHp);
        }
    }

    public void AbilityUpdate()
    {
        if (_creature == null)
            _creature = Creature;
        // 얼마나 올래 발사 했는지 체크
        if (_player.IsFire && _player.WeaponSwaper.CurrentWeapon && !_player.WeaponSwaper.CurrentWeapon.IsReload)
            _fireTime += Time.deltaTime;
        else
            _fireTime = 0;


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

        //피뢰침_B
        if (LightingRod_B_Active)
        {
            _lightingRod_B_ElasepdTime += Time.deltaTime;
            if (_lightingRod_B_ElasepdTime > _lightingRod_B_DurationTime)
            {
                _lightingRod_B_ElasepdTime = 0;
                LightingRod_B_Active = false;
            }
        }
        // 탁한잎
        if (_cludyLeafIncreaseMaxHp != 0 || _cludyLeafIncreaseAttackPowerPercentage != 0) {
            if (!_inventory.GetIsHaveItem(ItemName.탁한잎) && !_inventory.GetIsHaveItem(ItemName.탁한잎_A) && !_inventory.GetIsHaveItem(ItemName.탁한잎_B))
            {
                _player.Character.RemoveaxHp(_cludyLeafIncreaseMaxHp);
                _cludyLeafIncreaseMaxHp = 0;
                _cludyLeafIncreaseAttackPowerPercentage = 0;
            }
        }

        //피묻은뼈목걸이_B
        if (_bloodyBoneNecklessIncreasedAttackPoint != 0)
        {
            if (!_inventory.GetIsHaveItem(ItemName.피묻은뼈목걸이_B))
            {
                Creature.AttackPower -= _bloodyBoneNecklessIncreasedAttackPoint;
                _bloodyBoneNecklessIncreasedAttackPoint = 0;
            }
        }

        // 작은 송곳니_B
        if (_inventory.GetIsHaveItem(ItemName.작은송곳니_B))
        {
            _littleCanineTime += Time.deltaTime;
            if (_fireTime != 0 && _littleCanineTime > 1)
            {
                _littleCanineTime = 0;
                _player.Character.Attack(_player.Character, 1, 0, Vector3.zero, _player.transform.position + Vector3.up*2, 0);
                _littleCanineLostHp += 1;
            }
            else if(_fireTime == 0&& _littleCanineLostHp > 0)
            {
                _player.Character.Hp += _littleCanineLostHp;
                _littleCanineLostHp = 0;
                _littleCanineTime = 0;
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
     
        if (_inventory.GetIsHaveItem(ItemName.니트로글리세린_B))
        {
            if (Random.Range(0, 100) < 10)
            {
                _inventory.Explosion(_player.Character, point, 10);
            }
        }
            // 타겟이 죽는다면 
            if (target == null || target.IsDead)
        {
            if (_inventory.GetIsHaveItem(ItemName.푸카라스웨트_B))
            {
                Managers.GetManager<GameManager>().Mental += 1;
            }
            if (_inventory.GetIsHaveItem(ItemName.문들어진송곳니)|| _inventory.GetIsHaveItem(ItemName.문들어진송곳니_A)|| _inventory.GetIsHaveItem(ItemName.문들어진송곳니_B))
            {
                if (Random.Range(0, 100) < 5)
                {
                    _player.Character.Hp += Mathf.RoundToInt(_player.Character.MaxHp / (_inventory.GetIsHaveItem(ItemName.문들어진송곳니_B) ? 5 : 10f));

                    if (_inventory.GetIsHaveItem(ItemName.문들어진송곳니_A))
                    {
                        Character creature = Managers.GetManager<GameManager>().Creature;
                        creature.Hp += Mathf.RoundToInt(creature.MaxHp / 10f);
                    }
                }
            }

            // 니트로글리세린
            {
                float probability = 0;
                float radius = 0;
                if (_inventory.GetIsHaveItem(ItemName.니트로글리세린))
                {
                    probability = 10;
                    radius = 10;
                }

                if (_inventory.GetIsHaveItem(ItemName.니트로글리세린_A))
                {
                    probability = 20;
                    radius = 20;
                }
                if (probability != 0)
                {
                    if (Random.Range(0, 100) < probability)
                    {
                        _inventory.Explosion(_player.Character, point, radius);
                    }
                }
            }
        }
    }

    void OnAddtionalAttack(Character target, int totalDamage, float power, Vector3 direction, Vector3 point, float stunTime)
    {
        if (target == null || target.IsDead)
        {
            if (_inventory.GetIsHaveItem(ItemName.푸카라스웨트_B))
            {
                Managers.GetManager<GameManager>().Mental += 1;
            }
        }
    }

    private void InvisibleHand()
    {
        if (_inventory.GetIsHaveItem(ItemName.보이지않는손) || _inventory.GetIsHaveItem(ItemName.보이지않는손_A) || _inventory.GetIsHaveItem(ItemName.보이지않는손_B))
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
                    float reloadTimeMul = 3f;
                    float reloadAmmoMul = 1f;

                    if (_inventory.GetIsHaveItem(ItemName.보이지않는손_A))
                        reloadTimeMul = 1.5f;
                    if (_inventory.GetIsHaveItem(ItemName.보이지않는손_B))
                        reloadAmmoMul = 2f;
                    if (_invisbleHandElaspedTimeList[i] > weapon.ReloadTime * reloadTimeMul)
                    {
                        Managers.GetManager<TextManager>().ShowText(_player.transform.position + Vector3.up * 5, $"{weapon.WeaponName.ToString()} 장전완료", 10, Color.green);
                        weapon.CompleteReload(reloadAmmoMul,false);
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

        //아이템 : 검은유리창파편
        if (_inventory.GetIsHaveItem(ItemName.검은유리창파편) || _inventory.GetIsHaveItem(ItemName.검은유리창파편_A) || _inventory.GetIsHaveItem(ItemName.검은유리창파편_B))
        {
            if (_player.WeaponSwaper.CurrentWeapon != null)
            {
                float coefficient = 5;
                if (_inventory.GetIsHaveItem(ItemName.검은유리창파편_A))
                    coefficient = 10;
                percentage += ((int)_player.WeaponSwaper.CurrentWeapon.MaxAmmo / 10) * coefficient;
            }
        }

        //아이템 : 망각의 서
        if (_inventory.GetIsHaveItem(ItemName.망각의서) || _inventory.GetIsHaveItem(ItemName.망각의서_A) || _inventory.GetIsHaveItem(ItemName.망각의서_B))
        {
            percentage -= _bookOfOblivionLoseAttackPowerPercentage;
        }

        // 아이템 : 탁한 잎
        if (_inventory.GetIsHaveItem(ItemName.탁한잎) || _inventory.GetIsHaveItem(ItemName.탁한잎_A) || _inventory.GetIsHaveItem(ItemName.탁한잎_B))
        {
            percentage += _cludyLeafIncreaseAttackPowerPercentage;
        }


        // 아이템 : 마지막탄환
        if (_inventory.GetIsHaveItem(ItemName.마지막탄환) || (_inventory.GetIsHaveItem(ItemName.마지막탄환_B)))
        {
            if (_player.WeaponSwaper.CurrentWeapon != null)
            {
                if (_player.WeaponSwaper.CurrentWeapon.CurrentAmmo == 1)
                {
                    percentage += 100f;
                }
            }
        }
        if (_inventory.GetIsHaveItem(ItemName.마지막탄환_A))
        {
            if (_player.WeaponSwaper.CurrentWeapon != null)
            {
                if (_player.WeaponSwaper.CurrentWeapon.CurrentAmmo == 1)
                {
                    percentage += 200f;
                }
            }
        }

        // 아이템 : 부서진 약지
        {
            if (_inventory.GetIsHaveItem(ItemName.부서진약지) || _inventory.GetIsHaveItem(ItemName.부서진약지_B))
            {
                if (_inventory.IsActiveBrokenRingFinger)
                {
                    percentage += _brokenRingFingerIncreaseAttackPowerPercentage;
                }
            }
            if (_inventory.GetIsHaveItem(ItemName.부서진약지_A))
            {
                if (_inventory.IsActiveBrokenRingFinger)
                {
                    percentage += _brokenRingFingerIncreaseAttackPowerPercentage*2;
                }
            }
        }
        // 아이템 : 검붉은나무파편_A
        if (_inventory.GetIsHaveItem(ItemName.검붉은나무파편_A))
        {
            percentage += (int)(_player.Character.MaxHp / 25f) * 15;
        }
        // 아이템 : 검붉은나무파편, 검붉은나무파편_B
        else if (_inventory.GetIsHaveItem(ItemName.검붉은나무파편) || _inventory.GetIsHaveItem(ItemName.검붉은나무파편_B))
        {
            percentage += (int)(_player.Character.MaxHp / 25f) *10;
        }

        // 아이템: 작은송곳니
        {
            if (_inventory.GetIsHaveItem(ItemName.작은송곳니))
            {
                percentage = (int)(_fireTime) * 3;
            }
            if (_inventory.GetIsHaveItem(ItemName.작은송곳니_A))
            {
                percentage = (int)(_fireTime) * 3;
            }
        }

        return percentage;
    }

    public float GetIncreasedAttackSpeedPercentage()
    {
        Character creature = Managers.GetManager<GameManager>().Creature;
        float percentage = 0;

        percentage += IncreasedAttackSpeedPercentage;

        //// 아이템: 과충전배터리
        //if (_inventory.GetItemCount(ItemName.과충전배터리) > 0)
        //{
        //    percentage += _inventory.GetItemCount(ItemName.피뢰침) * 5;
        //    percentage += _inventory.GetItemCount(ItemName.부서진건전지) * 5;
        //}

        // 아이템 : 검붉은나무파편_B
        if (_inventory.GetIsHaveItem(ItemName.검붉은나무파편_B))
        {
            percentage += (int)(_player.Character.MaxHp / 25f) * 10;
        }

        // 아이템 : 피뢰침_B
        if (_lightingRod_B_Active)
        {
            percentage += _lightingRod_B_Coefficient;
        }

        // 아이템 : 검은유리창파편_B
        if (_inventory.GetIsHaveItem(ItemName.검은유리창파편_B))
        {
            if(_fireTime> 0)
            {
                percentage += ((int)(_fireTime)) * 5;
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
  

    public float GetHpRegeneration()
    {
        float regen = 0;
        regen = _player.Character.IncreasedHpRegeneration;

        return regen;
    }
}

