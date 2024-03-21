using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SpawnEnemy : MonoBehaviour
{
    Character _character;
    Rigidbody2D _rigidbody;
    EnemyAI _enemyAI;

    GameObject _target;

    [SerializeField] List<Define.EnemyName> _spawnEnemyList;
    [SerializeField] float _spawnCoolTime;
    [SerializeField] float _spawnTime;

    float _spawnReadyTime;
    float _spawnPlayTime = 2;

    [SerializeField] float _spawnEnemyHpMultifly;

    private void Awake()
    {
        _character = GetComponent<Character>();
        _rigidbody = GetComponent<Rigidbody2D>();

        _enemyAI = GetComponent<EnemyAI>();
    }

    private void Update()
    {
        _spawnTime += Time.deltaTime;


        if (_target == null && _enemyAI.Target != null)
        {
            _target = _enemyAI.Target.gameObject;
        }

        if (_spawnCoolTime > _spawnTime)
        {
            _spawnTime += Time.deltaTime;
        }
        else if(_target)
        {
            if (_spawnEnemyList.Count <= 0)
            {
                _spawnTime = 0;
                return;
            }
            if (_spawnReadyTime == 0)
            {
                _character.IsEnableMove = false;
                _character.AnimatorSetTrigger("Ready");
            }

            _spawnReadyTime += Time.deltaTime;
            if (_spawnReadyTime > _spawnPlayTime)
            {
                _character.AnimatorSetTrigger("Play");
                _rigidbody.AddForce((transform.position- _target.transform.position).normalized * 100,ForceMode2D.Impulse);

                EnemyNameDefine enemyOrigin = Managers.GetManager<DataManager>().GetData<EnemyNameDefine>((int)_spawnEnemyList.GetRandom());
                EnemyNameDefine enemy = Managers.GetManager<ResourceManager>().Instantiate(enemyOrigin);
                Character character = enemy.GetComponent<Character>();
                if (character != null)
                    character.MulHp(_spawnEnemyHpMultifly);
                enemy.transform.position = transform.position;

                _spawnTime = 0;
                _spawnReadyTime = 0;
                _target = null;
                Invoke("ReleaseMove", 1);
            }
        }
    }


    void ReleaseMove()
    {
        _character.IsEnableMove = true;
    }
}