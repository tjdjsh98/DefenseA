using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
        _skillDictionary.Add(CardName.비상식량, PlayEmergencyFood);
        _skillDictionary.Add(CardName.앞당김, PlayBringForward);
    }

    void UseSkill(SkillSlot skillSlot)
    {
        if (skillSlot.card == null || skillSlot.card.cardData == null) return;

        if (_skillDictionary.TryGetValue(skillSlot.card.cardData.CardName, out var func))
        {
            func?.Invoke(skillSlot);
        }
    }

    void PlayBringForward(SkillSlot slot) {

        if (slot.isActive) return;
        if (slot.skillCoolTime > slot.skillElapsed) return;

        foreach (var skillSlot in SkillSlotList)
        {
            if (skillSlot.isActive) return;

            if (skillSlot.skillElapsed + 10 < skillSlot.skillCoolTime)
                skillSlot.skillElapsed += 10;
            else
                skillSlot.skillElapsed = skillSlot.skillCoolTime;
        }
    }

    void PlayEmergencyFood(SkillSlot slot)
    {
        if (slot.isActive) return;
        if (slot.skillCoolTime > slot.skillElapsed) return;


        slot.skillElapsed = 0;
        Girl.Hp += (Girl.MaxHp - Girl.Hp);
    }


    
    void HandleCommonAbility()
    {
      

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
             
                //case CardName.일제사격:
                //    if (rank > 0)
                //        BlackSphereAttackPower -= (int)card.cardData.PropertyList[rank - 1];
                //    BlackSphereAttackPower += (int)value;
                //    break;
               
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

                    //case CardName.일제사격:
                    //    if (rank > 0)
                    //        BlackSphereAttackPower += (int)card.cardData.PropertyList[rank - 1];
                    //    BlackSphereAttackPower -= (int)value;
                    //    break;
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
        _skillSlotList[index].skillElapsed = _skillSlotList[index].skillCoolTime;
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
                    _skillSlotList[i].skillElapsed = 0;
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

            if (skill.skillElapsed < skill.skillCoolTime)
            {
                skill.skillElapsed += Time.deltaTime;
            }
            else
            {
                skill.skillElapsed = skill.skillCoolTime;
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
                _skillSlotList[i].skillElapsed = 0;
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

    public override void ManagerDestroy()
    {
    }
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
    public float skillElapsed;
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