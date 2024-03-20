using System;
using System.Data;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using Random = UnityEngine.Random;

public class FlyingEnemy : EnemyAI
{
    [SerializeField] protected float _flyHeight;
    public float FlyingHeight => _flyHeight;
    [SerializeField] protected float _flyRange;
    float _normalSpeed;
    [SerializeField] float _flyAttackSpeed = 10;

    private Vector3 _flyAttackDirection;


    protected override void Awake()
    {
        base.Awake();
        _normalSpeed = _character.Speed;

        _character.IsEnableTurn = false;
        _flyHeight += Random.Range(-_flyRange, _flyRange);
    }


    protected override void Move()
    {
        if (_character.IsAttack) return;
        if (_character.IsStun) return;
        if (!IsAutoMove) return;

        Vector3 targetPosition = Vector3.zero;

        if (_target == null)
        {
            Player player = Managers.GetManager<GameManager>().Player;
            if (player == null) return;


            targetPosition.x = player.transform.position.x;
            targetPosition.x += transform.position.x < player.transform.position.x ? -_targetDetectRange.size.x / 3 : _targetDetectRange.size.x / 3;

            Vector3? groundPostion = Managers.GetManager<GameManager>().GetGroundTop(targetPosition);

            if(groundPostion != null)
                targetPosition.y = groundPostion.Value.y + _flyHeight;
            else
                targetPosition.y = player.transform.position.y + _flyHeight;

            targetPosition.y += Mathf.Sin(Time.time + gameObject.GetInstanceID())*2 -1;


            _character.IsEnableTurn = true;
            _character.TurnBody(player.transform.position - transform.position);
            _character.IsEnableTurn = false;
        }
        else
        {
            targetPosition.x = _target.transform.position.x;
            targetPosition.x += transform.position.x < _target.transform.position.x ? -_targetDetectRange.size.x / 3 : _targetDetectRange.size.x / 3;

            Vector3? groundPostion = Managers.GetManager<GameManager>().GetGroundTop(targetPosition);

            if (groundPostion != null)
                targetPosition.y = groundPostion.Value.y + _flyHeight;
            else
                targetPosition.y = _target.transform.position.y + _flyHeight;

            targetPosition.y += Mathf.Sin(Time.time + gameObject.GetInstanceID()) * 2 - 1;


            _character.IsEnableTurn = true;
            _character.TurnBody(_target.transform.position - transform.position);
            _character.IsEnableTurn = false;
        }


        _character.Move(targetPosition - transform.position);
    }

    protected override void DetectTarget()
    {
        if (_character.IsAttack) return;
        if (_character.IsStun) return;

        GameObject[] gameObjects = Util.RangeCastAll2D(gameObject, _targetDetectRange,Define.CharacterMask);

        if (gameObjects.Length > 0)
        {
            foreach (var gameObject in gameObjects)
            {
                if (gameObject == this.gameObject) continue;
                Character character = gameObject.GetComponent<Character>();
                if (character)
                {
                    if (character.CharacterType == Define.CharacterType.Player)
                    {
                        _target = character;
                        return;
                    }
                }
            }
        }

        _target = null;
    }

    protected override void PlayAttack()
    {
        if (_noAttack) return;

        if (_attackElapsed < _attackDelay)
        {
            _attackElapsed += Time.deltaTime;
        }
        else
        {
            // 공격 종료
            if(_target == null || (_character.IsAttack && transform.position.y >= _target.transform.position.y + _flyHeight))
            {
                _character.SetSpeed(_normalSpeed);
                _attackElapsed = 0;
                _character.IsEnableTurn = true;
                _target = null;
                _character.IsAttack = false;
                _character.IsSuperArmer = false;
                return;

            }
            // 공격 시작
            if (!_character.IsAttack &&_target && Mathf.Abs(transform.position.y - (_target.transform.position.y + _flyHeight)) <= 1f && _isTargetInRange)
            {
                _character.SetSpeed(_flyAttackSpeed);
                _attackedList.Clear();
                _flyAttackDirection = (_target.GetCenter() - _character.GetCenter()).normalized;
                _character.IsEnableTurn = false;
                _character.IsAttack = true;
                _character.IsSuperArmer = true;

            }
            // 공격 중
            if (_character.IsAttack&&transform.position.y < (_target.transform.position.y + _flyHeight))
            {
                if (_flyAttackDirection.x > 0)
                {
                    if (_target.transform.position.x < transform.position.x)
                    {
                        _flyAttackDirection.y = Mathf.Abs(_flyAttackDirection.y);
                    }
                }
                else
                {
                    if (_target.transform.position.x > transform.position.x)
                    {

                        _flyAttackDirection.y = Mathf.Abs(_flyAttackDirection.y);
                    }
                }
                _character.Move(_flyAttackDirection);
                OnPlayAttack();
            }
           

        }
    }
    protected override void OnPlayAttack()
    {
        GameObject[] gos = Util.RangeCastAll2D(gameObject, _attackRange, LayerMask.GetMask("Character"));

        foreach (var go in gos)
        {
            Character character = go.GetComponent<Character>();
            if (character == null || character.CharacterType == _character.CharacterType) continue;
            if (_attackedList.Contains(character)) continue;

            _character.Attack(character, 1, 1, Vector3.zero);
            _attackedList.Add(character);
        }
    }

}
