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
    [SerializeField] int _creatureSkillCount;
    [SerializeField] List<SkillSlot> _skillList = new List<SkillSlot>();
    public List<SkillSlot> CreatureSkillSlotList => _skillList;

    // 검은 구체
    float _blackSphereCoolTime = 5;
    float _blackSphereTime;

    // 전기 관련 능력
    public float CurrentElectric;
    public override void Init()
    {
    }

    public override void ManagerUpdate()
    {
        ProcessCommonAbility();
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
        }
        if (creatureCardData != null)
        {
            Creature.AddMaxHp(creatureCardData.IncreaseHp);
            Creature.IncreasedRecoverHpPower += creatureCardData.IncreaseRecoverHpPower;
            Creature.IncreasedDamageReducePercentage += creatureCardData.IncreaseDamageReducePercentage;
            Creature.AttackPower += creatureCardData.IncreaseAttackPoint;

            CreatureAI.CreatureAbility.AddAbility(creatureCardData.UnlockAbility);
        }
        if (wallCardData != null)
        {
            Wall.AddMaxHp(wallCardData.IncreaseHp);
            Wall.IncreasedRecoverHpPower += wallCardData.IncreaseRecoverHpPower;
            Wall.IncreasedDamageReducePercentage += wallCardData.IncreaseDamageReducePercentage;
            Wall.AttackPower += wallCardData.IncreaseAttackPoint;
            Wall.transform.localScale += new Vector3(wallCardData.SizeUpPercentage / 100, wallCardData.SizeUpPercentage / 100, 0);
            
            WallAI.WallAbility.AddGirlAbility(wallCardData.UnlockAbility);
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
        }
    }
    public void AddCommonAbility(CommonAbilityName commonAbilityName)
    {
        if ((int)commonAbilityName > (int)CommonAbilityName.None && (int)commonAbilityName < (int)CommonAbilityName.END)
        {
            if (_commonAbilityUnlocks.ContainsKey(commonAbilityName))
            {
                _commonAbilityUnlocks[commonAbilityName] = true;
            }
            else
            {
                _commonAbilityUnlocks.Add(commonAbilityName, true);
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

    //public void AddSkill(CardName cardName, int index = -1)
    //{
    //    if (cardName == CardName.None) return;

    //    if (index == -1)
    //    {
    //        for (int i = 0; i < _creatureSkillCount; i++)
    //        {
    //            if (_skillList[i].cardName == CardName.None)
    //            {
    //                index = i;
    //                break;
    //            }

    //        }
    //    }

    //    if (index <= 0 || _skillList.Count <= index) return;

    //    _skillList[index].cardName = cardName;
    //    _skillList[index].skillTime = 0;

    //}

    void HandleSkill()
    {
        foreach (var skill in _skillList)
        {
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
    public void UseSkill(int index)
    {
        
    }
    public void UseSkill1() { UseSkill(0); }
    public void UseSkill2() { UseSkill(1); }
    public void UseSkill3() { UseSkill(2); }
    public void UseSkill4() { UseSkill(3); }
}

[System.Serializable]
public class SkillSlot
{
    public SkillData skillData;
    public float skillCoolTime;
    public float skillTime;
    public bool isActive;
}
