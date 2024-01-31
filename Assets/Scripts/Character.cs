using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class Character : MonoBehaviour
{
    Rigidbody2D _rigidBody;

    [Header("캐릭터 능력치")]
    [SerializeField] Define.CharacterType _characterType;
    public Define.CharacterType CharacterType=>_characterType;
    [SerializeField] int _maxHp;
    public int MaxHp => _maxHp;
    [SerializeField] int _hp;
    public int Hp => _hp;

    [SerializeField] float _speed;
    [SerializeField] bool _isEnableFly;

    float _stunTime;

    // 캐릭터 행동상태
    [SerializeField] bool _isTurnBodyAlongVelocity = true;
    public bool IsStun {private set; get; }
    public bool IsAttack {private set; get; }
    bool IsMove = false;

    [field:SerializeField]public bool IsEnableMove { set; get; } = true;
    Character _attackTarget;

    public Action<Character> CharacterAttack;
    public Action CharacterDead;

    [SerializeField] GaugeBar _hpBar;

    public List<Define.Passive> CharacterPassive = new List<Define.Passive>();


    Animator _animator;

    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _hp = _maxHp;

        if (_isEnableFly)
            _rigidBody.isKinematic = true;

    }

    private void Update()
    {
        ControlAnimation();
        if (_hpBar)
        {
            _hpBar.SetRatio(_hp, _maxHp);
        }

        if (IsStun)
        {
            _stunTime += Time.deltaTime;
            if (_stunTime > 0.5f)
            {
                IsStun = false;
                _stunTime = 0;
            }
        }
        if(IsAttack)
        {
            _stunTime += Time.deltaTime;
            if (_stunTime > 0.5f)
            {
                IsAttack = false;
                _stunTime = 0;
            }
        }
    }

    void ControlAnimation()
    {
        if (!_animator) return;
        if(!IsStun)
        {
            if(IsMove)
            {
                _animator.SetBool("Walk", true);
            }
            else
            {
                _animator.SetBool("Walk", false);
            }
            IsMove = false;
        }
    }

    public void SetHp(int hp)
    {
        _maxHp = hp;
        _hp = hp;
    }

    public void Damage(Character attacker, int damage, float power, Vector3 direction)
    {

        Managers.GetManager<TextManager>().ShowText(transform.position + Vector3.up, damage.ToString(), 10, Color.red);

        _hp -= damage;
        _rigidBody.velocity = Vector2.zero;
        _rigidBody.AddForce(direction.normalized * power, ForceMode2D.Force);


        if (_hp <= 0)
        {
            CharacterDead?.Invoke();
            Destroy(gameObject);
        }

        IsStun = true;
        _stunTime = 0;
    }

    public void Move(Vector2 direction)
    {
        if (IsStun) return;

        // 진행 방향에 맞게 몸을 회전
        if (_isTurnBodyAlongVelocity)
        {
            if (direction.x > 0)
                transform.localScale = new Vector3(1, 1, 1);
            else if (direction.x < 0)
                transform.localScale = new Vector3(-1, 1, 1);
        }

        direction.x = Mathf.Clamp(direction.x, -1, 1);
        direction.y = Mathf.Clamp(direction.y, -1, 1);

        // 움직임 제어
        if (IsEnableMove)
        {
            IsMove = true;
            if (_isEnableFly)
                _rigidBody.velocity = new Vector2(direction.x * _speed, direction.y * _speed);
            else
                _rigidBody.velocity = new Vector2(direction.x * _speed, _rigidBody.velocity.y);
        }
    }

    public void Attack(Character character)
    {
        if (IsStun || IsAttack) return;

        IsAttack = true;
        _attackTarget = character;
        CharacterAttack?.Invoke(_attackTarget);
    }
}
