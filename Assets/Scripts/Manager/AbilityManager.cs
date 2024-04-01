
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AbilityManager : ManagerBase
{
    #region 캐릭터 변수
    Character Girl => Managers.GetManager<GameManager>().Girl;
    Player Player => Managers.GetManager<GameManager>().Player;

    Character Creature => Managers.GetManager<GameManager>().Creature;
    CreatureAI CreatureAI => Managers.GetManager<GameManager>().CreatureAI;

    #endregion

    public int AbilityCount { set; get; } = 0;

    // 공용 능력
    Dictionary<CommonAbilityName, bool> _commonAbilityUnlocks = new Dictionary<CommonAbilityName, bool>();

    // 스킬 슬롯
    [SerializeField] int _skillCount = 4;
    [SerializeField] List<SkillSlot> _skillSlotList = new List<SkillSlot>();
    public List<SkillSlot> SkillSlotList => _skillSlotList;
    Dictionary<SkillName, Action<SkillSlot>> _skillDictionary = new Dictionary<SkillName, Action<SkillSlot>>();

    // 검은구체 관련 변수
    List<BlackSphere> _blackSphereList = new List<BlackSphere>();
    public List<BlackSphere> BlackSphereList => _blackSphereList;
    public int MaxBlackSphereCount { set; get; } = 10;
    public Action<BlackSphere> BlackSphereAddedHandler;             // 갯수가 초과 되면 null로 실행됨

    // 검은구체
    float _blackSphereCoolTime = 5;
    float _blackSphereTime;

    // 전기 관련 능력
    float _maxElectricity = 0;
    public float MaxElectricity { get { return _maxElectricity; } }
    float _currentElectricity;
    public float CurrentElectricity => _currentElectricity;
    float _chargeElectricty = 0;
    public float ChargeElectricty{ get { return _chargeElectricty; } }
    public bool IsUnlockOverCharge { set; get; } = false;

    // 포식 관련 능력
    int _maxPredation = 0;
    public int MaxPredation => _maxPredation;
    int _predation = 0;
    public int Predation => _predation;


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

        // 제거된 검은구체 제거
        for (int i = _blackSphereList.Count - 1; i >= 0; i--)
        {
            if (_blackSphereList[i] == null)
                _blackSphereList.RemoveAt(i);
        }
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

    void PlayVolleyFire(SkillSlot slot)
    {
        if (slot.isActive) return;
        if (slot.skillCoolTime > slot.skillTime) return;
        if (_blackSphereList.Count <= 0) return;

        Vector3 mousePosition = Managers.GetManager<InputManager>().MouseWorldPosition;

        BlackSphere blackSphere = _blackSphereList[0];
        _blackSphereList.RemoveAt(0);

        blackSphere.ChangeAttackMode(mousePosition, GetIsHaveCommonAbility(CommonAbilityName.ExplosiveSphere), 0);
    }
    
    public void AddPredation(int value)
    {
        if(_maxPredation < _predation + value)
        {
            _predation = _maxPredation;
        }
        else
        {
            _predation += value;
        }
    }
    public void AddElectricity(float value)
    {
        float maxElectricity = MaxElectricity;
        maxElectricity *= IsUnlockOverCharge ? 2 : 1;
        if (_currentElectricity + value < maxElectricity)
            _currentElectricity += value;
        else
            _currentElectricity = maxElectricity;
    }

    public void ResetElectricity()
    {
        _currentElectricity = 0;
    }
    void ProcessCommonAbility()
    {
        // 전기충전
        if(MaxElectricity != 0)
        {
            float maxElectricity = MaxElectricity;
            maxElectricity *= IsUnlockOverCharge ? 2 : 1;
            if (_currentElectricity + ChargeElectricty * Time.deltaTime > maxElectricity)
                _currentElectricity = maxElectricity;
            else
                _currentElectricity += ChargeElectricty * Time.deltaTime;
        }

        if (GetIsHaveCommonAbility(CommonAbilityName.BlackSphere))
        {
            if (_blackSphereTime < _blackSphereCoolTime)
            {
                _blackSphereTime += Time.deltaTime;

            }
            else
            {
                AddBlackSphere(Player.transform.position);
                _blackSphereTime = 0;
            }
        }

    }
    public void AddBlackSphere(Vector3 position)
    {
        BlackSphere blackSphere = null;
        if (_blackSphereList.Count < MaxBlackSphereCount)
        {
            GameObject go = Managers.GetManager<ResourceManager>().Instantiate("Prefabs/BlackSphere");
            go.transform.position = position;
            blackSphere = go.GetComponent<BlackSphere>();
            blackSphere.Init(Player.Character, new Vector3(-3, 5));
            _blackSphereList.Add(blackSphere);
        }

        BlackSphereAddedHandler?.Invoke(blackSphere);
    }
    // 선택한 카드는 랭크가 올라간 상태
    public void ApplyCardAbility(CardData cardData)
    {
        GirlCardData girlCardData = cardData as GirlCardData;
        CreatureCardData creatureCardData = cardData as CreatureCardData;
        WallCardData wallCardData = cardData as WallCardData;
        CommonCardData commonCardData = cardData as CommonCardData;
        if (girlCardData != null)
        {
      
            if (girlCardData.UnlockAbility != GirlAbilityName.None && !Player.GirlAbility.GetIsHaveAbility(girlCardData.UnlockAbility))
                Player.GirlAbility.AddGirlAbility(girlCardData.UnlockAbility);
            Player.IncreasedAttackSpeedPercentage += girlCardData.DecreaseFireDelayPercentage;
            Player.IncreasedReloadSpeedPercentage += girlCardData.IncreaseReloadSpeedPercentage;
            Player.IncreasedPenerstratingPower += girlCardData.IncreasePenerstratingPower;

            AddSkill(girlCardData);
        }
        if (creatureCardData != null)
        {
            CreatureAI.CreatureAbility.AddAbility(creatureCardData.UnlockAbility);

            AddSkill(creatureCardData);
        }
      
        if (commonCardData != null)
        {
            AddCommonAbility(commonCardData);
            AddSkill(commonCardData);
        }
        AbilityCount++;
    }
    public void AddCommonAbility(CommonCardData cardData)
    {
        CommonAbilityName commonAbilityName = cardData.UnlockAbility;
        if ((int)commonAbilityName > (int)CommonAbilityName.None && (int)commonAbilityName < (int)CommonAbilityName.END)
        {

            if (_commonAbilityUnlocks.ContainsKey(commonAbilityName))
            {
                if (!_commonAbilityUnlocks[commonAbilityName])
                {
                    _commonAbilityUnlocks[commonAbilityName] = true;
                }
            }
            else
            {
                _commonAbilityUnlocks.Add(commonAbilityName, true);
            }

            int rank = Managers.GetManager<GameManager>().GetCardSelectionCount(cardData.CardName);
            float value = 0;
            if (rank == 1) value = cardData.Property1;
            if (rank == 2) value = cardData.Property2;
            if (rank == 3) value = cardData.Property3;

            switch (commonAbilityName)
            {
                case CommonAbilityName.None:
                    break;
                case CommonAbilityName.BlackSphere:
                    _blackSphereCoolTime = value;
                    break;
                case CommonAbilityName.ControlSphere:
                    MaxBlackSphereCount = (int)value;
                    break;
                case CommonAbilityName.VolleyFire:
                    break;
                case CommonAbilityName.ExplosiveSphere:
                    break;
                case CommonAbilityName.Bait:
                    break;
                case CommonAbilityName.MicroPower:
                    _maxElectricity = 100;
                    _chargeElectricty = 1;
                    break;
                case CommonAbilityName.ExtraBattery:
                    _maxElectricity = 200;
                    break;
                case CommonAbilityName.SelfGeneration:
                    _chargeElectricty = 2;
                    break;
                case CommonAbilityName.Appetite:
                    _maxPredation = 40;
                    break;
                case CommonAbilityName.END:
                    break;
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
