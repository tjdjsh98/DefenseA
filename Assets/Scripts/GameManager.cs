using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : Manager
{
    [SerializeField] GameObject _enemyOrigin;
    [SerializeField] GameObject _enemySpawnPoint;

    List<GameObject> _enemySpawnList = new List<GameObject>();
    int _spawnCount;
    int _maxSpawnCount = 5;
    bool _isStartWave;
    float _time;

    public override void Init()
    {
    }

    public override void ManagerUpdate()
    {
        if (!_isStartWave)
        {
            _time += Time.time;
            if (_time > 3.0f)
            {
                StartWave();
                _time = 0;
            }
        }
        PlayWave();
    }

    void StartWave()
    {
        if (_isStartWave) return;
        _isStartWave= true;
        Debug.Log("StartWave");
    }

    void PlayWave()
    {
        if (!_isStartWave) return;

        _time += Time.deltaTime;

        if(_time > 2f && _spawnCount < _maxSpawnCount)
        {
            GameObject go = Instantiate(_enemyOrigin);
            go.transform.position = _enemySpawnPoint.transform.position;
            _enemySpawnList.Add(go);
            _spawnCount++;
        }

        if(_spawnCount == _maxSpawnCount && _enemySpawnList.Count <= 0)
        {
            EndWave();
        }
    }

    void EndWave()
    {
        Debug.Log("EndWave");
        _isStartWave = false;
        _time = 0;
        _spawnCount = 0;
    }
}
