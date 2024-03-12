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
                Projectile projectile = Managers.GetManager<ResourceManager>().Instantiate("Prefabs/Projectile").GetComponent<Projectile>();

                if (projectile != null)
                {
                    projectile.transform.position = transform.position;
                    projectile.Init(_fireSpeed, 20, _character.AttackPower, Define.CharacterType.Player);
                    projectile.Fire(_character, _enemyAI.Target.GetCenter() - transform.position);
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
