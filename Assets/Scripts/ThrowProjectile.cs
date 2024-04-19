using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowProjectile : MonoBehaviour
{
    Character _character;
    EnemyAI _enemyAI;


    bool _isStartFire;
    [SerializeField] float _fireSpeed = 10;
    float _fireTime;
    [SerializeField]float _fireDelay = 1f;
    [SerializeField] float _fireCoolTime = 3;
    [SerializeField] int _fireMaxCount = 3;
    [SerializeField] Define.ProjectileName _projectileName;
    int _fireCount = 0;
    private void Awake()
    {
        _character = GetComponent<Character>();
        _enemyAI = GetComponent<EnemyAI>();
    }

    private void Update()
    {
        if(_enemyAI.Target == null) return;
        if (_fireCoolTime > _fireTime && !_isStartFire)
        {
            _fireTime += Time.deltaTime;
            return;
        }
        if(!_isStartFire)
        {
            _fireTime = 0;
            _isStartFire= true;
        }

        if(_fireTime < _fireDelay)
        {
            _fireTime += Time.deltaTime;
        }
        else
        {
            if (_fireCount < _fireMaxCount)
            {
                Projectile projectile = Managers.GetManager<ResourceManager>().Instantiate<Projectile>((int)_projectileName);
                if (projectile != null)
                {
                    Vector3 destionationPosition = _enemyAI.Target.GetCenter() + _enemyAI.Target.MySpeed * (_enemyAI.Target.GetCenter() - transform.position).magnitude/ _fireSpeed;
                    projectile.transform.position = transform.position;
                    projectile.Init(5,_fireSpeed, _character.AttackPower, Define.CharacterType.Player);
                    projectile.Fire(_character, destionationPosition -  transform.position);
                }
                _fireCount++;
            }
            else
            {
                _fireCount = 0;
                _isStartFire = false;
            }
            _fireTime = 0;
        }

    }
}
