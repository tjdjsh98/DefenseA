using UnityEditor.Rendering;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    protected Character _character;
    
    [SerializeField]protected Define.Range _attackRange;

    [SerializeField] protected Character _target;

    // 공격과 공격사이의 딜레이
    [SerializeField] protected float _attackDelay = 1;
    protected  float _attackElapsed;

    // 공격하는 중의 딜레이
    protected float _attackingDelay = 2;
    protected float _attackingElasepd = 0;

    [SerializeField] bool _isRangedAttack;
    public bool IsAutoMove { set; get; } = true;
    [SerializeField] string _attackTriggerName;

    SpriteRenderer _attackRangeSprite;

    protected virtual void Awake()
    {
        _character = GetComponent<Character>();
        _character.PlayAttackHandler += OnPlayAttack;
        _character.FinishAttackHandler += FinishAttack;
        _character.CharacterDead += () =>
        {
            Managers.GetManager<GameManager>().Exp += 1;
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

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Define.Range range = _attackRange;
        range.center.x = transform.localScale.x > 0 ? range.center.x : -range.center.x;
        Gizmos.DrawWireCube(transform.position + range.center, range.size);
    }
    void Update()
    {
        CheckTarget();
        Move();
        PlayAttack();
    }

    protected virtual void Move()
    {
        if (_target != null) return;
        if (_character.IsAttack) return;
        if (_character.IsStun) return;

        if (!IsAutoMove) return;
        Player player = Managers.GetManager<GameManager>().Player;
        if(player == null) return;
        _character.Move((player.transform.position - transform.position).normalized);
    }

    protected virtual void CheckTarget()
    {
        if (_character.IsAttack) return;
        if (_character.IsStun) return;
        if (_target != null) return;
        GameObject[] gameObjects = Util.RangeCastAll2D(gameObject, _attackRange);

        if(gameObjects.Length > 0 ) 
        { 
            foreach(var gameObject in gameObjects)
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

    protected virtual void PlayAttack()
    {
        if (_attackElapsed < _attackDelay)
        {
            _attackElapsed += Time.deltaTime;
        }
        else
        {
            if (_target == null) return;

            
            if (!_isRangedAttack)
            {
                // 공격 모션이 따로 있을 때
                if (!_attackTriggerName.Equals(""))
                {
                    _character.IsEnableMove = false;
                    _character.IsEnableTurn = false;
                    _character.IsAttack = true;
                    _character.TurnBody(_target.transform.position - transform.position);
                    _character.AnimatorSetTrigger(_attackTriggerName);
                }
                // 공격 모션이 따로 없을 때
                else
                {
                    if (_attackingElasepd == 0)
                    {
                        Color color = _attackRangeSprite.color;
                        color.a = 0;
                        _attackRangeSprite.color = color;
                        _attackRangeSprite.transform.localScale = _attackRange.size;
                        _attackRangeSprite.transform.localPosition = _attackRange.center;

                        _attackRangeSprite.gameObject.SetActive(true);
                        _character.IsAttack = true;
                    }

                    if (_attackingDelay > _attackingElasepd)
                    {
                        _attackingElasepd += Time.deltaTime;
                        Color color = _attackRangeSprite.color;
                        if(_attackingDelay != 0)
                            color.a = _attackingElasepd/_attackingDelay;
                        _attackRangeSprite.color = color;

                    }
                    else
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

     void OnPlayAttack()
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
