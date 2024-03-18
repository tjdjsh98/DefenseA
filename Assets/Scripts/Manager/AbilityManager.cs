using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AbilityManager : ManagerBase
{
    #region 캐릭터 변수
    Character Girl => Managers.GetManager<GameManager>().Girl;
    Player Player => Managers.GetManager<GameManager>().Player;

    Character Creature => Managers.GetManager<GameManager>().Creature;
    CreatureAI CreatureAI => Managers.GetManager<GameManager>().CreatureAI;

    Character Wall => Managers.GetManager<GameManager>().Wall;
    WallAI WallAI => Managers.GetManager<GameManager>().WallAI;
    #endregion

    // 공용 능력
    Dictionary<CommonAbilityName, bool> _commonAbilityUnlocks = new Dictionary<CommonAbilityName, bool>();

    // 스킬 슬롯
    [SerializeField] int _skillCount = 4;
    [SerializeField] List<SkillSlot> _skillSlotList = new List<SkillSlot>();
    public List<SkillSlot> SkillSlotList => _skillSlotList;
    Dictionary<SkillName, Action<SkillSlot>> _skillDictionary = new Dictionary<SkillName, Action<SkillSlot>>();

    // 검은 구체
    float _blackSphereCoolTime = 5;
    float _blackSphereTime;

    // 전기 관련 능력
    public float CurrentElectric;
    public override void Init()
    {
        RegistSkill();

        Managers.GetManager<InputManager>().Skill1KeyDownHandler += UseSkill1;
        Managers.GetManager<InputManager>().Skill2KeyDownHandler += UseSkill2;
        Managers.GetManager<InputManager>().Skill3KeyDownHandler += UseSkill3;
        Managers.GetManager<InputManager>().Skill4KeyDownHandler += UseSkill4;
        for(int i =0; i< _skillCount; i++) 
        {
            _skillSlotList.Add(new SkillSlot());
        }
    }

    public override void ManagerUpdate()
    {
        HandleSkill();
        ProcessCommonAbility();
    }

    void RegistSkill()
    {
        _skillDictionary.Add(SkillName.VolleyFire, PlayVolleyFire);
    }

    void UseSkill(SkillSlot skillSlot)
    {
        if (skillSlot.skillData == null) return;

        if (_skillDictionary.TryGetValue(skillSlot.skillData.skillName, out var func))
        {
            func?.Invoke(skillSlot);
        }
    }

    void PlayVolleyFire(SkillSlot skillSlot)
    {
        if (Player.GirlAbility.BlackSphereList.Count <= 0) return;

        Vector3 mousePosition = Managers.GetManager<InputManager>().MouseWorldPosition;

        BlackSphere blackSphere = Player.GirlAbility.BlackSphereList[0];
        Player.GirlAbility.BlackSphereList.RemoveAt(0);

        blackSphere.ChangeAttackMode(mousePosition, 0);

    }
    
    void ProcessCommonAbility()
    {
        if (GetIsHaveCommonAbility(CommonAbilityName.BlackSphere))
        {
            if (_blackSphereTime < _blackSphereCoolTime)
            {
                _blackSphereTime += Time.deltaTime;
            }
            else
            {
                Player.GirlAbility.AddBlackSphere(Player.transform.position);
                _blackSphereTime = 0;
            }
        }
    }

    public void ApplyCardAbility(CardData cardData)
    {
        GirlCardData girlCardData = cardData as GirlCardData;
        CreatureCardData creatureCardData = cardData as CreatureCardData;
        WallCardData wallCardData = cardData as WallCardData;
        CommonCardData commonCardData = cardData as CommonCardData;
        if (girlCardData != null)
        {
            Girl.AddMaxHp(girlCardData.IncreaseHp);
            Girl.IncreasedRecoverHpPower += girlCardData.IncreaseRecoverHpPower;
            Girl.IncreasedDamageReducePercentage += girlCardData.IncreaseDamageReducePercentage;
            Girl.AttackPower += girlCardData.IncreaseAttackPoint;

            if (girlCardData.UnlockAbility != GirlAbilityName.None && !Player.GirlAbility.GetIsHaveAbility(girlCardData.UnlockAbility))
                Player.GirlAbility.AddGirlAbility(girlCardData.UnlockAbility);
            Player.IncreasedAttackSpeedPercentage += girlCardData.DecreaseFireDelayPercentage;
            Player.IncreasedReloadSpeedPercentage += girlCardData.IncreaseReloadSpeedPercentage;
            Player.IncreasedPenerstratingPower += girlCardData.IncreasePenerstratingPower;

            AddSkill(girlCardData);
        }
        if (creatureCardData != null)
        {
            Creature.AddMaxHp(creatureCardData.IncreaseHp);
            Creature.IncreasedRecoverHpPower += creatureCardData.IncreaseRecoverHpPower;
            Creature.IncreasedDamageReducePercentage += creatureCardData.IncreaseDamageReducePercentage;
            Creature.AttackPower += creatureCardData.IncreaseAttackPoint;

            CreatureAI.CreatureAbility.AddAbility(creatureCardData.UnlockAbility);

            AddSkill(creatureCardData);
        }
        if (wallCardData != null)
        {
            Wall.AddMaxHp(wallCardData.IncreaseHp);
            Wall.IncreasedRecoverHpPower += wallCardData.IncreaseRecoverHpPower;
            Wall.IncreasedDamageReducePercentage += wallCardData.IncreaseDamageReducePercentage;
            Wall.AttackPower += wallCardData.IncreaseAttackPoint;
            Wall.transform.localScale += new Vector3(wallCardData.SizeUpPercentage / 100, wallCardData.SizeUpPercentage / 100, 0);
            
            WallAI.WallAbility.AddGirlAbility(wallCardData.UnlockAbility);
            AddSkill(wallCardData);
        }

        if (commonCardData != null)
        {
            Wall.AddMaxHp(commonCardData.IncreaseHp);
            Creature.AddMaxHp(commonCardData.IncreaseHp);
            Girl.AddMaxHp(commonCardData.IncreaseHp);

            Wall.AttackPower += commonCardData.IncreaseAttackPoint;
            Creature.AttackPower += commonCardData.IncreaseAttackPoint;
            Girl.AttackPower += commonCardData.IncreaseAttackPoint;

            AddCommonAbility(commonCardData.UnlockAbility);
            AddSkill(commonCardData);
        }
    }
    public void AddCommonAbility(CommonAbilityName commonAbilityName)
    {
        if ((int)commonAbilityName > (int)CommonAbilityName.None && (int)commonAbilityName < (int)CommonAbilityName.END)
        {
            bool turnTrue = false;

            if (_commonAbilityUnlocks.ContainsKey(commonAbilityName))
            {
                if (!_commonAbilityUnlocks[commonAbilityName])
                {
                    turnTrue = true;
                    _commonAbilityUnlocks[commonAbilityName] = true;
                }
            }
            else
            {
                turnTrue = true;
                _commonAbilityUnlocks.Add(commonAbilityName, true);
            }

            if (turnTrue)
            {
                switch (commonAbilityName)
                {
                    case CommonAbilityName.None:
                        break;
                    case CommonAbilityName.ControlSphere:
                        Player.GirlAbility.MaxBlackSphereCount = 20;
                        break;
                    case CommonAbilityName.VolleyFire:
                        break;
                    case CommonAbilityName.ExplosiveSphere:
                        break;
                    case CommonAbilityName.Bait:
                        break;
                    case CommonAbilityName.MicroPower:
                        break;
                    case CommonAbilityName.ExtraBattery:
                        break;
                    case CommonAbilityName.SelfGeneration:
                        break;
                    case CommonAbilityName.Appetite:
                        break;
                    case CommonAbilityName.END:
                        break;
                }
            }
        }
    }
    public bool GetIsHaveCommonAbility(CommonAbilityName commonAbility)
    {
        if (_commonAbilityUnlocks.TryGetValue(commonAbility, out var abilityUnlock))
        {
            return abilityUnlock;
        }

        return false;
    }

    public void AddSkill(GirlCardData cardData, int index = -1)
    {
        if (cardData == null || !cardData.IsActiveAbility) return;
        if (cardData.UnlockAbility == GirlAbilityName.None) return;

        if (index == -1)
        {
            for (int i = 0; i < _skillCount; i++)
            {
                if (_skillSlotList[i].skillData == null)
                {
                    index = i;
                    break;
                }

            }
        }

        if (index < 0 || _skillSlotList.Count <= index) return;

        SkillData skillData = Managers.GetManager<DataManager>().GetData<SkillData>((int)ConvertAbilityToCardName(cardData.UnlockAbility));
        if (skillData == null) return;

        _skillSlotList[index].skillData = skillData;
        _skillSlotList[index].skillTime = 0;
        _skillSlotList[index].isActive = false;

    }
    public void AddSkill(CreatureCardData cardData, int index = -1)
    {
        if (cardData == null || !cardData.IsActiveAbility) return;
        if (cardData.UnlockAbility == CreatureAbilityName.None) return;

        if (index == -1)
        {
            for (int i = 0; i < _skillCount; i++)
            {
                if (_skillSlotList[i].skillData == null)
                {
                    index = i;
                    break;
                }

            }
        }

        if (index < 0 || _skillSlotList.Count <= index) return;

        SkillData skillData = Managers.GetManager<DataManager>().GetData<SkillData>((int)ConvertAbilityToCardName(cardData.UnlockAbility));
        if (skillData == null) return;

        _skillSlotList[index].skillData = skillData;
        _skillSlotList[index].skillTime = 0;
        _skillSlotList[index].isActive = false;

    }
    public void AddSkill(WallCardData cardData, int index = -1)
    {
        if (cardData == null || !cardData.IsActiveAbility) return;
        if (cardData.UnlockAbility == WallAbilityName.None) return;

        if (index == -1)
        {
            for (int i = 0; i < _skillCount; i++)
            {
                if (_skillSlotList[i].skillData == null)
                {
                    index = i;
                    break;
                }

            }
        }

        if (index < 0 || _skillSlotList.Count <= index) return;

        SkillData skillData = Managers.GetManager<DataManager>().GetData<SkillData>((int)ConvertAbilityToCardName(cardData.UnlockAbility));
        if (skillData == null) return;

        _skillSlotList[index].skillData = skillData;
        _skillSlotList[index].skillTime = 0;
        _skillSlotList[index].isActive = false;

    }
    public void AddSkill(CommonCardData cardData, int index = -1)
    {
        if (cardData == null || !cardData.IsActiveAbility) return;
        if (cardData.UnlockAbility == CommonAbilityName.None) return;

        if (index == -1)
        {
            for (int i = 0; i < _skillCount; i++)
            {
                if (_skillSlotList[i].skillData == null)
                {
                    index = i;
                    break;
                }

            }
        }

        if (index < 0 || _skillSlotList.Count <= index) return;

        SkillData skillData = Managers.GetManager<DataManager>().GetData<SkillData>((int)ConvertAbilityToCardName(cardData.UnlockAbility));
        Debug.Log(skillData);
        if (skillData == null) return;

        _skillSlotList[index].skillData = skillData;
        _skillSlotList[index].skillTime = 0;
        _skillSlotList[index].isActive = false;

    }

    void HandleSkill()
    {
        foreach (var skill in _skillSlotList)
        {
            if (skill.skillData == null) continue;
            if (skill.isActive) return;

            if (skill.skillTime < skill.skillCoolTime)
            {
                skill.skillTime += Time.deltaTime;
            }
            else
            {
                skill.skillTime = skill.skillCoolTime;
            }
        }
    }

    public void ResetSkillTime(SkillName skillName)
    {
        for(int i =0; i < _skillSlotList.Count; i++)
        {
            if (_skillSlotList[i].skillData == null) continue;
            if (_skillSlotList[i].skillData.skillName == skillName)
            {
                _skillSlotList[i].skillTime = 0;
                return;
            }
        }
    }
    void UseSkill(int index)
    {
        if (_skillSlotList[index].skillData == null) return;

        if (_skillSlotList[index].skillData.character == Define.MainCharacter.Girl)
        {
            Player.UseSkill(_skillSlotList[index]);
        }
        if (_skillSlotList[index].skillData.character == Define.MainCharacter.Creture)
        {
            CreatureAI.UseSkill(_skillSlotList[index]);
        }
        if (_skillSlotList[index].skillData.character == Define.MainCharacter.Wall)
        {
            WallAI.UseSkill(_skillSlotList[index]);
        }
        if (_skillSlotList[index].skillData.character == Define.MainCharacter.Common)
        {
            UseSkill(_skillSlotList[index]);
        }
    }
    public void UseSkill1() { UseSkill(0); }
    public void UseSkill2() { UseSkill(1); }
    public void UseSkill3() { UseSkill(2); }
    public void UseSkill4() { UseSkill(3); }

    SkillName ConvertAbilityToCardName(GirlAbilityName girlAbilityName)
    {
        string abilityName = girlAbilityName.ToString();
        for (int i = 0; i < (int)SkillName.END; i++)
        {
            if (((SkillName)i).ToString(). Equals(girlAbilityName.ToString()))
            {
                return (SkillName)i;
            }
        }
        return SkillName.None;
    }
    SkillName ConvertAbilityToCardName(CreatureAbilityName creatureAbilityName)
    {
        string abilityName = creatureAbilityName.ToString();
        for (int i = 0; i < (int)SkillName.END; i++)
        {
            if (((SkillName)i).ToString().Equals(abilityName))
            {
                return (SkillName)i;    
            }
        }
        return SkillName.None;
    }
    SkillName ConvertAbilityToCardName(WallAbilityName wallAbilityName)
    {
        string abilityName = wallAbilityName.ToString();
        for (int i = 0; i < (int)SkillName.END; i++)
        {
            if (((SkillName)i).ToString().Equals(abilityName))
            {
                return (SkillName)i;
            }
        }
        return SkillName.None;
    }
    SkillName ConvertAbilityToCardName(CommonAbilityName commonAbilityName)
    {
        string abilityName = commonAbilityName.ToString();
        for (int i = 0; i < (int)SkillName.END; i++)
        {
            if (((SkillName)i).ToString().Equals(abilityName))
            {
                return (SkillName)i;
            }
        }
        return SkillName.None;
    }

}

[System.Serializable]
public class SkillSlot
{
    public SkillData skillData;
    public float skillCoolTime=>skillData == null? 0:skillData.coolTime;
    public float skillTime;
    public bool isActive;
}
