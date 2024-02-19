using UnityEngine;

public class EnemyAI : MonoBehaviour, ITypeDefine
{
    protected Character _character;
    [SerializeField] Define.EnemyName _enemyName;
    public Define.EnemyName EnemyName => _enemyName;
    
    [SerializeField]protected Define.Range _attackRange;

    [SerializeField] protected Character _target;

    [SerializeField] protected float _attackDelay = 1;
    protected  float _attackElapsed;

    [SerializeField] bool _isRangedAttack;

    [SerializeField] string _attackTriggerName;

    protected virtual void Awake()
    {
        _character = GetComponent<Character>();
        _character.AttackHandler += Attack;
        _character.FinishAttackHandler += FinishAttack;
        _character.CharacterDead += () =>
        {
            Managers.GetManager<GameManager>().Exp += 1;
        };
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
        if (_character.IsAttack || _character.IsStun) return;

        CheckTarget();
        Move();
        PlayAttack();
    }

    protected virtual void Move()
    {
        if (_target != null) return;

        Player player = Managers.GetManager<GameManager>().Player;
        if(player == null) return;
        _character.Move(player.transform.position - transform.position);
    }

    protected virtual void CheckTarget()
    {
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

            _attackElapsed = 0;
            
            if (!_isRangedAttack)
            {
                if (!_attackTriggerName.Equals(""))
                {
                    _character.IsEnableMove = false;
                    _character.IsEnableTurn = false;
                    _character.IsAttack = true;
                    _character.TurnBody(_target.transform.position - transform.position);
                    _character.AnimatorSetTrigger(_attackTriggerName);
                }
                else
                {
                    Attack();
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

     void Attack()
    {
        GameObject[] gos = Util.RangeCastAll2D(gameObject, _attackRange, LayerMask.GetMask("Character"));

        foreach (var go in gos)
        {
            Character character = go.GetComponent<Character>();
            if(character == null || character.CharacterType == _character.CharacterType) continue;

            character.Damage(_character, 1, 1, Vector3.zero);
        }
        _target = null;
    }

     void FinishAttack()
    {
        _character.IsAttack = false;
        _character.IsEnableMove = true;
        _character.IsEnableTurn = true;

    }
    public int GetEnumToInt()
    {
        return (int)_enemyName;
    }
}
