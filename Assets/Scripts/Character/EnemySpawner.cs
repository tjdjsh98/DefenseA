using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] bool _debug;
    [SerializeField] Define.Range _spawnRange;
    [SerializeField] List<SpawnData> _spawnDataList;

    bool _isSpawn;
    private void OnDrawGizmos()
    {
        if (!_debug) return;

        Util.DrawRangeOnGizmos(gameObject,_spawnRange,Color.blue);

        foreach (var data in _spawnDataList)
        {
            Define.Range range = new Define.Range()
            {
                center = data.spawnPoint,
                size = new Vector3(1f, 0f, 0),
                figureType = Define.FigureType.Circle
            };
            Util.DrawRangeOnGizmos(gameObject, range, Color.yellow);
        }
    }

    public void Update()
    {
        if (_isSpawn) return;

        List<RaycastHit2D> hits = Util.RangeCastAll2D(gameObject, _spawnRange, Define.CharacterMask, (hit) =>
        {
            Character character = hit.collider.GetComponent<Character>();
            return (character !=  null && character.CharacterType == Define.CharacterType.Player);
            });

        if(hits.Count > 0)
        {
            foreach (var data in _spawnDataList)
                StartCoroutine(CorSpawnEnemey(data));
            _isSpawn = true;
        }
    }


    IEnumerator CorSpawnEnemey(SpawnData data)
    {
        yield return new WaitForSeconds(data.delay);

        EnemyNameDefine enemyOrigin = Managers.GetManager<DataManager>().GetData<EnemyNameDefine>((int)data.spawnEnemy);
        EnemyNameDefine enemy = Managers.GetManager<ResourceManager>().Instantiate(enemyOrigin);
        if (enemy.IsGroup)
            enemy.GetComponent<EnemyGroup>().SetHp(data.hpMultifly);
        else
            enemy.GetComponent<Character>().MulHp(data.hpMultifly);

        enemy.transform.position = transform.position + data.spawnPoint;

    }
}

[System.Serializable]
public class SpawnData
{
    public Vector3 spawnPoint;
    public Define.EnemyName spawnEnemy;
    public float hpMultifly;
    public float delay;
}