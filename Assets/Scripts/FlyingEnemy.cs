using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class FlyingEnemy : EnemyAI
{
    [SerializeField] protected float _flyHeight;
    public float FlyingHeight => _flyHeight;
    float _normalSpeed;
    [SerializeField] float _flyAttackSpeed = 10;
    
    
    private Vector3 _flyAttackDirection;


    protected override void Awake()
    {
        base.Awake();
        _normalSpeed = _character.Speed;
    }


    protected override void Move()
    {
        if (_character.IsAttack) return;
        if (_character.IsStun) return;
        if (!IsAutoMove) return;

        Vector3 moveDirection = Vector3.zero;

        if (_target == null)
        {
            Player player = Managers.GetManager<GameManager>().Player;
            if (player == null) return;

            moveDirection.x = player.transform.position.x - transform.position.x;
            if (transform.position.y - player.transform.position.y > _flyHeight)
            {
                moveDirection.y = -1;
            }
            else
            {
                if (Mathf.Abs(transform.position.y - (player.transform.position.y + _flyHeight)) < 0.2f)
                    moveDirection.y = 0;
                else
                    moveDirection.y = 1;
            }
        }
        else
        {
            moveDirection.x = _target.transform.position.x - transform.position.x;
            if (transform.position.y - _target.transform.position.y > _flyHeight)
            {
                moveDirection.y = -1;
            }
            else
            {
                if (Mathf.Abs(transform.position.y - (_target.transform.position.y + _flyHeight)) < 0.2f)
                    moveDirection.y = 0;
                else
                    moveDirection.y = _target.transform.position.y + _flyHeight - transform.position.y;
            }
        }
        if (_targetInRange) moveDirection.x =0;

        _character.Move(moveDirection);
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
        if (_attackElapsed < _attackDelay)
        {
            _attackElapsed += Time.deltaTime;
        }
        else
        {
            if (!_character.IsAttack &&_target && Mathf.Abs(transform.position.y - (_target.transform.position.y + _flyHeight)) <= 1f && _targetInRange)
            {
                _character.SetSpeed(_flyAttackSpeed);
                _attackedList.Clear();
                _flyAttackDirection = (_target.GetCenter() - _character.GetCenter()).normalized;
                _character.IsEnableTurn = false;
                _character.IsAttack = true;
            }
            if (_character.IsAttack&&transform.position.y < (_target.transform.position.y + _flyHeight))
            {
                if (_flyAttackDirection.x > 0)
                {
                    if (_target.transform.position.x < transform.position.x)
                    {

                        _flyAttackDirection.x = 1;
                        _flyAttackDirection.y = Mathf.Abs(_flyAttackDirection.y);
                    }
                }
                else
                {
                    if (_target.transform.position.x > transform.position.x)
                    {

                        _flyAttackDirection.x = -1;
                        _flyAttackDirection.y = Mathf.Abs(_flyAttackDirection.y);
                    }
                }
                _character.Move(_flyAttackDirection);
                OnPlayAttack();
            }
            else if(_character.IsAttack&&transform.position.y >= _target.transform.position.y + _flyHeight )
            {
                _character.SetSpeed(_normalSpeed);
                _attackElapsed = 0;
                _character.IsEnableTurn = true;
                _target = null;
                _character.IsAttack = false;
                Debug.Log("End");
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
