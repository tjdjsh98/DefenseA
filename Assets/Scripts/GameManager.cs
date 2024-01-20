using System.Collections;
using System.Collections.Generic;
using System.Net.Http.Headers;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class GameManager : ManagerBase
{
    Player _player;
    public Player Player=>_player;

    [SerializeField]Character _building;
    public Character Building => _building;

    [SerializeField] GameObject _enemyOrigin;
    [SerializeField] GameObject _enemySpawnPoint;

    List<GameObject> _enemySpawnList = new List<GameObject>();

    [SerializeField] List<Wave> _waveList;
    int _currentWave = 0;
    public int CurrentWave => _currentWave;

    int _spawnCount;
    int _maxSpawnCount = 5;
    bool _isStartWave;
    public bool IsStartWave => _isStartWave;
    float _time;

    private int _money;
    public int Money
    {
        set
        {
            _money = value;
        }
        get
        {
            return _money;
        }
    }

    public override void Init()
    {
    }

    public void SetPlayer(Player player)
    {
        _player = player;
    }
    public override void ManagerUpdate()
    {
        if (!_isStartWave && !Managers.GetManager<UIManager>().GetUI<UIShop>().gameObject.activeSelf)
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

        _currentWave++;
        _isStartWave= true;
    }

    void PlayWave()
    {
        if (!_isStartWave) return;

        _time += Time.deltaTime;

        if(_time > 2f && _spawnCount < _maxSpawnCount)
        {
            Wave wave = null;

            if (_waveList.Count > _currentWave)
                wave = _waveList[_currentWave];
            else
                wave = _waveList[0];

            Character character = Instantiate(wave.characterList[_spawnCount]);
            character.transform.position = _enemySpawnPoint.transform.position;
            character.SetHp((_currentWave + 1) * 3);

            character.CharacterDead += () =>
            {
                _enemySpawnList.Remove(character.gameObject);
            };
            _enemySpawnList.Add(character.gameObject);
            _spawnCount++;
            _time = 0;
        }

        if(_spawnCount == _maxSpawnCount && _enemySpawnList.Count <= 0)
        {
            EndWave();
        }
    }

    void EndWave()
    {
        _isStartWave = false;
        _time = 0;
        _spawnCount = 0;
        Managers.GetManager<UIManager>().GetUI<UIShop>().Open();
    }
}

[System.Serializable]
class Wave
{
    public List<Character> characterList;
}