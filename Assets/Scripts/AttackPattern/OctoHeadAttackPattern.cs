using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.TextCore.Text;

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
                    if (Random.Range(0, 2) == 0)
                        _sequence = 1;
                    else
                        _sequence = 11;
                }
            }
            else
            {
                _attackTime += Time.deltaTime;
            }
        }
        AttackPattern1();
        SummonPattern();
    }

    void AttackPattern1()
    {
        if (_sequence == 1)
        {
            int random = Random.Range(0,2);
            for (int i = 0; i < 10; i++) 
            {
                Vector3 pos = transform.position + new Vector3(transform.localScale.x * 10*(i+random),0,0);
                Vector3? position = Managers.GetManager<GameManager>().GetGroundTop(pos);
                if (position.HasValue)
                {
                    _character.IsEnableMove = false;
                    _character.IsEnableTurn = false;
                    _character.AnimatorSetTrigger("Attack1Ready");
                    GameObject go = Managers.GetManager<ResourceManager>().Instantiate("Prefabs/AttackPattern/TentacleAttackPattern");
                    AttackPattern tentacle = go.GetComponent<AttackPattern>();
                    tentacle.transform.position = position.Value;
                    tentacle.StartPattern(_character, _attack1Delay + 0.4f*i, Define.CharacterType.Player, _character.AttackPower, 10, Vector3.up, 1);
                    _sequence = 2;
                    _attackTime = 0;
                }
            }
        }
        else if (_sequence == 2)
        {
            _attackTime += Time.deltaTime;
            if (_attack1Delay < _attackTime)
            {
                _sequence = 3;
                _character.AnimatorSetTrigger("Attack1");
               
            }
        }
        else if (_sequence == 3)
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

    void SummonPattern()
    {
        if (_sequence == 11)
        {
            _character.AnimatorSetTrigger("Summon");
            _attackTime = 0;
            _sequence = 12;
        }
        else if (_sequence == 12)
        {
            _attackTime += Time.deltaTime;
            if (_attackTime > 3)
            {
                _attackTime = 0;
                _sequence++;
                _enemyAI.ResetTarget();
            }
        }
        else if (_sequence == 13)
        {
            _attackTime += Time.deltaTime;
            if (_attackTime > 3)
            {
                _sequence = 0;
                _attackTime = 0;
            }
        }
    }

    public void SummonSpore()
    {
        EnemyNameDefine enemyName = Managers.GetManager<ResourceManager>().Instantiate<EnemyNameDefine>((int)Define.EnemyName.Spore);
        if(enemyName != null)
        {
            Character enemy = enemyName.GetComponent<Character>();
            if(enemy != null)
            {
                enemy.transform.position = transform.position;
                enemy.AddForce(transform.lossyScale.x > 0 ? new Vector3(1, 1).normalized * 300 : new Vector3(-1, 1).normalized * 300);

            }
        }

    }
}
