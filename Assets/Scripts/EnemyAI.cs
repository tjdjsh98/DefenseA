using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using Unity.Loading;
using UnityEditor.Rendering;
using UnityEditor.Timeline;
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

    [field:SerializeField]public bool IsEnableBodyAttack = true;

    // ������
    public float MoveRate { get; set; } = 1;     //������ ���� => �޸��� , �ȱ⿡ ���

    // ���ݰ� ���ݻ����� ������
    [SerializeField] protected bool _noAttack;
    [SerializeField] protected float _attackDelay = 1;
    protected  float _attackElapsed;
    protected bool _isTargetInRange;
    public bool IsTargetInRange=>_isTargetInRange;

    // �����ϴ� ���� ������
    protected float _attackingDelay = 2;
    protected float _attackingElasepd = 0;

    [field:SerializeField] public bool IsAutoMove { set; get; } = true;
    [SerializeField] bool _isHaveAnimation;

    protected SpriteRenderer _attackRangeSprite;
    protected List<Character> _attackedList = new List<Character>();

    [SerializeField] float _itemDropPercentage;

    Coroutine _detectTargetCoroutine;

    // �ٵ� ����
    Define.Range _bodySize;
    [SerializeField]float _bodyAttackCoolTime = 1;
    List<GameObject> _bodyAttackList = new List<GameObject>();
    Coroutine _bodyAttackCoroutine;

    protected virtual void Awake()
    {
        _character = GetComponent<Character>();
        _rigidbody = _character.GetComponent<Rigidbody2D>();
        _bodySize = _character.GetSize();
        _character.CharacterDeadHandler += OnCharacterDead;

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
        CheckTargetInAttackRange();
        Move();
        PlayAttack();
    }

    IEnumerator CorBodyAttack()
    {
        while(true)
        {
            Util.RangeCastAll2D(gameObject, _bodySize, Define.CharacterMask, (hit) =>
            {
                if (hit.collider == null) return false;
                Character character = hit.collider.GetComponent<Character>();

                if (character != null && character.CharacterType == Define.CharacterType.Player)
                {
                    _character.Attack(character, _character.AttackPower, 50, character.transform.position - transform.position, hit.point, 0.1f);
                    _bodyAttackList.Add(hit.collider.gameObject);
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

        int percentage = 0;
        if (Managers.GetManager<CardManager>().AbilityCount == 0) percentage = 15;
        else if (Managers.GetManager<CardManager>().AbilityCount == 1) percentage = 13;
        else if (Managers.GetManager<CardManager>().AbilityCount == 2) percentage = 11;
        else if (Managers.GetManager<CardManager>().AbilityCount == 3) percentage = 9;
        else if(Managers.GetManager<CardManager>().AbilityCount == 4) percentage = 7;
        else percentage = 1;
        
        if (Random.Range(0, 100f) < percentage) 
        {
            Managers.GetManager<ResourceManager>().Instantiate("Prefabs/Wallet").transform.position = transform.position;
        }
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
                        if (character)
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
            // ���� ����� ���� ���� ��
            if (!_isHaveAnimation && _isTargetInRange)
            {
                _attackedList.Clear();
                _character.IsEnableMove = false;
                _character.IsEnableTurn = false;
                _character.IsAttack = true;
                _character.TurnBody(_target.transform.position - transform.position);
                _character.AnimatorSetTrigger("Attack");
                _attackElapsed = 0;
            }
            // ���� ����� ���� ���� ��
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

            Managers.GetManager<EffectManager>().InstantiateHitEffect(hit.point, hit.centroid - hit.point);

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
