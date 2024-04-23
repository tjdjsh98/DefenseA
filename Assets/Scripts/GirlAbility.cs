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
    // ������ ������ �߰��� �ɷ�ġ
    public float IncreasedAttackPowerPercentage { set; get; }
    public float IncreasedAttackSpeedPercentage { set; get; }
    public float IncreasedReloadSpeedPercentage { set; get; }


    float _fireTime = 0;

    // ��ų
    Dictionary<CardName, Action<SkillSlot>> _skillDictionary = new Dictionary<CardName, Action<SkillSlot>>();

    // �۰���
    public bool IsActiveCanine { get; set; }
    public float _canineElasepdTime;
    public float _canineDurationTime = 10;
    SkillSlot _canineSlot;

    #region ������ �ɷ�

    // Ź�� ��
    float _cludyLeafIncreaseAttackPowerPercentage;
    int _cludyLeafIncreaseMaxHp;

    // ������ �ʴ� ��
    List<float> _invisbleHandElaspedTimeList = new List<float>();

    // ������� �۰���
    int _crumbledCanineHuntingCount = 0;

    // ���߻� źâ

    // �μ�������
    float _brokenRingFingerIncreaseAttackPowerPercentage = 50;

    // �Ƿ�ħ_B
    bool _lightingRod_B_Active = false;

    // ������ ��
    float _bookOfOblivionLoseAttackPowerPercentage = 0;

    // �ǹ����������_B
    int _bloodyBoneNecklessIncreasedAttackPoint = 0;

    // �����۰���_B
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
        // ������ : �ǹ����������
        {
            int hpAmount = 0;
            if (_inventory.GetIsHaveItem(ItemName.�ǹ����������))
            {
                hpAmount = 30;
            }
            if (_inventory.GetIsHaveItem(ItemName.�ǹ����������_A))
            {
                hpAmount = 70;
            }
            if (_inventory.GetIsHaveItem(ItemName.�ǹ����������_B))
            {
                hpAmount = 30;
                Creature.AttackPower += 1;
                _bloodyBoneNecklessIncreasedAttackPoint += 1;
            }
            Creature.Hp += hpAmount;
        }

        //������: ������ ��
        {
            if (_inventory.GetIsHaveItem(ItemName.�����Ǽ�))
            {
                _bookOfOblivionLoseAttackPowerPercentage += 5;
            }
            else if (_inventory.GetIsHaveItem(ItemName.�����Ǽ�_A))
            {
                _bookOfOblivionLoseAttackPowerPercentage += 2;
            }
            else if (_inventory.GetIsHaveItem(ItemName.�����Ǽ�_B))
            {
                _bookOfOblivionLoseAttackPowerPercentage += 10;
            }
            else
            {
                _bookOfOblivionLoseAttackPowerPercentage = 0;
            }
        }
        // ������: ��������_B
        if (_inventory.GetIsHaveItem(ItemName.��������_B))
        {
            if (Random.Range(0, 100) < 50)
            {
                _player.Character.IgnoreDamageCount++;
            }
        }
        // ������: �μ��� ����
        if (dmg > 0 && _inventory.IsActiveBrokenRingFinger)
            _inventory.IsActiveBrokenRingFinger = false;
    }

    private void OnPlayerDead()
    {
    }

    private void OnCreatureDead()
    {
        //Ź����
        {
            float attackPercentage = 0;
            int maxHp = 0;
            if (_inventory.GetIsHaveItem(ItemName.Ź����_A))
            {
                attackPercentage = 15;
            }
            if (_inventory.GetIsHaveItem(ItemName.Ź����_B))
            {
                attackPercentage = 8;
                maxHp = 10;
            }
            if (_inventory.GetIsHaveItem(ItemName.Ź����))
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
        // �󸶳� �÷� �߻� �ߴ��� üũ
        if (_player.IsFire && _player.WeaponSwaper.CurrentWeapon && !_player.WeaponSwaper.CurrentWeapon.IsReload)
            _fireTime += Time.deltaTime;
        else
            _fireTime = 0;


        InvisibleHand();
        //FastReload();
        _player.Character.IncreasedHpRegeneration = GetHpRegeneration();

        //��ų: �۰���
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

        //�Ƿ�ħ_B
        if (LightingRod_B_Active)
        {
            _lightingRod_B_ElasepdTime += Time.deltaTime;
            if (_lightingRod_B_ElasepdTime > _lightingRod_B_DurationTime)
            {
                _lightingRod_B_ElasepdTime = 0;
                LightingRod_B_Active = false;
            }
        }
        // Ź����
        if (_cludyLeafIncreaseMaxHp != 0 || _cludyLeafIncreaseAttackPowerPercentage != 0) {
            if (!_inventory.GetIsHaveItem(ItemName.Ź����) && !_inventory.GetIsHaveItem(ItemName.Ź����_A) && !_inventory.GetIsHaveItem(ItemName.Ź����_B))
            {
                _player.Character.RemoveaxHp(_cludyLeafIncreaseMaxHp);
                _cludyLeafIncreaseMaxHp = 0;
                _cludyLeafIncreaseAttackPowerPercentage = 0;
            }
        }

        //�ǹ����������_B
        if (_bloodyBoneNecklessIncreasedAttackPoint != 0)
        {
            if (!_inventory.GetIsHaveItem(ItemName.�ǹ����������_B))
            {
                Creature.AttackPower -= _bloodyBoneNecklessIncreasedAttackPoint;
                _bloodyBoneNecklessIncreasedAttackPoint = 0;
            }
        }

        // ���� �۰���_B
        if (_inventory.GetIsHaveItem(ItemName.�����۰���_B))
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


    #region ��ų ����
    void RegistSkill()
    {
        _skillDictionary.Add(CardName.�۰���, PlayCanine);
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
     
        if (_inventory.GetIsHaveItem(ItemName.��Ʈ�α۸�����_B))
        {
            if (Random.Range(0, 100) < 10)
            {
                _inventory.Explosion(_player.Character, point, 10);
            }
        }
            // Ÿ���� �״´ٸ� 
            if (target == null || target.IsDead)
        {
            if (_inventory.GetIsHaveItem(ItemName.Ǫī�󽺿�Ʈ_B))
            {
                Managers.GetManager<GameManager>().Mental += 1;
            }
            if (_inventory.GetIsHaveItem(ItemName.��������۰���)|| _inventory.GetIsHaveItem(ItemName.��������۰���_A)|| _inventory.GetIsHaveItem(ItemName.��������۰���_B))
            {
                if (Random.Range(0, 100) < 5)
                {
                    _player.Character.Hp += Mathf.RoundToInt(_player.Character.MaxHp / (_inventory.GetIsHaveItem(ItemName.��������۰���_B) ? 5 : 10f));

                    if (_inventory.GetIsHaveItem(ItemName.��������۰���_A))
                    {
                        Character creature = Managers.GetManager<GameManager>().Creature;
                        creature.Hp += Mathf.RoundToInt(creature.MaxHp / 10f);
                    }
                }
            }

            // ��Ʈ�α۸�����
            {
                float probability = 0;
                float radius = 0;
                if (_inventory.GetIsHaveItem(ItemName.��Ʈ�α۸�����))
                {
                    probability = 10;
                    radius = 10;
                }

                if (_inventory.GetIsHaveItem(ItemName.��Ʈ�α۸�����_A))
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
            if (_inventory.GetIsHaveItem(ItemName.Ǫī�󽺿�Ʈ_B))
            {
                Managers.GetManager<GameManager>().Mental += 1;
            }
        }
    }

    private void InvisibleHand()
    {
        if (_inventory.GetIsHaveItem(ItemName.�������ʴ¼�) || _inventory.GetIsHaveItem(ItemName.�������ʴ¼�_A) || _inventory.GetIsHaveItem(ItemName.�������ʴ¼�_B))
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

                    if (_inventory.GetIsHaveItem(ItemName.�������ʴ¼�_A))
                        reloadTimeMul = 1.5f;
                    if (_inventory.GetIsHaveItem(ItemName.�������ʴ¼�_B))
                        reloadAmmoMul = 2f;
                    if (_invisbleHandElaspedTimeList[i] > weapon.ReloadTime * reloadTimeMul)
                    {
                        Managers.GetManager<TextManager>().ShowText(_player.transform.position + Vector3.up * 5, $"{weapon.WeaponName.ToString()} �����Ϸ�", 10, Color.green);
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

        //������ : ��������â����
        if (_inventory.GetIsHaveItem(ItemName.��������â����) || _inventory.GetIsHaveItem(ItemName.��������â����_A) || _inventory.GetIsHaveItem(ItemName.��������â����_B))
        {
            if (_player.WeaponSwaper.CurrentWeapon != null)
            {
                float coefficient = 5;
                if (_inventory.GetIsHaveItem(ItemName.��������â����_A))
                    coefficient = 10;
                percentage += ((int)_player.WeaponSwaper.CurrentWeapon.MaxAmmo / 10) * coefficient;
            }
        }

        //������ : ������ ��
        if (_inventory.GetIsHaveItem(ItemName.�����Ǽ�) || _inventory.GetIsHaveItem(ItemName.�����Ǽ�_A) || _inventory.GetIsHaveItem(ItemName.�����Ǽ�_B))
        {
            percentage -= _bookOfOblivionLoseAttackPowerPercentage;
        }

        // ������ : Ź�� ��
        if (_inventory.GetIsHaveItem(ItemName.Ź����) || _inventory.GetIsHaveItem(ItemName.Ź����_A) || _inventory.GetIsHaveItem(ItemName.Ź����_B))
        {
            percentage += _cludyLeafIncreaseAttackPowerPercentage;
        }


        // ������ : ������źȯ
        if (_inventory.GetIsHaveItem(ItemName.������źȯ) || (_inventory.GetIsHaveItem(ItemName.������źȯ_B)))
        {
            if (_player.WeaponSwaper.CurrentWeapon != null)
            {
                if (_player.WeaponSwaper.CurrentWeapon.CurrentAmmo == 1)
                {
                    percentage += 100f;
                }
            }
        }
        if (_inventory.GetIsHaveItem(ItemName.������źȯ_A))
        {
            if (_player.WeaponSwaper.CurrentWeapon != null)
            {
                if (_player.WeaponSwaper.CurrentWeapon.CurrentAmmo == 1)
                {
                    percentage += 200f;
                }
            }
        }

        // ������ : �μ��� ����
        {
            if (_inventory.GetIsHaveItem(ItemName.�μ�������) || _inventory.GetIsHaveItem(ItemName.�μ�������_B))
            {
                if (_inventory.IsActiveBrokenRingFinger)
                {
                    percentage += _brokenRingFingerIncreaseAttackPowerPercentage;
                }
            }
            if (_inventory.GetIsHaveItem(ItemName.�μ�������_A))
            {
                if (_inventory.IsActiveBrokenRingFinger)
                {
                    percentage += _brokenRingFingerIncreaseAttackPowerPercentage*2;
                }
            }
        }
        // ������ : �˺�����������_A
        if (_inventory.GetIsHaveItem(ItemName.�˺�����������_A))
        {
            percentage += (int)(_player.Character.MaxHp / 25f) * 15;
        }
        // ������ : �˺�����������, �˺�����������_B
        else if (_inventory.GetIsHaveItem(ItemName.�˺�����������) || _inventory.GetIsHaveItem(ItemName.�˺�����������_B))
        {
            percentage += (int)(_player.Character.MaxHp / 25f) *10;
        }

        // ������: �����۰���
        {
            if (_inventory.GetIsHaveItem(ItemName.�����۰���))
            {
                percentage = (int)(_fireTime) * 3;
            }
            if (_inventory.GetIsHaveItem(ItemName.�����۰���_A))
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

        //// ������: ���������͸�
        //if (_inventory.GetItemCount(ItemName.���������͸�) > 0)
        //{
        //    percentage += _inventory.GetItemCount(ItemName.�Ƿ�ħ) * 5;
        //    percentage += _inventory.GetItemCount(ItemName.�μ���������) * 5;
        //}

        // ������ : �˺�����������_B
        if (_inventory.GetIsHaveItem(ItemName.�˺�����������_B))
        {
            percentage += (int)(_player.Character.MaxHp / 25f) * 10;
        }

        // ������ : �Ƿ�ħ_B
        if (_lightingRod_B_Active)
        {
            percentage += _lightingRod_B_Coefficient;
        }

        // ������ : ��������â����_B
        if (_inventory.GetIsHaveItem(ItemName.��������â����_B))
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

