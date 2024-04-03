
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class CardManager : ManagerBase
{
    #region ĳ���� ����
    Character Girl => Managers.GetManager<GameManager>().Girl;
    Player Player => Managers.GetManager<GameManager>().Player;

    Character Creature => Managers.GetManager<GameManager>().Creature;
    CreatureAI CreatureAI => Managers.GetManager<GameManager>().CreatureAI;

    #endregion

    [Header("ī�� ������")]
    Dictionary<CardName, Card> _remainCardSelectionDic = new Dictionary<CardName, Card>();
    Dictionary<CardName, Card> _possessCardDic = new Dictionary<CardName, Card>();
    List<PriorCard> _priorCardList = new List<PriorCard>();


    public int AbilityCount { set; get; } = 0;

    // ��ų ����
    [SerializeField] int _skillCount = 4;
    [SerializeField] List<SkillSlot> _skillSlotList = new List<SkillSlot>();
    public List<SkillSlot> SkillSlotList => _skillSlotList;
    Dictionary<CardName, Action<SkillSlot>> _skillDictionary = new Dictionary<CardName, Action<SkillSlot>>();

    // ������ü ���� ����
    List<BlackSphere> _blackSphereList = new List<BlackSphere>();
    public List<BlackSphere> BlackSphereList => _blackSphereList;
    public int MaxBlackSphereCount { set; get; } = 10;
    public Action<BlackSphere> BlackSphereAddedHandler;             // ������ �ʰ� �Ǹ� null�� �����
    float _blackSphereCoolTime = 5;
    float _blackSphereTime;
    int _blackSphereAttackPower;

    // ���� ���� �ɷ�
    float _maxElectricity = 0;
    public float MaxElectricity { get { return _maxElectricity; } }
    float _currentElectricity;
    public float CurrentElectricity => _currentElectricity;
    float _chargeElectricty = 0;
    public float ChargeElectricty { get { return _chargeElectricty; } }
    public bool IsUnlockOverCharge { set; get; } = false;

    // ���� ���� �ɷ�
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
        for (int i = 0; i < _skillCount; i++)
        {
            _skillSlotList.Add(new SkillSlot());
        }
        List<CardData> cardDatas = Managers.GetManager<DataManager>().GetDataList<CardData>((data) =>
        {
            if (data.IsStartCard)
            {
                return true;
            }
            else
            {
                PriorCard prior = new PriorCard();
                prior.card = new Card() { cardData = data, rank = 0 };
                foreach (var cardData in data.PriorCards)
                    prior.priorCardDataList.Add(cardData);
                _priorCardList.Add(prior);
                return false;
            }
        });
        foreach (var cardData in cardDatas)
        {
            _remainCardSelectionDic.Add(cardData.CardName, new Card() { cardData = cardData, rank = 0 });
        }
    }

    public override void ManagerUpdate()
    {
        HandleSkill();
        ProcessCommonAbility();

        // ���ŵ� ������ü ����
        for (int i = _blackSphereList.Count - 1; i >= 0; i--)
        {
            if (_blackSphereList[i] == null)
                _blackSphereList.RemoveAt(i);
        }
    }

    #region ī�� ���� �Լ�

    public Card GetRandomCardSelectionData()
    {
        return _remainCardSelectionDic.Values.ToList().GetRandom();
    }
    public List<Card> GetRandomCardSelectionData(int count)
    {
        return _remainCardSelectionDic.Values.ToList().GetRandom(count);
    }
    public List<Card> GetRemainCardSelection()
    {
        return _remainCardSelectionDic.Values.ToList();
    }
    // ī�带 �����Ͽ� �ɷ�ġ �߰�
    public void SelectCard(Card card)
    {

        // �������� ī�忡 ���� �� �߰�
        if (!_possessCardDic.ContainsKey(card.cardData.CardName))
        {
            _possessCardDic.Add(card.cardData.CardName, card);
        }

        card.rank++;

        // ����ī�尡 ��� �����Ǿ��ٸ� ����ī�忡 �߰�
        for (int i = _priorCardList.Count - 1; i >= 0; i--)
        {
            bool success = true;
            foreach (var priorCardData in _priorCardList[i].priorCardDataList)
            {
                if (!_possessCardDic.ContainsKey(priorCardData.priorCardName))
                {
                    success = false;
                    break;
                }
                if (_possessCardDic[priorCardData.priorCardName].rank < priorCardData.priorUpgradeCount)
                {
                    success = false;
                    break;
                }
            }
            if (success)
            {
                _remainCardSelectionDic.Add(_priorCardList[i].card.cardData.CardName, _priorCardList[i].card);
                _priorCardList.Remove(_priorCardList[i]);
            }
        }
        // ���׷��̵尡 ��� �Ϸ� �� ���� ī�忡�� ����
        if (card.cardData.MaxUpgradeCount <= card.rank)
        {
            _remainCardSelectionDic.Remove(card.cardData.CardName);
        }

        // TODO
        // �ɷ�����

        if (card.cardData.CardSelectionType == Define.CardType.Weapon)
        {
            WeaponCardSelection weaponCardSelection = card.cardData as WeaponCardSelection;

            WeaponSwaper swaper = Player.GetComponent<WeaponSwaper>();

            swaper.ChangeNewWeapon(weaponCardSelection.WeaponSlotIndex, weaponCardSelection.WeaponName);
        }
        else
        {
            Managers.GetManager<CardManager>().ApplyCardAbility(card);
        }
    }
    public Card GetCard(CardName cardname)
    {
        if (_possessCardDic.TryGetValue(cardname, out Card card))
        {
            return card;    
        }
        return null;
    }

    public List<Card> GetPossessCardList()
    {
        return _possessCardDic.Values.ToList();
    }
    #endregion

    void RegistSkill()
    {
        _skillDictionary.Add(CardName.�������, PlayVolleyFire);
        _skillDictionary.Add(CardName.�̳�, PlayBait);
    }

    void UseSkill(SkillSlot skillSlot)
    {
        if (skillSlot.card == null || skillSlot.card.cardData == null) return;

        if (_skillDictionary.TryGetValue(skillSlot.card.cardData.CardName, out var func))
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

        for(int i= _blackSphereList.Count-1; i >= 0; i--)
        {
            BlackSphere blackSphere = _blackSphereList[i];
            _blackSphereList.RemoveAt(i);
            blackSphere.ChangeAttackMode(mousePosition,_blackSphereAttackPower, GetIsHaveAbility(CardName.���߼���ü), i*0.1f);
        }
    }

    void PlayBait(SkillSlot slot)
    {
        Debug.Log(slot.card.cardData.PropertyList[slot.card.rank - 1]);
        if (slot.isActive) return;
        if (slot.skillCoolTime > slot.skillTime) return;

        StartCoroutine(CorPlayBait(slot));
    }

    IEnumerator CorPlayBait(SkillSlot slot)
    {
        slot.isActive = true;

        Vector3 mousePosition = Managers.GetManager<InputManager>().MouseWorldPosition;
        Vector3? top = Managers.GetManager<GameManager>().GetGroundTop(mousePosition);
        List<BlackSphere> useBlackSpheres = new List<BlackSphere>();
        if (top.HasValue)
        {
            int count = (((int)slot.card.cardData.PropertyList[slot.card.rank - 1] > _blackSphereList.Count) ?  _blackSphereList.Count: (int)slot.card.cardData.PropertyList[slot.card.rank - 1]);

            for (int i = 0; i < count; i++)
            {
                BlackSphere blackSphere = _blackSphereList[0];
                _blackSphereList.RemoveAt(0);
                blackSphere.MoveToDestination(top.Value + Vector3.up, 1, false);
                yield return new WaitForSeconds(0.05f);
                useBlackSpheres.Add(blackSphere);
            }

            yield return new WaitForSeconds(1.5f);


            foreach (var item in useBlackSpheres)
            {
                item.Destroy();
            }


            Effect effect = Managers.GetManager<ResourceManager>().Instantiate<Effect>((int)Define.EffectName.BlackWhale);
            effect.SetAttackProperty(Girl, 10 + (count-1)* 3, 50, 0.2f, Define.CharacterType.Enemy);
            effect.SetMultiflySize(1+ count * 0.3f);
            effect.Play(top.Value);
        }

        yield return new WaitForSeconds(1f);

        slot.isActive = false;
        slot.skillTime = 0;
    }

    public void AddPredation(int value)
    {
        if (_maxPredation < _predation + value)
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
        // ��������
        if (MaxElectricity != 0)
        {
            float maxElectricity = MaxElectricity;
            maxElectricity *= IsUnlockOverCharge ? 2 : 1;
            if (_currentElectricity + ChargeElectricty * Time.deltaTime > maxElectricity)
                _currentElectricity = maxElectricity;
            else
                _currentElectricity += ChargeElectricty * Time.deltaTime;
        }

        if (GetIsHaveAbility(CardName.������ü))
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

    // ������ ī��� ��ũ�� �ö� ����
    public void ApplyCardAbility(Card card)
    {
        Player.GirlAbility.ApplyCardAbility(card);
        CreatureAI.CreatureAbility.ApplyCardAbility(card);
        ApplyCommonAbility(card);
        AddSkill(card);
        AbilityCount++;
    }

    public void ApplyCommonAbility(Card card)
    {
        if (card != null && card.cardData != null)
        {
            int rank = card.rank;
            float value = card.cardData.PropertyList[rank - 1];

            switch (card.cardData.CardName)
            {
                case CardName.������ü:
                    _blackSphereCoolTime = value;
                    break;
                case CardName.��ü����:
                    MaxBlackSphereCount = (int)value;
                    break;
                case CardName.�������:
                    _blackSphereAttackPower = (int)value;
                    break;
                case CardName.�̼�����:
                    _maxElectricity = 100;
                    _chargeElectricty += 0.1f;
                    break;
                case CardName.�߰����͸�:
                    _maxElectricity += value;
                    break;
                case CardName.�ڰ�����:
                    _chargeElectricty += value;
                    break;
                case CardName.�Ŀ�:
                    _maxPredation= (int)value;
                    break;
            }
        }
    }
    public bool GetIsHaveAbility(CardName cardName)
    {
        Card card = GetCard(cardName);

        return card != null;
    }

    public void AddSkill(Card card, int index = -1)
    {
        if (card == null||card.cardData == null || !card.cardData.IsActiveAbility) return;


        if (index == -1)
        {
            for (int i = 0; i < _skillCount; i++)
            {
                if (_skillSlotList[i].card == null)
                {
                    index = i;
                    break;
                }

            }
        }


        if (index < 0 || _skillSlotList.Count <= index) return;

        _skillSlotList[index].card = card;
        _skillSlotList[index].skillTime = 0;
        _skillSlotList[index].isActive = false;

    }
    
    void HandleSkill()
    {
        foreach (var skill in _skillSlotList)
        {
            if (skill.card == null || skill.card.cardData == null) continue;
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

    public void ResetSkillTime(CardName cardName)
    {
        for(int i =0; i < _skillSlotList.Count; i++)
        {
            if (_skillSlotList[i].card == null) continue;
            if (_skillSlotList[i].card.cardData.CardName == cardName)
            {
                _skillSlotList[i].skillTime = 0;
                return;
            }
        }
    }
    void UseSkill(int index)
    {
        if (_skillSlotList[index].card == null|| _skillSlotList[index].card.cardData == null) return;

        Player.GirlAbility.UseSkill(_skillSlotList[index]);
        CreatureAI.CreatureAbility.UseSkill(_skillSlotList[index]);
        UseSkill(_skillSlotList[index]);

    }
    public void UseSkill1() { UseSkill(0); }
    public void UseSkill2() { UseSkill(1); }
    public void UseSkill3() { UseSkill(2); }
    public void UseSkill4() { UseSkill(3); }
}

[System.Serializable]
public class SkillSlot
{
    public Card card;
    public float skillCoolTime
    {
        get
        {
            if(card != null && card.cardData != null)
            {
                if (card.cardData.coolTimeList.Count == 0) return 99999;
                if (card.cardData.coolTimeList.Count <= card.rank)
                {
                    return card.cardData.coolTimeList[card.cardData.coolTimeList.Count - 1];
                }
                return card.cardData.coolTimeList[card.rank];
            }
            return 99999;
        }
    } 
    public float skillTime;
    public bool isActive;
}

[System.Serializable]
public class PriorCard
{
    public Card card;
    public List<PriorCardData> priorCardDataList = new List<PriorCardData>();
}

[System.Serializable]
public class Card
{
    public CardData cardData;
    public int rank = 0;
    public float property
    {
        get
        {
            if (cardData == null || cardData.PropertyList.Count < rank)
                return 0;

            return cardData.PropertyList[rank - 1];
        }
    }
}