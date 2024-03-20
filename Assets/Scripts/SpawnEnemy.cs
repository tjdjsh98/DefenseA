using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnEnemy : MonoBehaviour
{
    Character _character;
    [SerializeField] List<Define.EnemyName> _spawnEnemyList;
    [SerializeField] float _spawnCoolTime;
    [SerializeField] float _spawnTime;

    float _spawnReadyTime;
    float _spawnPlayTime = 2;

    [SerializeField] float _spawnEnemyHpMultifly;

    private void Awake()
    {
        _character = GetComponent<Character>();
    }

    private void Update()
    {
        _spawnTime += Time.deltaTime;

        if (_spawnCoolTime > _spawnTime)
        {
            _spawnTime += Time.deltaTime;
        }
        else
        {
            if (_spawnReadyTime == 0)
            {
                _character.AnimatorSetTrigger("Ready");
            }
            if (_spawnEnemyList.Count <= 0)
            {
                _spawnTime = 0;
                return;
            }

            _spawnReadyTime += Time.deltaTime;
            if (_spawnReadyTime > _spawnPlayTime)
            {
                _spawnTime = 0;
                _spawnReadyTime = 0;
                EnemyNameDefine enemyOrigin = Managers.GetManager<DataManager>().GetData<EnemyNameDefine>((int)_spawnEnemyList.GetRandom());
                EnemyNameDefine enemy = Managers.GetManager<ResourceManager>().Instantiate(enemyOrigin);
                Character character = enemy.GetComponent<Character>();
                if (character != null)
                    character.MulHp(_spawnEnemyHpMultifly);
                enemy.transform.position = transform.position;
                _character.AnimatorSetTrigger("Play");

            }
        }
    }
}
