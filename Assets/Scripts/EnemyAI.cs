using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    protected Character _character;
    protected Rigidbody2D _rigidbody;

    [SerializeField] protected Define.Range _targetDetectRange;
    [SerializeField]protected Define.Range _enableAttackRange;
    public Define.Range EnableAttackRange => _enableAttackRange;
    [SerializeField]protected Define.Range _attackRange;
    public Define.Range AttackRange => _attackRange;

    [SerializeField] protected Character _target;
    public Character Target=>_target;

    // 움직임
    public float MoveRate { get; set; } = 1;     //움직임 배율 => 달리기 , 걷기에 사용

    // 공격과 공격사이의 딜레이
    [SerializeField] protected bool _noAttack;
    [SerializeField] protected float _attackDelay = 1;
    protected  float _attackElapsed;
    protected bool _isTargetInRange;
    public bool IsTargetInRange=>_isTargetInRange;

    // 공격하는 중의 딜레이
    protected float _attackingDelay = 2;
    protected float _attackingElasepd = 0;

    [SerializeField] bool _isRangedAttack;
    [field:SerializeField] public bool IsAutoMove { set; get; } = true;
    [SerializeField] string _attackTriggerName;

    protected SpriteRenderer _attackRangeSprite;
    protected List<Character> _attackedList = new List<Character>();

    [SerializeField] float _itemDropPercentage;

    protected virtual void Awake()
    {
        _character = GetComponent<Character>();
        _rigidbody = _character.GetComponent<Rigidbody2D>();
        _character.PlayAttackHandler += OnPlayAttack;
        _character.FinishAttackHandler += FinishAttack;
        _character.CharacterDeadHandler += () =>
        {
            Managers.GetManager<GameManager>().Exp += 1;
            float random = _itemDropPercentage * 100;
            if(Random.Range(0, 10000) < random)
            {
                Managers.GetManager<ResourceManager>().Instantiate("Prefabs/Wallet").transform.position = transform.position;
            }
        };

        if (transform.Find("AttackRange"))
        {
            _attackRangeSprite = transform.Find("AttackRange").GetComponent<SpriteRenderer>();
        }else
        {
            _attackRangeSprite = Managers.GetManager<ResourceManager>().Instantiate("AttackRange").GetComponent<SpriteRenderer>();
            _attackRangeSprite.transform.SetParent(transform);
        }
        _attackRangeSprite.gameObject.SetActive(false);
    }

    protected virtual void OnDrawGizmosSelected()
    {
        Util.DrawRangeOnGizmos(gameObject, _attackRange, Color.red);
        Util.DrawRangeOnGizmos(gameObject, _enableAttackRange, Color.green);
        Util.DrawRangeOnGizmos(gameObject, _targetDetectRange, Color.blue);
    }
    void Update()
    {
        DetectTarget();
        CheckTargetInAttackRange();
        Move();
        PlayAttack();
    }

    protected virtual void Move()
    {
        if (_isTargetInRange) return;
        if (_character.IsAttack) return;
        if (_character.IsStun) return;

        if (!IsAutoMove) return;
        if (_target == null)
        {
            Player player = Managers.GetManager<GameManager>().Player;
            if (player == null) return;
            _character.Move((player.transform.position - transform.position).normalized * MoveRate);
        }else
        {
            _character.Move((_target.transform.position - transform.position).normalized* MoveRate);
        }
    }

    protected virtual void DetectTarget()
    {
        if (_character.IsAttack) return;

        GameObject[] gameObjects = Util.RangeCastAll2D(gameObject, _targetDetectRange,Define.CharacterMask);

        float distance = 0;
        Character closeOne = null;
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
                        if (closeOne == null || distance > (transform.position - character.transform.position).magnitude)
                        {
                            closeOne = character;
                            distance= (transform.position - closeOne.transform.position).magnitude;
                        }
                    }
                }
            }
        }
        _target = closeOne;

    }
    protected virtual void CheckTargetInAttackRange()
    {
        if (_target == null)
        {
            _isTargetInRange = false;
            return;
        }

        GameObject[] gameObjects = Util.RangeCastAll2D(gameObject, _enableAttackRange);

        if(gameObjects.Length > 0 ) 
        { 
            foreach(var gameObject in gameObjects)
            {
                if (_target.gameObject == gameObject)
                {
                    _isTargetInRange = true;
                    return;
                }
            }
        }

        _isTargetInRange = false;
    }

    protected virtual void PlayAttack()
    {
        if(_noAttack) return;

        if (_attackElapsed < _attackDelay)
        {
            _attackElapsed += Time.deltaTime;
        }
        else
        {
            if (!_isRangedAttack)
            {
                // 공격 모션이 따로 있을 때
                if (!_attackTriggerName.Equals("") && _isTargetInRange)
                {
                    _attackedList.Clear();
                    _character.IsEnableMove = false;
                    _character.IsEnableTurn = false;
                    _character.IsAttack = true;
                    _character.TurnBody(_target.transform.position - transform.position);
                    _character.AnimatorSetTrigger(_attackTriggerName);
                    _attackElapsed = 0;
                }
                // 공격 모션이 따로 없을 때
                else
                {
                    if (_attackingElasepd == 0 && _isTargetInRange)
                    {
                        _attackedList.Clear();
                        Color color = _attackRangeSprite.color;
                        color.a = 0;
                        _attackRangeSprite.color = color;
                        _attackRangeSprite.transform.localScale = _attackRange.size;
                        _attackRangeSprite.transform.localPosition = _attackRange.center;

                        _attackRangeSprite.gameObject.SetActive(true);
                        _character.IsAttack = true;
                    }

                    if (_attackingDelay > _attackingElasepd && _character.IsAttack)
                    {
                        _attackingElasepd += Time.deltaTime;
                        Color color = _attackRangeSprite.color;
                        if(_attackingDelay != 0)
                            color.a = _attackingElasepd/_attackingDelay;
                        _attackRangeSprite.color = color;

                    }
                    if (_attackingDelay <= _attackingElasepd && _character.IsAttack)
                    {
                        _attackingElasepd = 0;
                        _attackRangeSprite.gameObject.SetActive(false);
                        OnPlayAttack();
                        _attackElapsed = 0;
                        _character.IsAttack = false;
                    }
                }
            }
            else
            {
                Projectile projectile = Managers.GetManager<ResourceManager>().Instantiate<Projectile>("Prefabs/Projectile");
                projectile.Init(10, 10, 2, Define.CharacterType.Player);
                projectile.transform.position = transform.position;
                projectile.Fire(_character, _target.GetCenter() - transform.position);
            }
        }
    }

    protected virtual void OnPlayAttack()
    {
        GameObject[] gos = Util.RangeCastAll2D(gameObject, _attackRange, LayerMask.GetMask("Character"));

        foreach (var go in gos)
        {
            Character character = go.GetComponent<Character>();
            if(character == null || character.CharacterType == _character.CharacterType) continue;

            _character.Attack(character, 1, 1, Vector3.zero);
        }
        _target = null;
    }

     void FinishAttack()
    {
        _character.IsAttack = false;
        _character.IsEnableMove = true;
        _character.IsEnableTurn = true;
        _attackElapsed = 0;
    }

}
