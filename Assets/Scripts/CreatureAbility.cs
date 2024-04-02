using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class CreatureAbility
{
    CreatureAI _creatureAI;

    //��������
    [SerializeField] Define.Range _survivalIntinctRange;
    [SerializeField] int _survivalIntinctCount;
    [SerializeField] float _survivalIntinctElapsed;

    public void Init(CreatureAI creatureAI)
    {
        _creatureAI = creatureAI;
        _creatureAI.Character.AttackHandler += OnAttack;
        _creatureAI.Character.CharacterDamagedHandler += OnDamage;
    }

  
    public void AbilityUpdate()
    {
        SurvivalInstinct();

        _creatureAI.Character.IncreasedHpRegeneration = GetHpRegeneration();
    }

    void OnAttack(Character target, int damage)
    {
        CardManager manager = Managers.GetManager<CardManager>();
        if (GetIsHaveAbility(CardName.�Ŀ�))
        {
            if (target == null || target.IsDead)
            {
                Managers.GetManager<CardManager>().AddPredation(1);
            }
        }
        if (GetIsHaveAbility(CardName.�Ļ翹��))
        {
            if(target == null || target.IsDead)
            {
                Managers.GetManager<CardManager>().AddPredation(3);
            }
        }
        if (GetIsHaveAbility(CardName.����))
        {
            Card card = manager.GetCard(CardName.����);
            if (card != null)
            {
                Managers.GetManager<CardManager>().AddElectricity(card.property);
            }
        }
    }

    void OnDamage(Character attacker, int damage, float power, Vector3 direction, Vector3 point, float stunTime)
    {
        CardManager manager = Managers.GetManager<CardManager>();
        if (GetIsHaveAbility(CardName.�˰��帣��))
        {
            Card card = manager.GetCard(CardName.�˰��帣��);
            if (card != null)
            {
                if (Random.Range(0, 100) < card.property)
                {
                    manager.AddBlackSphere(_creatureAI.Character.GetCenter());
                }

            }
        }
    }
    public void ApplyCardAbility(Card card)
    {
        switch (card.cardData.CardName)
        {

        }
    }
    public bool GetIsHaveAbility(CardName cardName)
    {
         Card card = Managers.GetManager<CardManager>().GetCard(cardName);
        
        return card != null;
    }
    void SurvivalInstinct()
    {
        if (GetIsHaveAbility(CardName.��������))
        {
            _survivalIntinctElapsed += Time.deltaTime;
            //  2�ʸ��� ����
            if (_survivalIntinctElapsed > 2)
            {
                _survivalIntinctElapsed = 0;
                List<RaycastHit2D> hits = Util.RangeCastAll2D(_creatureAI.gameObject, _survivalIntinctRange, LayerMask.GetMask("Character"));

                _survivalIntinctCount = 0;
                foreach (var hit in hits)
                {
                    Character character = hit.collider.GetComponent<Character>();
                    if (character && character.CharacterType == Define.CharacterType.Enemy)
                    {
                        _survivalIntinctCount++;
                    }
                }
            }
        }
    }
    public float GetIncreasedAttackPowerPercentage()
    {
        float percentage = 0;
        CardManager cardManager = Managers.GetManager<CardManager>();

        if (GetIsHaveAbility(CardName.�г�))
        {
            percentage += 50;
        }


        return percentage;
    }
    public float GetIncreasedAttackSpeedPercentage()
    {
        float percentage = 0;
        CardManager cardManager = Managers.GetManager<CardManager>();

        if (GetIsHaveAbility(CardName.�г�))
        {
            percentage += 50;
        }


        return percentage;
    }
    public float GetHpRegeneration()
    {
        float regen = 0;
        if (GetIsHaveAbility(CardName.��������))
        {
            regen = _survivalIntinctCount * Managers.GetManager<CardManager>().GetCard(CardName.��������).property;
        }

        return regen;
    }
}
