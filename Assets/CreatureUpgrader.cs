using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatureUpgrader : MonoBehaviour
{
    bool _isFinish;

    public void IncreaseCreatureAttackPower()
    {
        Managers.GetManager<GameManager>().Creature.AttackPower += 10;
        _isFinish = true;
    }
    public void IncreaseCreatureMaxHp()
    {
        Managers.GetManager<GameManager>().Creature.SetHp(Managers.GetManager<GameManager>().Creature.MaxHp + 50);
        _isFinish = true;
    }
    public void IncreaseCreatureAttackSpeed()
    {
        Managers.GetManager<GameManager>().CreatureAI.CreatureAbility.IncreasedAttackSpeedPercentage += 20f;
        _isFinish = true;
    }
    public void IncreaseCreatureHpGegeneration()
    {
        Managers.GetManager<GameManager>().Creature.IncreasedHpRegeneration+= 5f;
        _isFinish = true;
    }
}
