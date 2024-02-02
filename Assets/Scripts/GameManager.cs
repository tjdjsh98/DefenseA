using System.Collections;
using System.Collections.Generic;
using System.Data.Common;
using Unity.Properties;
using UnityEngine;
using UnityEngine.WSA;

public class GameManager : ManagerBase
{

    // 가족 변수
    public Player Player { set; get; }

    public FatherAI FatherAI;

    Character _daugther;
    public Character Daughter { get { if (_daugther == null) _daugther = Player?.GetComponent<Character>(); return _daugther; } }

    public DogAI DogAI { set; get; }
    Character _dog;
    public Character Dog { get { if (_dog == null) _dog = DogAI?.GetComponent<Character>(); return _dog; } }

    Character _father;
    public Character Father { get { if (_father == null) _father = FatherAI?.GetComponent<Character>(); return _father; } }



    // 게임 진행 변수
    [SerializeField] bool _stop;
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
    List<CardSelectionData> _remainCardSelectionList;
    Dictionary<Define.CardSelection, int> _cardSelectionCount = new Dictionary<Define.CardSelection, int>();
    

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
        _remainCardSelectionList = Managers.GetManager<DataManager>().GetDataList<CardSelectionData>();
        _map = new Map(180f);
        _map.SetCenterGround(GameObject.Find("Ground"));
    }

  
    public override void ManagerUpdate()
    {
        _totalTime += Time.deltaTime;
        HandleGround();
        if (_stop) return;
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

        if(Player.transform.position.x > MapSize)
        {
            _stop = true;

            for(int i = _enemySpawnList.Count-1; i >= 0; i--)
            {
                Managers.GetManager<ResourceManager>().Destroy(_enemySpawnList[i]);
                _enemySpawnList.RemoveAt(i);
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
            int count = Random.Range(1, 2);

            for(int i = 0; i < count; i++)
            {

                EnemyAI enemyOrigin = Managers.GetManager<DataManager>().GetData<EnemyAI>(Random.Range(0,Define.ENEMY_COUNT));
                EnemyAI enemy = Managers.GetManager<ResourceManager>().Instantiate(enemyOrigin);
                Character enemyCharacter = enemy.GetComponent<Character>();
                enemyCharacter.SetHp((int)(enemyCharacter.MaxHp *(1+ _totalTime/120)));

                Vector3 randomPosition = Vector3.zero;
                float distance = 50;
                if (enemyCharacter.IsEnableFly)
                {
                    float angle = 0;
                    if(Random.Range(0,2) == 0)
                        angle = Random.Range(30, 70);
                    else
                        angle = Random.Range(110, 150);

                    angle = angle * Mathf.Deg2Rad;
                    randomPosition.x = Player.transform.position.x + Mathf.Cos(angle) * distance; 
                    randomPosition.y = Player.transform.position.y + Mathf.Sin(angle) * distance;

                }
                else
                {

                    randomPosition.x = Player.transform.position.x + (enemy.EnemyName == Define.EnemyName.Walker1 ? distance : -distance);
                    randomPosition.y = _map.YPosition;
                }
                enemyCharacter.transform.position =  randomPosition;
                if(enemy)
                    _enemySpawnList.Add(enemy.gameObject);
            }

            _genTime = 0;
        }
        else
        {
            _genTime += Time.deltaTime;
        }
    }

    public CardSelectionData GetRandomCardSelectionData()
    {
        return _remainCardSelectionList.GetRandom();
    }

    public void SelectCardData(CardSelectionData data)
    {
        if (!_cardSelectionCount.ContainsKey(data.CardSelection))
            _cardSelectionCount.Add(data.CardSelection ,0);
        _cardSelectionCount[data.CardSelection]++;

        if(data.MaxUpgradeCount <= _cardSelectionCount[data.CardSelection])
        {
            _remainCardSelectionList.Remove(data);
        }

        // TODO
        // 능력적용

        if(data.CardSelectionType == Define.CardSelectionType.Weapon)
        {
            WeaponCardSelection weaponCardSelection = data as WeaponCardSelection;

            WeaponSwaper swaper = Player.GetComponent<WeaponSwaper>();

            swaper.ChangeNewWeapon(weaponCardSelection.WeaponSlotIndex, weaponCardSelection.WeaponName);
        }
        else
        {
            if(data.CardSelection == Define.CardSelection.최대체력증가)
            {
                Daughter.SetMaxHp(Daughter.MaxHp + 2);
            }
            
            if(data.CardSelection == Define.CardSelection.반동제어)
            {
                Player.SetReboundControlPower(Player.ReboundControlPower + 10);
            }
            if(data.CardSelection == Define.CardSelection.패밀리어스피어능력해제)
            {
                FatherAI.IsUnlockSpear= true;
            }
            if(data.CardSelection == Define.CardSelection.방벽크기증가)
            {
                Vector3 scale = Dog.transform.localScale;
                scale.x += 0.1f;
                scale.y += 0.1f;
                Dog.transform.localScale = scale;
            }
            if(data.CardSelection == Define.CardSelection.방벽최대체력증가)
            {
                Dog.SetMaxHp(Dog.MaxHp + 5);
            }
        }
    }
    public int GetCardSelectionCount(Define.CardSelection cardSelection)
    {
        int count = 0;
        _cardSelectionCount.TryGetValue(cardSelection, out count);

        return count;
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
    public float YPosition => yPosision;

    GameObject groundFolder;
    public Map(float groundTenm)
    {
        this.groundTerm = groundTenm;
        groundFolder = new GameObject("GroundFolder");
        groundFolder.AddComponent<Rigidbody2D>().isKinematic = true;
        groundFolder.AddComponent<CompositeCollider2D>();
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
        left.transform.SetParent(groundFolder.transform);
        right.transform.SetParent(groundFolder.transform);
        center.transform.SetParent(groundFolder.transform);
    }
    public int GetIndex(float x)
    {
        int index = (int)(x / (groundTerm ));
        return index;
    }
}