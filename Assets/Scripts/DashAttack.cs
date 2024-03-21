using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DashAttack : MonoBehaviour
{
    Character _character;
    EnemyAI _enemyAI;

    bool _isStartDash;

    float _preSpeed;
    [SerializeField] float _dashSpeed = 20;

    [SerializeField] float _dashCoolTime = 5;
    float _dashCoolTimeElased = 0;

    [SerializeField] float _dashDelayTime = 2;
    float _dashTime;
    float _dashDuration = 2;
    Vector3 _dashDirection;

    private void Awake()
    {
        _character = GetComponent<Character>();
        _enemyAI = GetComponent<EnemyAI>();
    }

    private void Update()
    {
        if (_dashCoolTime > _dashCoolTimeElased)
        {
            _dashCoolTimeElased+= Time.deltaTime;
            return;
        }
        if (_enemyAI.Target != null && !_isStartDash)
        {
            _character.IsAttack = true;
            _preSpeed = _character.Speed;
            _character.SetSpeed(_dashSpeed);
            _isStartDash = true;
            _dashDirection = _enemyAI.Target.transform.position - transform.position;
            _character.IsSuperArmer = true;
        }

        if(_isStartDash)
        {
            _dashTime += Time.deltaTime;
            if(_dashTime > _dashDelayTime)
            {
                _character.Move(_dashDirection);
                Util.RangeCastAll2D(gameObject, _enemyAI.AttackRange, Define.CharacterMask,
                    (go) =>
                    {
                        Character character = go.GetComponent<Character>();
                        if (character != null && _character.CharacterType != character.CharacterType)
                        {
                            _character.Attack(character, _character.AttackPower, 100, new Vector3(_dashDirection.x >0 ? 1: -1,1,0), 0.5f);
                            _dashTime = _dashDelayTime + _dashDuration;
                            _character.Move(Vector3.zero);
                            _character.Damage(_character, 0, 50, new Vector3(_dashDirection.x > 0 ? 1 : 1, -1, 0));
                        }
                        return false;
                    });
            }

            if(_dashTime >= _dashDelayTime + _dashDuration)
            {
                _character.SetSpeed(_preSpeed);
                _dashTime = 0;
                _isStartDash = false;
                _dashCoolTimeElased = 0;
                _character.IsAttack = false;
                _character.IsSuperArmer = false;
            }
        }
    }
}
