using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.VFX;

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

    Define.CharacterType _enableAttackCharacterType;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_isAttack) return;
        Character character = null;
        if (character = collision.gameObject.GetComponent<Character>())
        {
            if (character.Hp > 0 && character.CharacterType == _enableAttackCharacterType)
            {
                _direction = _direction.normalized;
                character.Damage(_attacker, _damage, _power, _direction);
                Effect flare = Managers.GetManager<ResourceManager>().Instantiate<Effect>("Prefabs/Effect/Flare");
                flare.SetProperty("Direction", _direction*-1);
                flare.Play(collision.ClosestPoint(transform.position));
                Destroy(gameObject);
                _isAttack = true;
            }
        }
    }

    public void Init(float power, float speed, int damage,Define.CharacterType enableAttackCharacterType)
    {
        _rigid = GetComponent<Rigidbody2D>();
        _power = power;
        _speed = speed;
        _damage = damage;
        _enableAttackCharacterType = enableAttackCharacterType;
    }

    private void Update()
    {
        _time += Time.deltaTime;
        if(_time > 5)
        {
            Destroy(gameObject);
        }
    }

    public virtual void Fire(Character attacker, Vector3 direction)
    {
        direction.z = 0;    
        _attacker = attacker;

        Vector3 dir = direction.normalized + attacker.MySpeed.normalized;
        _rigid.velocity = (Vector2)direction.normalized * (_speed + (attacker.MySpeed.magnitude ));
        _direction = direction.normalized;
    }
}
