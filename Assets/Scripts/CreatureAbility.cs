using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class CreatureAbility
{
    CreatureAI _creatureAI;

    // �ɷ� �ر�
    Dictionary<CreatureAbilityName, bool> _abilityUnlocks = new Dictionary<CreatureAbilityName, bool>();

    //��������
    [SerializeField] bool _debugSurvivalIntinctRange;
    [SerializeField] Define.Range _survivalIntinctRange;
    [SerializeField] int _survivalIntinctCount;
    [SerializeField] float _survivalIntinctElapsed;

    public void Init(CreatureAI creatureAI)
    {
        _creatureAI = creatureAI;
        _creatureAI.Character.AttackHandler += OnAttack;

    }


    public void AbilityUpdate()
    {
        SurvivalInstinct();
    }

    void OnAttack(Character target, int damage)
    {
        if (GetIsHaveAbility(CreatureAbilityName.FlowingBlack))
        {
            if(Random.Range(0,5) == 0)
            {
                Managers.GetManager<AbilityManager>().AddBlackSphere(target.transform.position);
            }
        }
        if (GetIsHaveAbility(CreatureAbilityName.DiningEtiquette))
        {
            if(target == null || target.IsDead)
            {
                Managers.GetManager<AbilityManager>().AddPredation(3);
            }
        }
        if (GetIsHaveAbility(CreatureAbilityName.Charge))
        {
            if (target == null || target.IsDead)
            {
                Managers.GetManager<AbilityManager>().AddElectricity(1);
            }
        }
    }
    public void AddAbility(CreatureAbilityName creatureAbilityName)
    {
        if ((int)creatureAbilityName > (int)CreatureAbilityName.None && (int)creatureAbilityName < (int)CreatureAbilityName.END)
        {
            bool turnTrue = false;
            if (_abilityUnlocks.ContainsKey(creatureAbilityName))
            {
                if (!_abilityUnlocks[creatureAbilityName])
                {
                    turnTrue = true;
                    _abilityUnlocks[creatureAbilityName] = true;
                }
            }
            else
            {
                turnTrue = true;
                _abilityUnlocks.Add(creatureAbilityName, true);
            }

            if (turnTrue)
            {

            }
        }
    }
    public bool GetIsHaveAbility(CreatureAbilityName ability)
    {
        if (_abilityUnlocks.TryGetValue(ability, out bool value) && value)
            return value;

        return false;
    }
    void SurvivalInstinct()
    {
        if (GetIsHaveAbility(CreatureAbilityName.SurvialIntinct))
        {

            _survivalIntinctElapsed += Time.deltaTime;
            if (_survivalIntinctElapsed > 2)
            {
                _survivalIntinctElapsed = 0;
                List<RaycastHit2D> hits = Util.RangeCastAll2D(_creatureAI.gameObject, _survivalIntinctRange, LayerMask.GetMask("Character"));

                _survivalIntinctCount = 0;
                foreach (var hit in hits)
                {
                    Character character = hit.collider.GetComponent<Character>();
                    if (character)
                    {
                        _survivalIntinctCount++;
                    }
                }
            }
        }
    }
    public float GetIncreasedAttackSpeed()
    {
        float percentage = 0;
        AbilityManager abilityManager = Managers.GetManager<AbilityManager>();

        if (GetIsHaveAbility(CreatureAbilityName.CurrentPassing))
        {
            percentage += (int)(abilityManager.CurrentElectricity/10) * 5f;
        }

        return percentage;
    }
    public List<CreatureAbilityName> GetHaveAbilityNameList()
    {
        List<CreatureAbilityName> list = _abilityUnlocks.Keys.ToList();

        return list;
    }
}
