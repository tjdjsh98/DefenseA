
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class CardManager : ManagerBase
{
    #region 캐릭터 변수
    Character Girl => Managers.GetManager<GameManager>().Girl;
    Player Player => Managers.GetManager<GameManager>().Player;

    Character Creature => Managers.GetManager<GameManager>().Creature;
    CreatureAI CreatureAI => Managers.GetManager<GameManager>().CreatureAI;

    #endregion

    [Header("카드 선택지")]
    Dictionary<CardName, Card> _remainCardSelectionDic = new Dictionary<CardName, Card>();
    Dictionary<CardName, Card> _possessCardDic = new Dictionary<CardName, Card>();
    List<PriorCard> _priorCardList = new List<PriorCard>();

    public int AbilityCount { set; get; } = 0;

    // 스킬 슬롯
    [SerializeField] int _skillCount = 4;
    [SerializeField] List<SkillSlot> _skillSlotList = new List<SkillSlot>();
    public List<SkillSlot> SkillSlotList => _skillSlotList;
    Dictionary<CardName, Action<SkillSlot>> _skillDictionary = new Dictionary<CardName, Action<SkillSlot>>();

    // 검은구체 관련 변수
    List<BlackSphere> _blackSphereList = new List<BlackSphere>();
    public List<BlackSphere> BlackSphereList => _blackSphereList;
    public int MaxBlackSphereCount { set; get; } = 0;
    public Action<BlackSphere> BlackSphereAddedHandler;             // 갯수가 초과 되면 null로 실행됨
    float _blackSphereCoolTime = 0;
    float _blackSphereTime;
    public int BlackSphereAttackPower { set; get; }

    // 전기 관련 능력
    float _maxElectricity = 0;
    public float MaxElectricity { get { return _maxElectricity; } }
    float _currentElectricity;
    public float CurrentElectricity { get { return _currentElectricity; } set { _currentElectricity = Mathf.Clamp(value, 0, _maxElectricity); } }
    public float ChargeElectricty { get; set; }
    public bool IsUnlockOverCharge { set; get; } = false;

    // 포식 관련 능력
    int _maxPredation = 0;
    public int MaxPredation => _maxPredation;

    int _predation;
    public int Predation { get { return _predation; }set { _predation = Mathf.Clamp(value, 0, _maxPredation); } }

    public int HuntingPredation { set; get; }
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
                prior.card = new Card() { cardData = data, rank = -1 };
                foreach (var cardData in data.PriorCards)
                    prior.priorCardDataList.Add(cardData);
                _priorCardList.Add(prior);
                return false;
            }
        });
        foreach (var cardData in cardDatas)
        {
            _remainCardSelectionDic.Add(cardData.CardName, new Card() { cardData = cardData, rank = -1 });
        }
    }

    public override void ManagerUpdate()
    {
        HandleSkill();
        HandleCommonAbility();

        // 제거된 검은구체 제거
        for (int i = _blackSphereList.Count - 1; i >= 0; i--)
        {
            if (_blackSphereList[i] == null)
                _blackSphereList.RemoveAt(i);
        }
    }

    #region 카드 관련 함수

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
    // 카드를 선택하여 능력치 추가

   
    public void AddCard(Card card)
    {
        // 소유중인 카드에 없을 때 추가
        if (!_possessCardDic.ContainsKey(card.cardData.CardName))
        {
            _possessCardDic.Add(card.cardData.CardName, card);
            card.rank = 0;
        }
        else
        {
            card.rank++;
        }

        // 선행카드가 모두 만족되었다면 남은카드에 추가
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
        // 업그레이드가 모두 완료 시 남은 카드에서 삭제
        if (card.cardData.MaxUpgradeCount <= card.rank)
        {
            _remainCardSelectionDic.Remove(card.cardData.CardName);
        }
        ApplyCardAbility(card);
    }
    public void RemoveCard(Card card)
    {
        if (!_possessCardDic.ContainsKey(card.cardData.CardName)) return;

        RevertCardAbility(card);

        card.rank--;

        if (card.rank < 0)
        {
            card.rank = 0;
            _possessCardDic.Remove(card.cardData.CardName);
            if(!_remainCardSelectionDic.ContainsKey(card.cardData.CardName))
                _remainCardSelectionDic.Add(card.cardData.CardName, card);

        }
    }
    public bool RemoveRandomCard()
    {
        if (_possessCardDic.Count <= 0) return false;

        RemoveCard(_possessCardDic.Values.ToList().GetRandom());

        return true;
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
        _skillDictionary.Add(CardName.일제사격, PlayVolleyFire);
        _skillDictionary.Add(CardName.미끼, PlayBait);
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
            blackSphere.ChangeAttackMode(mousePosition,BlackSphereAttackPower, GetIsHaveAbility(CardName.폭발성구체), i*0.1f);
        }
        slot.skillTime = 0;
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

    void HandleCommonAbility()
    {
        // 전기충전
        if (MaxElectricity != 0)
        {
            float maxElectricity = MaxElectricity;
            maxElectricity *= IsUnlockOverCharge ? 2 : 1;
            if (CurrentElectricity + ChargeElectricty * Time.deltaTime > maxElectricity)
                CurrentElectricity = maxElectricity;
            else
                CurrentElectricity += ChargeElectricty * Time.deltaTime;
        }

        if (GetIsHaveAbility(CardName.검은구체))
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
    public void ApplyCardAbility(Card card)
    {
        Player.GirlAbility.ApplyCardAbility(card);
        CreatureAI.CreatureAbility.ApplyCardAbility(card);
        ApplyCommonAbility(card);
        AddSkill(card);
        AbilityCount++;
    }

    // 랭크가 내려가지 않은 상태로 들어온다.
    public void RevertCardAbility(Card card)
    {
        AbilityCount--;
        RemoveSkill(card);
        RevertCommonAbility(card);
        Player.GirlAbility.RevertCardAbility(card);

        CreatureAI.CreatureAbility.RevertCardAbility(card);
    }

    public void ApplyCommonAbility(Card card)
    {
        if (card != null && card.cardData != null)
        {
            int rank = card.rank;
            float value = 0;
            if (rank < card.cardData.PropertyList.Count)
                value = card.cardData.PropertyList[rank];
            float value2 = 0;
            if(rank < card.cardData.Property2List.Count)
                value2 = card.cardData.Property2List[rank];

            switch (card.cardData.CardName)
            {
                case CardName.검은구체:
                    if (rank > 0)
                    {
                        _blackSphereCoolTime -= card.cardData.PropertyList[rank - 1];
                        MaxBlackSphereCount -= (int)card.cardData.Property2List[rank - 1];
                    }
                    _blackSphereCoolTime += value;
                    MaxBlackSphereCount += (int)value2;
                    break;
                case CardName.일제사격:
                    if (rank > 0)
                        BlackSphereAttackPower -= (int)card.cardData.PropertyList[rank - 1];
                    BlackSphereAttackPower += (int)value;
                    break;
                case CardName.미세전력:
                    if (rank > 0)
                    {
                        _maxElectricity -= card.cardData.PropertyList[rank - 1];
                        ChargeElectricty -= (int)card.cardData.Property2List[rank - 1];
                    }
                    _maxElectricity += value;
                    ChargeElectricty += value2;
                    break;
                case CardName.식욕:
                    if (rank > 0)
                    {
                        _maxPredation -= (int)card.cardData.PropertyList[rank - 1];
                        HuntingPredation -= (int)card.cardData.Property2List[rank - 1];
                    }
                    _maxPredation += (int)value;
                    HuntingPredation += (int)value2;
                    break;
            }
        }
    }
    public void RevertCommonAbility(Card card)
    {
        if (card != null && card.cardData != null)
        {
            int rank = card.rank;
            if (rank >= 0)
            {
                float value = 0;
                if (rank < card.cardData.PropertyList.Count)
                    value = card.cardData.PropertyList[rank];
                float value2 = 0;
                if (rank < card.cardData.Property2List.Count)
                    value2 = card.cardData.Property2List[rank];

                switch (card.cardData.CardName)
                {
                    case CardName.검은구체:
                        if (rank > 0)
                        {
                            _blackSphereCoolTime += card.cardData.PropertyList[rank - 1];
                            MaxBlackSphereCount += (int)card.cardData.Property2List[rank - 1];
                        }
                        _blackSphereCoolTime -= value;
                        MaxBlackSphereCount -= (int)value2;
                        break;
                    case CardName.일제사격:
                        if (rank > 0)
                            BlackSphereAttackPower += (int)card.cardData.PropertyList[rank - 1];
                        BlackSphereAttackPower -= (int)value;
                        break;
                    case CardName.미세전력:
                        if (rank > 0)
                        {
                            _maxElectricity += card.cardData.PropertyList[rank - 1];
                            ChargeElectricty += card.cardData.Property2List[rank - 1];
                        }
                        _maxElectricity -= value;
                        ChargeElectricty -= value2 ;
                        break;
                    case CardName.식욕:
                        if (rank > 0)
                        {
                            _maxPredation += (int)card.cardData.PropertyList[rank - 1];
                            HuntingPredation += (int)card.cardData.Property2List[rank - 1];
                        }
                        _maxPredation -= (int)value;
                        HuntingPredation -= (int)value2;
                        break;
                }
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
    // 랭크가 내려가기 전의 카드 정보
    public void RemoveSkill(Card card)
    {
        if (card == null || card.cardData == null || !card.cardData.IsActiveAbility) return;
        if(card.rank == 0)
        {
            for (int i = 0; i < _skillCount; i++)
            {
                if (_skillSlotList[i].card == card)
                {
                    _skillSlotList[i].card= null;
                    _skillSlotList[i].skillTime = 0;
                    _skillSlotList[i].isActive = false;
                    break;
                }

            }
        }
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
    public int rank = -1;
    public float Property
    {
        get
        {
            if (cardData == null || cardData.PropertyList.Count <= rank)
                return 0;

            return cardData.PropertyList[rank];
        }
    }
    public float Property2
    {
        get
        {
            if (cardData == null || cardData.Property2List.Count <= rank)
                return 0;

            return cardData.Property2List[rank];
        }
    }
}