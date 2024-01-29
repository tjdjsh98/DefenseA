using System.Collections;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;

public class GameManager : ManagerBase
{
    Player _player;
    public Player Player => _player;

    [SerializeField] Character _building;
    public Character Building => _building;

    FamiliarAI _familiar;
    public FamiliarAI Familiar;

    [field: SerializeField]public  float MapSize { set; get; }
    // 플레이 경험 관련
    [Header("경험치")]
    [SerializeField] int _exp;
    public int Exp {
        set
        {
            _exp = value;
            if (_maxExpList.Count > Level)
            {
                if (_exp >= _maxExpList[Level])
                {
                    _exp = _exp - _maxExpList[Level];
                    Level++;
                    Managers.GetManager<UIManager>().GetUI<UICardSelection>().Open();
                }
            }
        }
        get { return _exp; }
    }

    public int MaxExp
    {
        get
        {
            int maxExp = 9999999;
            if (_maxExpList.Count > Level)
            {
                maxExp = _maxExpList[Level];
            }

            return maxExp;
        }
    }
    [SerializeField] List<int> _maxExpList;
    public int Level { set; get; }

    // 적 소환 관련 변수
    [Header("적관련변수")]
    [SerializeField] GameObject _enemySpawnPoint;
    [SerializeField] bool _isEndless;
    List<GameObject> _enemySpawnList = new List<GameObject>();
    [SerializeField] List<Wave> _waveList;
    int _currentWave = 0;
    public int CurrentWave => _currentWave;
    int _spawnCount;
    int _maxSpawnCount = 5;
    bool _isStartWave;
    public bool IsStartWave => _isStartWave;
    float _totalTime;
    float _genTime;

    [Header("카드 선택지")]
    [field:SerializeField]public List<CardSelectionInfo> CardSelectionInfoList;


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

    public bool IsStopWave { get; set; }

    // 땅과 관련변수
    Map _map;

    public override void Init()
    {
        _map = new Map(90f);
        _map.SetCenterGround(GameObject.Find("Ground"));
    }

    public void SetPlayer(Player player)
    {
        _player = player;
    }
    public override void ManagerUpdate()
    {
        _totalTime += Time.deltaTime;
        HandleGround();
        if (!IsStopWave)
        {
            if (!_isEndless)
            {
                if (!_isStartWave && !Managers.GetManager<UIManager>().GetUI<UIShop>().gameObject.activeSelf)
                {
                    _genTime += Time.time;
                    if (_genTime > 3.0f)
                    {
                        StartWave();
                        _genTime = 0;
                    }
                }
                PlayWave();
            }
            else
            {
                EndlessWave();
            }
        }
    }

    void HandleGround()
    {
        if (Player == null) return;

        float playerX = Player.transform.position.x;
        int index = _map.GetIndex(playerX);

        _map.SetGround(index);
    }

    void StartWave()
    {
        if (_isStartWave) return;

        _currentWave++;
        if (_currentWave >= _waveList.Count)
            _currentWave = 0;
        _maxSpawnCount = _waveList[_currentWave].characterList.Count;
        _isStartWave= true;
    }

    void PlayWave()
    {
        if (!_isStartWave) return;

        _genTime += Time.deltaTime;

        if(_genTime > 2f && _spawnCount < _maxSpawnCount)
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
            _genTime = 0;
        }

        if(_spawnCount == _maxSpawnCount && _enemySpawnList.Count <= 0)
        {
            EndWave();
        }
    }

    void EndWave()
    {
        _isStartWave = false;
        _genTime = 0;
        _spawnCount = 0;
        Managers.GetManager<UIManager>().GetUI<UIShop>().Open();
    }

    void EndlessWave()
    {
        float genTime = 100;
        if (_totalTime < 30)
            genTime = 2f;
        else if (_totalTime < 120)
            genTime = 1.5f;
        else if (_totalTime < 240)
            genTime = 1.1f;

        if (_genTime > genTime)
        {
            int count = Random.Range(1, 4);

            for(int i = 0; i < count; i++)
            {
                GameObject enemy = Managers.GetManager<ResourceManager>().Instantiate("FlyingEnemy");
                Vector3 random = Random.onUnitSphere;
                random.y = random.y < 0 ? -random.y : random.y;
                random.z = 0;
                random = random.normalized * 50;
                enemy.transform.position = _player.transform.position + random;
            }

            _genTime = 0;
        }
        else
        {
            _genTime += Time.deltaTime;
        }
    }
}

[System.Serializable]
class Wave
{
    public List<Character> characterList;
}

class Map
{
    int currentIndex = -10002;

    GameObject left;
    GameObject center;
    GameObject right;


    float groundTerm;
    float yPosision = -11.4f;

    public Map(float groundTenm)
    {
        this.groundTerm = groundTenm;
    }

  
    public void SetGround(int index)
    {
        if (index == currentIndex) return;

        if(index > currentIndex)
        {
            GameObject temp = left;
            left = center;
            center = right;
            right = temp;
        }
        else
        {
            GameObject temp = right;
            right = center;
            center = left;
            left = temp;
        }
        currentIndex= index;

        center.transform.position = new Vector3(index * groundTerm, yPosision, 0);
        right.transform.position = new Vector3((index+1) * groundTerm, yPosision, 0);
        left.transform.position = new Vector3((index -1)* groundTerm, yPosision, 0);
        
    }

    public void SetCenterGround(GameObject ground)
    {
        center= ground;
        left = Managers.GetManager<ResourceManager>().Instantiate("Ground");
        right = Managers.GetManager<ResourceManager>().Instantiate("Ground");
    }
    public int GetIndex(float x)
    {
        int index = (int)(x / (groundTerm / 2));
        return index;
    }
}