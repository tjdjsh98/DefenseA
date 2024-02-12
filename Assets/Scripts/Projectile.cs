using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.Rendering;
using UnityEngine.VFX;

public class Projectile : MonoBehaviour
{
    Rigidbody2D _rigid;
    TrailRenderer _trailRenderer;

    float _power;
    float _speed;
    int _damage;
    int _penerstratingPower;
    int _penerstrateCount;
    float _time;

    Vector3 _direction;
    private Character _attacker;

    bool _isAttack;

    Define.CharacterType _enableAttackCharacterType;


    Vector3 _prePostion;
    List<GameObject> _attackCharacterList = new List<GameObject>();


    private void Awake()
    {
        _rigid = GetComponent<Rigidbody2D>();
        _trailRenderer = GetComponent<TrailRenderer>();
    }

    public void Init(float power, float speed, int damage,Define.CharacterType enableAttackCharacterType,int penetratingPower= 0)
    {
        _trailRenderer.Clear();
        _power = power;
        _speed = speed;
        _damage = damage;
        _enableAttackCharacterType = enableAttackCharacterType;
        _penerstratingPower = penetratingPower;
        _isAttack = false;
        _time = 0;
        _penerstrateCount = 0;
        _attackCharacterList.Clear();
        gameObject.SetActive(true);
    }

    private void Update()
    {
        _time += Time.deltaTime;
        if(_time > 3)
        {
            Managers.GetManager<ResourceManager>().Destroy(gameObject);
        }
        else
        {
            CheckCollision();
        }
    }

    void CheckCollision()
    {
        if (_isAttack) return;
        if(transform.position == _prePostion) return;

        Vector3 direction = (transform.position - _prePostion);

        RaycastHit2D[] hits = Physics2D.CircleCastAll(_prePostion, 0.5f, direction.normalized, direction.magnitude, LayerMask.GetMask("Character"));

        _prePostion = transform.position;

        foreach (var hit in hits)
        {
            if (_attackCharacterList.Contains(hit.collider.gameObject)) continue;

            _attackCharacterList.Add(hit.collider.gameObject);

            Character character = null;
            if (character = hit.collider.GetComponent<Character>())
            {
                if (character.Hp > 0 && character.CharacterType == _enableAttackCharacterType)
                {

                    _direction = _direction.normalized;
                    character.Damage(_attacker, _damage, _power, _direction);
                    Effect flare = Managers.GetManager<ResourceManager>().Instantiate<Effect>("Prefabs/Effect/Flare");
                    flare.SetProperty("Direction", _direction * -1);
                    flare.Play(transform.position);
                    _penerstrateCount++;

                    if (_penerstrateCount > _penerstratingPower)
                    {
                        Managers.GetManager<ResourceManager>().Destroy(gameObject);
                        _isAttack = true;
                        return;
                    }
                }
            }
        }
    }

    public virtual void Fire(Character attacker, Vector3 direction)
    {
        direction.z = 0;    
        _attacker = attacker;

        Vector3 dir = direction.normalized + attacker.MySpeed.normalized;
        _rigid.velocity = (Vector2)direction.normalized * (_speed + (attacker.MySpeed.magnitude ));
        _direction = direction.normalized;
        _prePostion = transform.position;
    }
}
