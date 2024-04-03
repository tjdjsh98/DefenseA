using System;
using System.Collections;
using System.Collections.Generic;
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

    float _girlToCreatureDistance = 5f;
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

    Character _closeOne;


    // 일반공격 변수
    Vector3 _normalAttackPosition;
    float _normalAttackTime = 0;
    float _normalAttackCoolTime = 3f;
    float NormalAttackCoolTime => DecreasedNormalAttackCoolTimePercentage > 0 ? _normalAttackCoolTime / (1 + (DecreasedNormalAttackCoolTimePercentage / 100)) : _normalAttackCoolTime * (1 - (DecreasedNormalAttackCoolTimePercentage / 100));
    public int NormalAttackDamage => IncreasedNormalAttackPercentage > 0 ? AttackPower * (1 + IncreasedNormalAttackPercentage / 100) : AttackPower / (1 - IncreasedNormalAttackPercentage / 100);
    public int IncreasedNormalAttackPercentage { set; get; }
    public float IncreasedNormalAttackSpeedPercentage { set; get; }
    public float DecreasedNormalAttackCoolTimePercentage { set; get; }

    Character _closeEnemy;

   

    [SerializeField] float _throwPower;

    // 부활
    float _reviveTime = 20;
    float _reviveElasped;
    private void Awake()
    {
        _character = GetComponent<Character>();
        _boxCollider = GetComponent<BoxCollider2D>();
        _model = transform.Find("Model").gameObject;
        _soulModel = transform.Find("SoulModel").gameObject;
        _creatureAbility.Init(this);

        Managers.GetManager<GameManager>().CreatureAI = this;
        _character.CharacterDeadHandler += OnCharacterDead;

    }
    private void OnCharacterDead()
    {
        _model.gameObject.SetActive(false);
        _character.Move(Vector2.zero);
        _reviveElasped = 0;
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
        if (_isSoulForm)
        {
            Player player = Managers.GetManager<GameManager>().Player;

            _character.TurnBody(player.transform.position - transform.position);
            Vector3 offset = new Vector3(-3, 5, 0);
            offset.y += Mathf.Sin(Time.time * 0.5f + gameObject.GetInstanceID());
            if (player.transform.localScale.x < 0)
                offset.x = -offset.x;

            transform.position = Vector3.Lerp(transform.position, player.transform.position + offset, 0.01f);

            return;
        }

        DefaultAI();


        _creatureAbility.AbilityUpdate();
    }

    void Revive()
    {
        if (_character.IsDead)
        {
            _reviveElasped += Time.deltaTime;
            if (_reviveElasped > _reviveTime)
            {
                _character.Revive();
                _reviveElasped = 0;
                _model.gameObject.SetActive(true);
            }
        }
    }
    void DefaultAI()
    {
        if (_closeEnemy) return;
        if (_character.IsAttack) return;

        if (_normalAttackCoolTime > _normalAttackTime)
        {
            _normalAttackTime += Time.deltaTime;
            FollowPlayer();
        }
        else
        {
            if(_closeEnemy == null)
                _closeEnemy = GetCloseEnemy();

            if (_closeEnemy == null)
                FollowPlayer();

            StartCoroutine(CorPlayNormalAttack());
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
                if (_closeEnemy == null || _character.IsAttack) break;
                _character.Move(_closeEnemy.transform.position - transform.position);

                if (Util.RangeCastAll2D(gameObject, attackRange, Define.CharacterMask, (hit) =>
                {
                    Character character = hit.collider.GetComponent<Character>();
                    if (character != null && character.CharacterType == Define.CharacterType.Enemy)
                    {
                        return true;
                    }

                    return false;
                }).Count > 0) break;
                yield return null;
            }
            if (_closeEnemy != null && !_character.IsAttack)
            {
                _character.Move(Vector2.zero);
                _character.TurnBody(_closeEnemy.transform.position - transform.position);

                _character.SetAnimationSpeed(AttackSpeed);
                _character.AnimatorSetTrigger("NormalAttack");
                _character.IsAttack = true;
                _character.IsEnableMove = false;
                _character.IsEnableTurn = false;

                while (_character.IsAttack)
                {
                    yield return null;
                }
            }
        }

        _normalAttackTime = 0;
        _closeEnemy = null;
    }
    void FollowPlayer()
    {
        if (_character.IsAttack) return;
        if (_player == null)
        {
            _player = Managers.GetManager<GameManager>().Player;
            return;
        }

        Vector3 distacne = _player.transform.position - transform.position;
        if (Mathf.Abs(distacne.y) < 3 && Mathf.Abs(distacne.x) > _girlToCreatureDistance)
        {
            _character.Move(Vector3.right * (distacne.x + (distacne.x > 0 ? -_girlToCreatureDistance : _girlToCreatureDistance)) / (_girlToCreatureDistance));
        }
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
        if (CreatureAbility.GetIsHaveAbility(CardName.분노))
        {
            _character.Attack(_character, 1, 0, Vector3.zero, _character.GetCenter(), 0);
        }
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
        Character player = Managers.GetManager<GameManager>().Girl;

        if (player != null)
            Util.RangeCastAll2D(player.gameObject, _normalAttackDetectRange, Define.CharacterMask, (hit) =>
            {
                if (hit.collider != null)
                {
                    Character character = hit.collider.GetComponent<Character>();
                    if (character == null || character.CharacterType != Define.CharacterType.Enemy || character.IsEnableFly) return false;
                    if (close == null || (close.transform.position - player.transform.position).magnitude > (character.transform.position - player.transform.position).magnitude)
                        close = character;
                }
                return true;
            });

        return close;
    }

   
}