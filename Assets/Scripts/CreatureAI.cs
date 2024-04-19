using MoreMountains.Feedbacks;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class CreatureAI : MonoBehaviour
{
    Character _character;
    public Character Character => _character;
    Player _player;
    BoxCollider2D _boxCollider;
    GameObject _model;
    GameObject _soulModel;

    [SerializeField] bool _debug;

    [SerializeField] CreatureAbility _creatureAbility = new CreatureAbility();
    public CreatureAbility CreatureAbility=>_creatureAbility;


    [SerializeField] Define.Range _normalAttackDetectRange;
    [SerializeField] Define.Range _normalAttackRange;

    float _girlToCreatureDistance = 8f;
    bool _isSoulForm = false;

    public int AttackPower
    {
        get
        {
            int ap = _character.AttackPower;
            ap = Mathf.RoundToInt(ap*Util.CalcPercentage(CreatureAbility.GetIncreasedAttackPowerPercentage()));
            return ap;
        }
    }
    public float AttackSpeed
    {
        get
        {
            float attackSpeed = 1;
            attackSpeed *= Util.CalcPercentage(CreatureAbility.GetIncreasedAttackSpeedPercentage());
            return attackSpeed;
        }
    }


    // 일반공격 변수
    float _normalAttackCoolTime => 3f/Util.CalcPercentage(CreatureAbility.GetIncreasedAttackSpeedPercentage());
    public float DecreasedNormalAttackCoolTimePercentage { set; get; }

    public bool IsStopAI { set; get; }
    Character _closeEnemy;
    Coroutine _aiCoroutine;
   

    [SerializeField] float _throwPower;

    // 부활
    public float ReviveTime {  set; get; } = 20;
    public float ReviveElasped { get;private set; }


    private void Awake()
    {
        _character = GetComponent<Character>();
        _boxCollider = GetComponent<BoxCollider2D>();
        _model = transform.Find("Model").gameObject;
        _soulModel = transform.Find("SoulModel").gameObject;
        _creatureAbility.Init(this);
        Managers.GetManager<GameManager>().CreatureAI = this;
        _character.CharacterDeadHandler += OnCharacterDead;
        _character.DamagedHandler += OnDamaged;
        _character.IncreasedHpRegeneration += 5f;
    }

    private void OnCharacterDead()
    {
        _model.gameObject.SetActive(false);
        _character.Move(Vector2.zero);
        ReviveElasped = 0;
    }
    void OnDamaged(Character attacker, int damage, float power, Vector3 direction, Vector3 point, float stunTime)
    {
        _closeEnemy = null;
        if (_aiCoroutine != null)
        {
            StopCoroutine(_aiCoroutine);
            _aiCoroutine = null;
        }

    }
    private void OnDrawGizmosSelected()
    {
        _creatureAbility.OnDrawGizmosSelected();
        if (_debug)
        {
            Util.DrawRangeOnGizmos(gameObject, _normalAttackDetectRange, Color.green);
            Util.DrawRangeOnGizmos(gameObject, _normalAttackRange, Color.red);
        }
    }
    private void Update()
    {
        Revive();
        if (_character.IsDead) return;
        //if (_isSoulForm)
        //{
        //    Player player = Managers.GetManager<GameManager>().Player;

        //    _character.TurnBody(player.transform.position - transform.position);
        //    Vector3 offset = new Vector3(-3, 5, 0);
        //    offset.y += Mathf.Sin(Time.time * 0.5f + gameObject.GetInstanceID());
        //    if (player.transform.localScale.x < 0)
        //        offset.x = -offset.x;

        //    transform.position = Vector3.Lerp(transform.position, player.transform.position + offset, 0.01f);

        //    return;
        //}

        DefaultAI();
        _creatureAbility.AbilityUpdate();
    }

    void Revive()
    {
        if (_character.IsDead)
        {
            ReviveElasped += Time.deltaTime;
            if (ReviveElasped > ReviveTime)
            {
                _character.Revive();
                ReviveElasped = 0;
                _model.gameObject.SetActive(true);
                transform.position = Managers.GetManager<GameManager>().Player.transform.position;
            }
        }
    }
    void DefaultAI()
    {
        if (_aiCoroutine != null) return;
        if (_character.IsAttack) return;
        if (IsStopAI) return;

        if (_player == null)
        {
            _player = Managers.GetManager<GameManager>().Player;
            return;
        }

        if (!_player.IsFire && (_player.transform.position - transform.position).magnitude > _girlToCreatureDistance)
        {
            FollowPlayer();
        }
        else
        {
            if(_closeEnemy == null)
                _closeEnemy = GetCloseEnemy();

            if (_closeEnemy == null)
                FollowPlayer();

           
            _aiCoroutine = StartCoroutine(CorPlayNormalAttack());
        }
    }
    IEnumerator CorPlayNormalAttack()
    {
        if (_closeEnemy != null)
        {
            Vector3 initPosition = transform.position;
            Define.Range attackRange = _normalAttackRange;
            attackRange.size = new Vector3(attackRange.size.x / 4, attackRange.size.y, 0);
            while (true)
            {
                if (_closeEnemy == null || _character.IsAttack || _character.IsStun || _closeEnemy.IsDead) break;
                _character.Move(_closeEnemy.transform.position - transform.position);

                if (Util.RangeCastAll2D(gameObject, attackRange, Define.CharacterMask, (hit) =>
                {
                    Character character = hit.collider.GetComponent<Character>();
                    if (character != null&&!character.IsDead && character.CharacterType == Define.CharacterType.Enemy)
                    {
                        return true;
                    }

                    return false;
                }).Count > 0) break;
                yield return null;
            }
            if (_closeEnemy != null && !_character.IsAttack && !_character.IsStun && !_closeEnemy.IsDead)
            {
                _character.Move(Vector2.zero);
                _character.TurnBody(_closeEnemy.transform.position - transform.position);

                _character.SetAnimationSpeed(AttackSpeed);
                _character.SetAnimatorTrigger("NormalAttack");
                _character.IsAttack = true;
                _character.IsEnableMove = false;
                _character.IsEnableTurn = false;

                while (_character.IsAttack)
                {
                    yield return null;
                }
            }
        }

        _aiCoroutine = null;
        _closeEnemy = null;
    }

    // AI 행동을 초기화
    public void ResetAI()
    {
        _character.SetAnimatorTrigger("Reset");
        _character.IsAttack = false;
        _character.IsEnableMove = true;
        _character.IsEnableTurn = true;
        _aiCoroutine = null;
        _closeEnemy = null;
    }
    void FollowPlayer()
    {
        if (_character.IsAttack) return;

        _aiCoroutine = StartCoroutine(CorFollowPlayer());
    }

    IEnumerator CorFollowPlayer()
    {
        while (true)
        {
            if ( _character.IsAttack) break;
            Vector3 distacne = _player.transform.position - transform.position;
            if (Mathf.Abs(distacne.x) > _girlToCreatureDistance)
            { 
                _character.Move(Vector3.right * (distacne.x + ((distacne.x > 0 ? -1 : 1) * (_girlToCreatureDistance))));
                _closeEnemy = GetCloseEnemy();
                if (_closeEnemy && _player.IsFire) break;
                if (_closeEnemy != null)
                {
                    if ((_closeEnemy.transform.position.x > _player.transform.position.x && _closeEnemy.transform.position.x < transform.position.x)||
                        (_closeEnemy.transform.position.x < _player.transform.position.x && _closeEnemy.transform.position.x > transform.position.x))
                    {
                        yield return StartCoroutine(CorPlayNormalAttack());
                    }
                    _closeEnemy = null;
                }
            }
            else
            {
                break;
            }
            yield return null;
        }

        _aiCoroutine = null;
    }

    public void Transform()
    {
        if (!_isSoulForm)
        {
            _boxCollider.enabled = false;
            _isSoulForm = true;
            _model.gameObject.SetActive(false);
            _soulModel.gameObject.SetActive(true);
            _character.ChangeEnableFly(true);
        }
        else
        {
            Vector3? position = Util.GetGroundPosition(transform.position);
            if (position != null)
                transform.position = Util.GetGroundPosition(transform.position).Value;

            _boxCollider.enabled = true;
            _isSoulForm = false;
            _model.gameObject.SetActive(true);
            _soulModel.gameObject.SetActive(false);
            _character.ChangeEnableFly(false);
        }
    }
   
    void NormalAttack()
    {
        List<RaycastHit2D> hits = Util.RangeCastAll2D(gameObject, _normalAttackRange, LayerMask.GetMask("Character"));

        foreach (var hit in hits)
        {
            Character c = hit.collider.GetComponent<Character>();
            if (c != null && c.CharacterType == Define.CharacterType.Enemy)
            {
                _character.Attack(c, AttackPower, 50, c.transform.position - transform.position, hit.point);

                Effect effect = Managers.GetManager<ResourceManager>().Instantiate<Effect>((int)Define.EffectName.NormalAttack);
                effect.SetMultiflySize(new Vector3(3.5f * (transform.lossyScale.x <0? -1:1), 2.5f, 0));
                effect.Play(transform.position + new Vector3(1f * (transform.lossyScale.x < 0 ? -1 : 1), 3.31f,0)); 
            }
        }
    }
     void FinishAttack()
    {
        _character.IsAttack = false;
        _character.IsEnableMove = true;
        _character.IsEnableTurn = true;
        _character.SetAnimationSpeed(1);
    }
    public Character GetCloseEnemy()
    {
        Character close = null;

        Util.RangeCastAll2D(_player.gameObject, _normalAttackDetectRange, Define.CharacterMask, (hit) =>
        {
            if (hit.collider != null)
            {
                Character character = hit.collider.GetComponent<Character>();
                if (character == null||character.IsDead || character.CharacterType != Define.CharacterType.Enemy || character.IsEnableFly) return false;
                if (close == null || (close.transform.position - transform.position).magnitude > (character.transform.position - transform.position).magnitude)
                    close = character;
            }
            return true;
        });

        return close;
    }

   
}