using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class Character : MonoBehaviour
{
    Rigidbody2D _rigidBody;

    [SerializeField] Define.CharacterType _characterType;
    public Define.CharacterType CharacterType=>_characterType;
    [SerializeField] int _maxHp;
    public int MaxHp => _maxHp;
    [SerializeField] int _hp;
    public int Hp => _hp;

    [SerializeField] float _speed;

    float _stunTime;

    public bool IsStun {private set; get; }
    public bool IsAttack {private set; get; }

    Character _attackTarget;
    public Action<Character> CharacterAttack;

    public Action CharacterDead;

    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _hp = _maxHp;

    }

    private void Update()
    {
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

    public void SetHp(int hp)
    {
        _maxHp = hp;
        _hp = hp;
    }

    public void Damage(Character attacker, int damage, float power, Vector3 direction)
    {
        _hp -= damage;
        _rigidBody.velocity = Vector2.zero;
        _rigidBody.AddForce(direction.normalized * power, ForceMode2D.Force);

        if (_hp <= 0)
        {
            CharacterDead?.Invoke();
            Destroy(gameObject);
        }

        IsStun = true;
    }

    public void Move(Vector2 direction)
    {
        if (IsStun) return;
        if (direction.x > 0)
            transform.localScale = new Vector3(1, 1, 1);
        else if(direction.x < 0)
            transform.localScale = new Vector3(-1, 1, 1);

        _rigidBody.velocity = new Vector2(direction.x * _speed, _rigidBody.velocity.y);
    }

    public void Attack(Character character)
    {
        if (IsStun || IsAttack) return;

        IsAttack = true;
        _attackTarget = character;
        CharacterAttack?.Invoke(_attackTarget);
    }
}
