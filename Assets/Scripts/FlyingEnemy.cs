using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class FlyingEnemy : EnemyAI
{
    [SerializeField] protected float _flyHeight;

    float _normalSpeed;
    [SerializeField] float _flyAttackSpeed = 10;

    float _flyAttackDuration = 2;
    float _flyAttackElasepd = 0;
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
        if (_target) return;
        if (!IsAutoMove) return;

        Player player = Managers.GetManager<GameManager>().Player;
        if (player == null) return;
        Vector3 moveDirection = Vector3.zero;

        moveDirection.x = player.transform.position.x - transform.position.x;
        if (transform.position.y-player.transform.position.y > _flyHeight)
        {
            moveDirection.y = -1;
        }
        else
        {
            if (Mathf.Abs(transform.position.y - (player.transform.position.y + _flyHeight) ) < 0.2f)
                moveDirection.y = 0;
            else
                moveDirection.y =  player.transform.position.y+ _flyHeight - transform.position.y;
        }
        _character.Move(moveDirection*0.5f);
    }
    protected override void DetectCharacter()
    {
        if (_character.IsAttack) return;
        if (_character.IsStun) return;
        if (_target != null) return;

        GameObject[] gameObjects = Util.RangeCastAll2D(gameObject, _attackDetectRange,Define.CharacterMask);

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
                        if (_flyAttackElasepd == 0 && Mathf.Abs(transform.position.y - (character.transform.position.y + _flyHeight)) > 0.3f) return;
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
            if (_target == null) return;

            if (_flyAttackElasepd == 0 && Mathf.Abs(transform.position.y - (_target.transform.position.y + _flyHeight)) <= 0.3f)
            {
                _character.SetSpeed(_flyAttackSpeed);
                _attackedList.Clear();
                _flyAttackDirection = (_target.GetCenter() - transform.position).normalized;
                _character.IsEnableTurn = false;
                _character.IsAttack = true;
            }
            if (_flyAttackElasepd < _flyAttackDuration && _character.IsAttack)
            {
                _flyAttackElasepd += Time.deltaTime;
                _character.Move(_flyAttackDirection);
                OnPlayAttack();
            }
            else if(_flyAttackElasepd >= _flyAttackDuration && _character.IsAttack)
            {
                _character.SetSpeed(_normalSpeed);
                _flyAttackElasepd = 0;
                _attackElapsed = 0;
                _character.IsEnableTurn = true;
                _target = null;
                _character.IsAttack = false;
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
