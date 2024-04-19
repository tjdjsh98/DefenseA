using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    protected Character _character;
    protected Rigidbody2D _rigidbody;

    [Header("적 감지 범위 & 공격 사거리")]
    [SerializeField] protected Define.Range _targetDetectRange;
    [SerializeField]protected Define.Range _enableAttackRange;
    public Define.Range EnableAttackRange => _enableAttackRange;
    [SerializeField]protected Define.Range _attackRange;
    public Define.Range AttackRange => _attackRange;
    protected bool _isTargetInRange;
    public bool IsTargetInRange => _isTargetInRange;
    [SerializeField] protected Character _target;

    public Character Target=>_target;
    Coroutine _detectTargetCoroutine;

    [Header("파츠")]
    [SerializeField] MonoBehaviour _movePart;
    IMoveAI _moveAI;

    [Header("행동 제약")]
    [field:SerializeField]public bool IsEnableBodyAttack = true;
    [field:SerializeField]public bool IsStopDetectTarget { get; set; }      // 적이 감지 되었을 떄 움직임을 멈춤
    public float MoveRate { get; set; } = 1;     //움직임 배율 => 달리기 , 걷기에 사용
    [SerializeField] protected bool _noAttack;                              // 체크되면 일반 공격하지 않음
    [field:SerializeField] public bool IsStopMoveAI { get; set; }
    // 공격과 공격사이의 딜레이
    [Header("공격 조건")]
    [SerializeField] protected float _attackDelay = 1;
    protected  float _attackElapsed;
  
    // 공격하는 중의 딜레이
    protected float _attackingDelay = 2;
    protected float _attackingElasepd = 0;
    [SerializeField] bool _isHaveAnimation;     // 일반 공격 때 특정 공격 모션을 가지고 있다면 체크
    protected List<Character> _attackedList = new List<Character>();


    // 임시로 공격 범위 표시
    protected SpriteRenderer _attackRangeSprite;

    // 바디 어택
    [Header("바디 어택")]
    Define.Range _bodySize;
    [SerializeField]float _bodyAttackKnockBackPower = 50;
    [SerializeField]float _bodyAttackCoolTime = 1;
    List<GameObject> _bodyAttackList = new List<GameObject>();
    Coroutine _bodyAttackCoroutine;

    [Header("드랍 아이템")]
    [SerializeField] float _dropPercentage;

    protected virtual void Awake()
    {
        _character = GetComponent<Character>();
        _rigidbody = _character.GetComponent<Rigidbody2D>();
        _bodySize = _character.GetSize();
        _character.CharacterDeadHandler += OnCharacterDead;
        if (_movePart)
            _moveAI = _movePart as IMoveAI;

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

    private void OnEnable()
    {
        if (_detectTargetCoroutine != null)
            StopCoroutine(_detectTargetCoroutine);
        _detectTargetCoroutine = StartCoroutine(CorDetectTarget());

        
        if(_bodyAttackCoroutine != null)
            StopCoroutine(_bodyAttackCoroutine);
        if (IsEnableBodyAttack) 
            _bodyAttackCoroutine = StartCoroutine(CorBodyAttack());
    }

    private void OnDisable()
    {
        if (!Managers.IsQuit)
        {
            if(_detectTargetCoroutine!=null)
                StopCoroutine(_detectTargetCoroutine);
            if (_bodyAttackCoroutine != null)
                StopCoroutine(_bodyAttackCoroutine);
        }

    }
    void Update()
    {
        if (_character.IsDead) return;

        CheckTargetInAttackRange();
        Move();
        PlayAttack();
    }
    public void ResetTarget()
    {
        _target = null;
    }
    IEnumerator CorBodyAttack()
    {
        yield return new WaitForSeconds(1);
        while(true)
        {
            Util.RangeCastAll2D(gameObject, _bodySize, Define.CharacterMask, (hit) =>
            {
                if (hit.collider == null || _bodyAttackList.Contains(hit.collider.gameObject)) return false;

                Character character = hit.collider.GetComponent<Character>();

                if (character != null && !character.IsDead && character.CharacterType == Define.CharacterType.Player)
                {
                    _character.Attack(character, _character.AttackPower, _bodyAttackKnockBackPower, character.transform.position - transform.position, hit.point, 0.1f);
                    _bodyAttackList.Add(hit.collider.gameObject);
                    StartCoroutine(CorRemoveBodyAttackList(hit.collider.gameObject));
                }
                return false;
            });
            yield return new WaitForSeconds(0.1f);
        }
    }
    IEnumerator CorRemoveBodyAttackList(GameObject gameObject)
    {
        yield return new WaitForSeconds(_bodyAttackCoolTime);
        _bodyAttackList.Remove(gameObject);
    }
    void OnCharacterDead()
    {
        Managers.GetManager<GameManager>().Money += 1;
        Managers.GetManager<GameManager>().HuntingCount += 1;
        if (Random.Range(0, 100) < _dropPercentage)
        {
            Managers.GetManager<ResourceManager>().Instantiate("Prefabs/Wallet").transform.position = transform.position;
        }

        if (_detectTargetCoroutine != null)
            StopCoroutine(_detectTargetCoroutine);
        if (_bodyAttackCoroutine != null)
            StopCoroutine(_bodyAttackCoroutine);
    }

    protected virtual void Move()
    {
        if (IsStopMoveAI) return;
        if (_isTargetInRange) return;
        if (_character.IsAttack) return;
        if (_character.IsStun) return;
        if (IsStopDetectTarget && _target != null) return;
        if (_moveAI != null)
        {
            _moveAI.MoveAI();
            return;
        }
    
        if (_target == null)
        {
            Character girl = Managers.GetManager<GameManager>().Girl;
            if (girl == null) return;

            _character.Move((girl.transform.position - transform.position).normalized * MoveRate);
        }else
        {
            _character.Move((_target.transform.position - transform.position).normalized* MoveRate);
        }
    }


    protected virtual IEnumerator CorDetectTarget()
    {
        while (true)
        {
            if (!_character.IsAttack && !_character.IsStun)
            {
                List<RaycastHit2D> hits = Util.RangeCastAll2D(gameObject, _targetDetectRange, Define.CharacterMask);

                float distance = 0;
                Character closeOne = null;
                if (hits.Count > 0)
                {
                    foreach (var hit in hits)
                    {
                        if (hit.collider == null) continue;

                        if (hit.collider.gameObject == this.gameObject) continue;
                        Character character = hit.collider.gameObject.GetComponent<Character>();
                        if (character && !character.IsDead)
                        {
                            if (character.CharacterType == Define.CharacterType.Player)
                            {
                                if (closeOne == null || distance > (transform.position - character.transform.position).magnitude)
                                {
                                    closeOne = character;
                                    distance = (transform.position - closeOne.transform.position).magnitude;
                                }
                            }
                        }
                    }
                }
                _target = closeOne;
            }

            yield return new WaitForSeconds(0.2f);
        }
    }


    protected virtual void CheckTargetInAttackRange()
    {
        if (_target == null)
        {
            _isTargetInRange = false;
            return;
        }

        List<RaycastHit2D> hits = Util.RangeCastAll2D(gameObject, _enableAttackRange);

        if(hits.Count > 0 ) 
        { 
            foreach(var hit in hits)
            {
                if (_target.gameObject == hit.collider.gameObject)
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
            // 공격 모션이 따로 있을 때
            if (!_isHaveAnimation && _isTargetInRange)
            {
                _attackedList.Clear();
                _character.IsEnableMove = false;
                _character.IsEnableTurn = false;
                _character.IsAttack = true;
                _character.TurnBody(_target.transform.position - transform.position);
                _character.SetAnimatorTrigger("Attack");
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
    }

    protected virtual void OnPlayAttack()
    {
        List<RaycastHit2D> hits = Util.RangeCastAll2D(gameObject, _attackRange, LayerMask.GetMask("Character"));

        foreach (var hit in hits)
        {
            Character character = hit.collider.GetComponent<Character>();
            if(character == null || character.CharacterType == _character.CharacterType) continue;

            _character.Attack(character, _character.AttackPower, 1, Vector3.zero, hit.point);

            Effect effect = Managers.GetManager<ResourceManager>().Instantiate<Effect>((int)Define.EffectName.Hit3);

            effect.transform.position = (character.GetCenter() + (transform.position + _attackRange.center))/2;
            effect.transform.rotation = Quaternion.Euler(character.GetCenter() - (transform.position + _attackRange.center));
            effect.Play(hit.point);

        }
        _target = null;
    }

    public void FinishAttack()
    {
        _character.IsAttack = false;
        _character.IsEnableMove = true;
        _character.IsEnableTurn = true;
        _attackElapsed = 0;
    }

}
