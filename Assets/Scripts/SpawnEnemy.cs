using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnEnemy : MonoBehaviour
{
    [SerializeField] List<Define.EnemyName> _spawnEnemyList;
    [SerializeField] float _spawnCoolTime;
    [SerializeField] float _spawnTime;

    [SerializeField] float _spawnEnemyHpMultifly;


    private void Update()
    {
        _spawnTime += Time.deltaTime;

        if (_spawnCoolTime > _spawnTime)
        {
            _spawnTime += Time.deltaTime;
        }
        else
        {
            _spawnTime = 0;
            if (_spawnEnemyList.Count <= 0) return;

            EnemyNameDefine enemyOrigin = Managers.GetManager<DataManager>().GetData<EnemyNameDefine>((int)_spawnEnemyList.GetRandom());
            EnemyNameDefine enemy = Managers.GetManager<ResourceManager>().Instantiate(enemyOrigin);
            Character character = enemy.GetComponent<Character>();
            if (character != null)
                character.MulHp(_spawnEnemyHpMultifly);
            enemy.transform.position = transform.position;
        }
    }

}
