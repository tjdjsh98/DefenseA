using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OctoHeadAttackPattern : MonoBehaviour
{
    Character _character;
    EnemyAI _enemyAI;

    int _sequence = 0;

    [SerializeField] float _attackCoolTime = 5;
    [SerializeField] float _attackTime = 0;

    [SerializeField] float _attack1Delay = 2;
    [SerializeField] float _attack1AfterDelay = 4;

    private void Awake()
    {
        _character = GetComponent<Character>();
        _enemyAI = GetComponent<EnemyAI>();
    }

    private void Update()
    {

        if (_sequence == 0)
        {
            if (_attackTime > _attackCoolTime)
            {
                if (_enemyAI.Target)
                {
                    _attackTime = 0;
                    Vector3? position = Managers.GetManager<GameManager>().GetGroundTop(_enemyAI.Target.transform.position);
                    if (position.HasValue)
                    {
                        _character.IsEnableMove = false;
                        _character.IsEnableTurn = false;
                        _character.AnimatorSetTrigger("Attack1Ready");
                        _sequence = 1;
                        GameObject go = Managers.GetManager<ResourceManager>().Instantiate("Prefabs/AttackPattern/TentacleAttackPattern");
                        AttackPattern tentacle = go.GetComponent<AttackPattern>();
                        tentacle.transform.position = position.Value;
                        tentacle.StartPattern(_character, _attack1Delay + 0.2f, Define.CharacterType.Player, _character.AttackPower, 10, Vector3.up, 1);
                    }
                }
            }
            else
            {
                _attackTime += Time.deltaTime;
            }
        }
        AttackPattern1();

    }

    void AttackPattern1()
    {
        if (_sequence == 1)
        {
            _attackTime += Time.deltaTime;
            if (_attack1Delay < _attackTime)
            {
                _sequence = 2;
                _attackTime = 0;
                _character.AnimatorSetTrigger("Attack1");
            }
        }
        else if(_sequence == 2)
        {
            _attackTime += Time.deltaTime;
            if (_attack1AfterDelay < _attackTime)
            {
                _sequence = 0;
                _attackTime = 0;
                _enemyAI.ResetTarget();
                _character.IsEnableMove = true;
                _character.IsEnableTurn = true;
            }
        }
    }
}
