using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

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


    protected virtual void Awake()
    {
        _character = GetComponent<Character>();

        _character.CharacterAttack += Attack;
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
        Define.Range range = _attackRange;
        range.center.x = transform.lossyScale.x > 0 ? range.center.x : -range.center.x;

        GameObject[] gameObjects = Util.BoxcastAll2D(gameObject, range);

        if(gameObjects.Length > 0 ) 
        { 
            foreach(var gameObject in gameObjects)
            {
                if (gameObject == this.gameObject) continue;
                _target = gameObject.GetComponent<Character>();
                if (_target)
                {
                    if (_target.CharacterType == Define.CharacterType.Enemy)
                        _target = null;
                    else
                        return;
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
                Attack(_target);
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

    public void Attack(Character character)
    {
        character.Damage(_character, 1, 1, Vector3.zero);
        _target = null;
    }

    public int GetEnumToInt()
    {
        return (int)_enemyName;
    }
}
