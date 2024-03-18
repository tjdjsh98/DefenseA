using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[System.Serializable]
public class CreatureAbility
{
    CreatureAI _creatureAI;

    // 능력 해금
    Dictionary<CreatureAbilityName, bool> _abilityUnlocks = new Dictionary<CreatureAbilityName, bool>();


    //생존본능
    [SerializeField] bool _debugSurvivalIntinctRange;
    [SerializeField] Define.Range _survivalIntinctRange;
    [SerializeField] int _survivalIntinctCount;
    [SerializeField] float _survivalIntinctElapsed;

    public void Init(CreatureAI creatureAI)
    {
        _creatureAI = creatureAI;
        _creatureAI.Character.AttackHandler += OnAttack;
    }

    public void OnDrawGizmosSelected()
    {
        Util.DrawRangeOnGizmos(_creatureAI.gameObject, _survivalIntinctRange, Color.green);
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
                Managers.GetManager<GameManager>().Player.GirlAbility.AddBlackSphere(target.transform.position);
            }
        }
    }

    public void AddAbility(CreatureAbilityName creatureAbilityName)
    {
        if ((int)creatureAbilityName > (int)CreatureAbilityName.None && (int)creatureAbilityName < (int)CreatureAbilityName.END)
        {
            if (_abilityUnlocks.ContainsKey(creatureAbilityName))
            {
                _abilityUnlocks[creatureAbilityName] = true;
            }
            else
            {
                _abilityUnlocks.Add(creatureAbilityName, true);
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
                GameObject[] gameObjects = Util.RangeCastAll2D(_creatureAI.gameObject, _survivalIntinctRange, LayerMask.GetMask("Character"));

                _survivalIntinctCount = 0;
                foreach (var gameObject in gameObjects)
                {
                    Character character = gameObject.GetComponent<Character>();
                    if (character)
                    {
                        _survivalIntinctCount++;
                    }
                }
            }
        }

    }
}
