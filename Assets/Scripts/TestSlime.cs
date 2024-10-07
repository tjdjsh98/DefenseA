using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestSlime : MonoBehaviour
{
    Character _character;
    EnemyAI _enemyAI;

    [SerializeField] CharacterPart _characterPart;
    int _partAccumulatedDamage;
    int _sequence;
    private void Awake()
    {
        _character = GetComponent<Character>();
        _enemyAI = GetComponent<EnemyAI>();

        _characterPart.CharacterPartDamagedHandler += (c, dmg, pw, point, stun) =>
        {
            _partAccumulatedDamage += dmg;
            if (_partAccumulatedDamage > _character.MaxHp / 10)
            {
                _character.SetAnimatorTrigger("Reset");
                _sequence = 0;
                _character.IsEnableTurn = true;
                _character.IsEnableMove = true;
                _character.IsAttack = true;
                _partAccumulatedDamage = 0;
            }
        };
    }
    void Update()
    {
        if (_sequence == 0)
        {
            if (_enemyAI.IsTargetInRange)
            {
                _sequence++;
            }
        }
        if (_sequence == 1)
        {
            _character.IsEnableTurn = false;
            _character.IsEnableMove = false;
            _character.IsAttack = true;
            _character.SetAnimatorTrigger("AttackReady");
        }
        if (_sequence == 2)
        {
            _character.SetAnimatorTrigger("Attack");
        }
        if (_sequence == 3)
        {
            _sequence = 0;
        }
    }

    public void NextSequence()
    {
        _sequence++;
    }
}
