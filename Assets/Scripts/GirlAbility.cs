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
    // ������ ������ �߰��� �ɷ�ġ
    public float IncreasedAttackPowerPercentage { set;get; }
    public float IncreasedAttackSpeedPercentage { set;get; }
    public float IncreasedReloadSpeedPercentage { set;get; }

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
    // ������ �ʴ� ��
    List<float> _invisbleHandElaspedTimeList = new List<float>();

    // ������� �۰���
    int _crumbledCanineHuntingCount = 0;

    // ���߻� źâ

    // �μ�������
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
        if (_inventory.GetItemCount(ItemName.�ǹ����������) > 0)
        {
            Creature.Hp += 10 * _inventory.GetItemCount(ItemName.�ǹ����������);
        }
        if (_inventory.GetItemCount(ItemName.�μ�������) > 0)
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
        if (_inventory.GetItemCount(ItemName.Ź����) > 0)
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

   
        if (_inventory.GetItemCount(ItemName.�����۰���) > 0)
        {
            if(Random.Range(0,100) < 5)
                Creature.Hp += _inventory.GetItemCount(ItemName.�����۰���) * 10;
        }
        if (_inventory.GetItemCount(ItemName.�����ڱ���) > 0)
        {
            if (Random.Range(0, 100) < 10)
                _player.Character.AddtionalAttack(target, totalDamage, 0, Vector3.zero, point+Vector3.up*1, 0);
        }

        // Ÿ���� �״´ٸ� 
        if (target == null || target.IsDead)
        {
            if (_inventory.GetItemCount(ItemName.��������۰���) > 0)
            {
                _crumbledCanineHuntingCount++;
                if (_crumbledCanineHuntingCount > 5)
                {
                    _player.Character.Hp += _inventory.GetItemCount(ItemName.��������۰���);
                }
            }

            if (_inventory.GetItemCount(ItemName.��Ʈ�α۸�����) > 0)
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
        if (_inventory.GetItemCount(ItemName.�������ʴ¼�) > 0)
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
                        Managers.GetManager<TextManager>().ShowText(_player.transform.position + Vector3.up * 5, $"{weapon.WeaponName.ToString()} �����Ϸ�", 10, Color.green);
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

        //������ : ��������â����
        if (_player.WeaponSwaper.CurrentWeapon != null)
        {
            if(_inventory.GetItemCount(ItemName.��������â����) > 0)
                percentage += ((int)_player.WeaponSwaper.CurrentWeapon.MaxAmmo/10) * 5 * _inventory.GetItemCount(ItemName.��������â����);
        }

        // ������ : ������źȯ
        if (_player.WeaponSwaper.CurrentWeapon != null)
        {
            if (_player.WeaponSwaper.CurrentWeapon.CurrentAmmo == 1)
            {
                percentage += _inventory.GetItemCount(ItemName.������źȯ) * 100f;
            }
        }

        // ������ : �μ��� ����
        if (_inventory.GetItemCount(ItemName.�μ�������) > 0)
        {
            if (_inventory.IsActiveBrokenRingFinger)
            {
                percentage += _brokenRingFingerIncreaseAttackPowerPercentage * _inventory.GetItemCount(ItemName.�μ�������);
            }
        }

        return percentage;
    }

    public float GetIncreasedAttackSpeedPercentage()
    {
        Character creature = Managers.GetManager<GameManager>().Creature;
        float percentage = 0;

        percentage += IncreasedAttackSpeedPercentage;

        // ������: ���������͸�
        if (_inventory.GetItemCount(ItemName.���������͸�) > 0)
        {
            percentage += _inventory.GetItemCount(ItemName.�Ƿ�ħ) * 5;
            percentage += _inventory.GetItemCount(ItemName.�μ���������) * 5;
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

