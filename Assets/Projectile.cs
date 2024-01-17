using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    Rigidbody2D _rigid;

    float _power;
    float _speed;
    int _damage;

    float _time;

    Vector3 _direction;
    private Character _attacker;

    bool _isAttack;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_isAttack) return;

        Character character = null;
        if (character = collision.gameObject.GetComponent<Character>())
        {
            _direction.y = 0;
            _direction = _direction.normalized;
            character.Damage(_attacker, _damage, _power, _direction);
            Destroy(gameObject);
            _isAttack = true;
        }
    }

    public void Init(float power, float speed, int damage)
    {
        _rigid = GetComponent<Rigidbody2D>();
        _power = power;
        _speed = speed;
        _damage = damage;
    }

    private void Update()
    {
        _time += Time.deltaTime;
        if(_time > 5)
        {
            Destroy(gameObject);
        }
    }

    public void Fire(Character attacker, Vector3 direction)
    {
        direction.z = 0;    
        _attacker = attacker;
        _rigid.velocity = direction.normalized * _speed;
        _direction = direction.normalized;
    }
}
