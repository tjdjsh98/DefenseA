using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.Playables;
using UnityEngine.WSA;

public class GameManager : ManagerBase
{

    // 가족 변수
    public Player Player { set; get; }

    public FatherAI FatherAI;

    Character _daugther;
    public Character Daughter { get { if (Player && _daugther == null) _daugther = Player.GetComponent<Character>(); return _daugther; } }

    public DogAI DogAI { set; get; }
    Character _dog;
    public Character Dog { get { if (DogAI && _dog == null) _dog = DogAI?.GetComponent<Character>(); return _dog; } }

    Character _father;
    public Character Father { get { if (FatherAI &&_father == null) _father = FatherAI?.GetComponent<Character>(); return _father; } }


    [Header("Debug")]
    [SerializeField] bool _summonDummy;
    [SerializeField] int _dummyHp;

    [Header("게임 진행")]
    [SerializeField] bool _stop;
    [field: SerializeField]public  float MapSize { set; get; }


    // 플레이 경험 관련
    [Header("경험치")]
    [SerializeField] bool _allMaxExpFive;
    [SerializeField] int _exp;

    [Header("타임라인")]
    [SerializeField] PlayableDirector _playableDirector;
    [SerializeField] PlayableAsset _enteracneTimeline;
    bool _isPlayTimeline;
    public bool IsPlayTimeline => _isPlayTimeline;

    public int Exp {
        set
        {
            _exp = value;
            if (_exp >= MaxExp)
            {
                _exp = _exp - MaxExp;
                Level++;
                Managers.GetManager<UIManager>().GetUI<UICardSelection>().Open();
            }
        }
        get { return _exp; }
    }
    public int MaxExp
    {
        get
        {
            if (_allMaxExpFive) return 5;
            return Level * 3;
        }
    }
    public int Level { set; get; }

    // 적 소환 관련 변수
    [Header("적관련변수")]
    [SerializeField] bool _isEndless;
    List<GameObject> _enemySpawnList = new List<GameObject>();
    [SerializeField] List<Wave> _waveList;

    [SerializeField]int _currentWave = 0;
    public int CurrentWave => _currentWave;
    int _spawnCount;
    int _maxSpawnCount = 5;
    bool _isStartWave;
    public bool IsStartWave => _isStartWave;
    float _totalTime;
    float _genTime;

    int _nextTime;

    [Header("카드 선택지")]
    List<CardData> _remainCardSelectionList;
    Dictionary<Define.CardName, int> _cardSelectionCount = new Dictionary<Define.CardName, int>();


    Vector3 _preMousePosition;

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
        _remainCardSelectionList = Managers.GetManager<DataManager>().GetDataList<CardData>((d) => { return d.IsStartCard; });
        _map = new Map(60f);
        _map.SetCenterGround(GameObject.Find("Ground"));
        _map.AddBuildingPreset("Prefabs/BuildingPreset1");
        _map.AddBuildingPreset("Prefabs/BuildingPreset2");
        _map.AddBuildingPreset("Prefabs/BuildingPreset3");
        _map.AddBuildingPreset("Prefabs/BuildingPreset4");
        _map.AddMoreBackBuildingPreset("Prefabs/MoreBackBuildingPreset1");

        
        _playableDirector.playableAsset = _enteracneTimeline;
        _playableDirector.Play();
        Invoke("OffTimeline", (float)_playableDirector.duration-0.1f);
        _isPlayTimeline = true;
    }

    void OffTimeline()
    {
        _isPlayTimeline = false;
        _father?.SetVelocityForcibly(Vector3.zero);
        _daugther?.SetVelocityForcibly(Vector3.zero);
        _dog?.SetVelocityForcibly(Vector3.zero);
    }
    public override void ManagerUpdate()
    {
        _totalTime += Time.deltaTime;
        HandleGround();
        if (_summonDummy)
        {
            EnemyAI enemyOrigin = Managers.GetManager<DataManager>().GetData<EnemyAI>((int)Define.EnemyName.Walker1);
            EnemyAI enemy = Managers.GetManager<ResourceManager>().Instantiate(enemyOrigin);
            enemy.transform.position = Player.transform.position + Vector3.right*3;
            Character enemyCharacter = enemy.GetComponent<Character>();
            enemyCharacter.SetHp(_dummyHp);
            _summonDummy = false;
        }


        if (_stop) return;
        if (!IsStopWave)
        {
            if (_isEndless)
            {
                EndlessWave();
            }
        }

        
        if(Player && Player.transform.position.x > MapSize)
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

        _map.Update(Player);
    }

    void EndlessWave()
    {
        if (_currentWave < _waveList.Count-1)
        {
            if (_totalTime > _waveList[_currentWave + 1].time)
                _currentWave++;
        }

        if (_genTime > _waveList[_currentWave].genTime)
        {
            _genTime = 0;
            if (_waveList[_currentWave].enemyList == null) return;

            EnemyAI enemyOrigin = Managers.GetManager<DataManager>().GetData<EnemyAI>((int)_waveList[_currentWave].enemyList.GetRandom());
            EnemyAI enemy = Managers.GetManager<ResourceManager>().Instantiate(enemyOrigin);
            Character enemyCharacter = enemy.GetComponent<Character>();
            enemyCharacter.SetHp((int)(enemyCharacter.MaxHp * _waveList[_currentWave].hpMultiply));

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
                randomPosition.x = Player.transform.position.x +distance;
                randomPosition.y = _map.YPosition+1;
            }
            enemyCharacter.transform.position =  randomPosition;
            if(enemy)
                _enemySpawnList.Add(enemy.gameObject);

        }
        else
        {
            _genTime += Time.deltaTime;
        }
    }

    public CardData GetRandomCardSelectionData()
    {
        return _remainCardSelectionList.GetRandom();
    }
    public List<CardData> GetRandomCardSelectionData(int count)
    {
        return _remainCardSelectionList.GetRandom(count);
    }
    public List<CardData> GetRemainCardSelection()
    {
        return _remainCardSelectionList;
    }
    // 카드를 선택하여 능력치 추가
    public void SelectCardData(CardData data)
    {
        if (!_cardSelectionCount.ContainsKey(data.CardName))
        {
            _cardSelectionCount.Add(data.CardName, 0);
            _remainCardSelectionList.AddRange(data.CardListToAdd);
        }
        _cardSelectionCount[data.CardName]++;

        if(data.MaxUpgradeCount <= _cardSelectionCount[data.CardName])
        {
            _remainCardSelectionList.Remove(data);
        }

        // TODO
        // 능력적용

        if(data.CardSelectionType == Define.CardType.Weapon)
        {
            WeaponCardSelection weaponCardSelection = data as WeaponCardSelection;

            WeaponSwaper swaper = Player.GetComponent<WeaponSwaper>();

            swaper.ChangeNewWeapon(weaponCardSelection.WeaponSlotIndex, weaponCardSelection.WeaponName);
        }
        else
        {
            switch (data.CardName)
            {
                case Define.CardName.None:
                    break;
                case Define.CardName.딸최대체력증가:
                    Daughter.AddMaxHp(2);
                    break;
                case Define.CardName.발사간격감소:
                    Player.DecreasedFireRatePercent += 10;
                    break;
                case Define.CardName.재장전속도증가:
                    Player.IncreasedReloadSpeedPercent += 10;
                    break;
                case Define.CardName.반동제어력증가:
                    Player.IncreasedReboundControlPowerPercent += 10;
                    break;
                case Define.CardName.반동회복력증가:
                    Player.IncreasedReboundRecoverPercent += 10;
                    break;
                case Define.CardName.반동삭제:
                    Player.IsHaveRemoveReboundAMoment = true;
                    break;
                case Define.CardName.빠른재장전:
                    Player.IsHaveFastReload = true;
                    break;
                case Define.CardName.추가탄창:
                    Player.IsHaveExtraAmmo = true;
                    break;
                case Define.CardName.관통력증가:
                    Player.IncreasedPenerstratingPower += 1;
                    break;
                case Define.CardName.라스트샷:
                    Player.IncreasedReloadSpeedPercent -= 30;
                    Player.IsHaveLastShot = true;
                    break;
                case Define.CardName.반동제어력감소데미지증가:
                    Player.IncreasedReboundControlPowerPercent -= 30;
                    Player.IncreasedDamage += 1;
                    break;
                case Define.CardName.딸체력재생력증가:
                    Daughter.IncreasedRecoverHpPower += 0.2f;
                    break;
                case Define.CardName.눈어시스트:
                    break;
                case Define.CardName.눈데미지증가:
                    break;
                case Define.CardName.엑스트라웨폰:
                    break;
                case Define.CardName.웨폰마스터:
                    break;
                case Define.CardName.자동장전:
                    Player.IsHaveAutoReload = true;
                    break;
                case Define.CardName.아빠최대체력증가:
                    Father.AddMaxHp(2);
                    break;
                case Define.CardName.아빠일반공격력증가:
                    FatherAI.IncreasedNormalAttackDamage += 1;
                    break;
                case Define.CardName.아빠공격속도증가:
                    FatherAI.IncreasedNormalAttackSpeedPercentage += 10;
                    break;
                case Define.CardName.아빠일반공격주기감소:
                    FatherAI.DecreasedNormalAttackCoolTimePercentage += 10;
                    break;
                case Define.CardName.쇼크웨이브언락:
                    FatherAI.IsUnlockShockwave = true;
                    break;
                case Define.CardName.쇼크웨이브반경증가:
                    FatherAI.ShockwaveRange += 5;
                    break;
                case Define.CardName.쇼크웨이브공격주기감소:
                    FatherAI.DecreasedShockwaveCoolTimePercentage += 10;
                    break;
                case Define.CardName.쇼크웨이브히트휫수증가:
                    FatherAI.ShockwaveHitCount += 1;
                    break;
                case Define.CardName.아빠체력재생력증가:
                    Father.IncreasedRecoverHpPower += 0.4f;
                    break;
                case Define.CardName.꿰뚫기언락:
                    break;
                case Define.CardName.꿰뚫기공격주기감소:
                    break;
                case Define.CardName.꿰뚫기거리증가:
                    break;
                case Define.CardName.꿰뚫기확장:
                    break;
                case Define.CardName.꿰뚫기확장거리증가:
                    break;
                case Define.CardName.강아지체력증가:
                    Dog.AddMaxHp(5);
                    Vector3 scale = Dog.transform.localScale;
                    scale.x += 0.1f;
                    scale.y += 0.1f;
                    scale.z += 0.1f;
                    Dog.transform.localScale = scale;

                    break;
                case Define.CardName.END:
                    break;
            }
            if (data.CardName == Define.CardName.딸최대체력증가)
            {
                Daughter.SetMaxHp(Daughter.MaxHp + 2);
            }
            //if(data.CardName == Define.CardName.반동제어)
            //{
            //    Player.SetReboundControlPower(Player.ReboundControlPower + 10);
            //}
            //if(data.CardName == Define.CardName.총알관통력증가)
            //{
            //    Player.PenerstratingPower++;
            //}
            //if (data.CardName == Define.CardName.재장전시간감소)
            //{
            //    Player.ReduceReloadTime += 10;
            //}
            //if (data.CardName == Define.CardName.스피어능력해제)
            //{
            //    FatherAI.IsUnlockSpear= true;
            //}
            //if (data.CardName == Define.CardName.쇼크웨이브능력해제)
            //{
            //    FatherAI.IsUnlockShockwave = true;
            //}
            //if (data.CardName == Define.CardName.방벽크기증가)
            //{
            //    Vector3 scale = Dog.transform.localScale;
            //    scale.x += 0.1f;
            //    scale.y += 0.1f;
            //    Dog.transform.localScale = scale;
            //    Dog.SetMaxHp(Dog.MaxHp + 5);
            //}
        }
    }
    public int GetCardSelectionCount(Define.CardName cardSelection)
    {
        int count = 0;
        _cardSelectionCount.TryGetValue(cardSelection, out count);

        return count;
    }
}

[System.Serializable]
struct Wave
{
    public int time;
    public float genTime;
    public List<Define.EnemyName> enemyList;
    public float hpMultiply;
}

class Map
{
    int currentIndex = -10002;
    int moreBackBuildingIndex = -1000;
    int groundCount = 5;

    Dictionary<int, GameObject> grounds = new Dictionary<int, GameObject>();
   
    GameObject leftBuilding;
    GameObject centerBuilding;
    GameObject rightBuilding;

    GameObject moreBackLeftBuilding;
    GameObject moreBackCenterBuilding;
    GameObject moreBackRightBuilding;

    List<string> buildingPresetPathList = new List<string>();
    List<string> moreBackBuildingPresetPathList = new List<string>();

    float groundTerm;
    float yPosision = -8.91f;
    public float YPosition => yPosision;

    GameObject groundFolder;
    public Map(float groundTenm)
    {
        this.groundTerm = groundTenm;
        groundFolder = new GameObject("GroundFolder");
        groundFolder.layer = LayerMask.NameToLayer("Ground");
        groundFolder.AddComponent<Rigidbody2D>().isKinematic = true;
        groundFolder.AddComponent<CompositeCollider2D>();
    }

    public void Update(Player player)
    {
        if(player == null) return;

        Vector3 pos = player.transform.position;
        SetGround(GetIndex(pos.x));

        int mul = 4;

        int index = Mathf.RoundToInt(pos.x / (groundTerm* mul));
        if(index != moreBackBuildingIndex)
        {
            if(moreBackBuildingPresetPathList.Count > 0)
            {
                // 중간
                Random.InitState(Mathf.RoundToInt(index / moreBackBuildingPresetPathList.Count));
                int random = (int)(Random.value * 1000);

                if (moreBackCenterBuilding)
                    Managers.GetManager<ResourceManager>().Destroy(moreBackCenterBuilding);
                Vector3 position = new Vector3(index * groundTerm * mul, 0, 0);
                position.y = -2f;
                moreBackCenterBuilding = Managers.GetManager<ResourceManager>().Instantiate(moreBackBuildingPresetPathList[(random + index) % moreBackBuildingPresetPathList.Count]);
                moreBackCenterBuilding.transform.parent = groundFolder.transform;
                // 왼쪽
                Random.InitState(Mathf.RoundToInt((index - 1) / moreBackBuildingPresetPathList.Count));
                random = (int)(Random.value * 1000);

                if (moreBackLeftBuilding)
                    Managers.GetManager<ResourceManager>().Destroy(moreBackLeftBuilding);
                position = new Vector3((index-1) * groundTerm * mul, 0, 0);
                position.y = -2f;
                moreBackLeftBuilding = Managers.GetManager<ResourceManager>().Instantiate(moreBackBuildingPresetPathList[(random + index - 1) % moreBackBuildingPresetPathList.Count]);
                moreBackLeftBuilding.transform.parent = groundFolder.transform;
                //오른쪽
                Random.InitState(Mathf.RoundToInt((index + 1) / moreBackBuildingPresetPathList.Count));
                random = (int)(Random.value * 1000);

                if (moreBackRightBuilding)
                    Managers.GetManager<ResourceManager>().Destroy(moreBackRightBuilding);
                position = new Vector3((index + 1) * groundTerm * mul , 0, 0);
                position.y = -2f;
                moreBackRightBuilding = Managers.GetManager<ResourceManager>().Instantiate(moreBackBuildingPresetPathList[(random + index + 1) % moreBackBuildingPresetPathList.Count]);
                moreBackRightBuilding.transform.parent = groundFolder.transform;
            }

            moreBackBuildingIndex = index;
        }

        float distacne = Camera.main.transform.position.x - moreBackBuildingIndex * groundTerm * mul;

        moreBackCenterBuilding.transform.position = new Vector3(moreBackBuildingIndex * groundTerm * mul + distacne/2, 0, 0);
        moreBackRightBuilding.transform.position = new Vector3(moreBackCenterBuilding.transform.position.x + groundTerm * 2, 0, 0);
        moreBackLeftBuilding.transform.position = new Vector3(moreBackCenterBuilding.transform.position.x - groundTerm * 2, 0, 0);

    }

  
    public void SetGround(int index)
    {
        if (index == currentIndex) return;

        Random.InitState(Mathf.RoundToInt(index / groundCount));
        int groundRandom = (int)(Random.value * 1000);

        List<int> indexList = grounds.Keys.ToList();

        for (int i = 0; i < groundCount; i++)
        {
            int groundIndex = index + ((i % 2) == 0 ? -1 : 1) * (Mathf.FloorToInt((i-1) / 2)+1) * (i == 0 ? 0 : 1);
            // int randomGroundTile = (groundRandom + groundIndex) % groundCount;

            if (!grounds.ContainsKey(groundIndex))
            {
                grounds.Add(groundIndex, Managers.GetManager<ResourceManager>().Instantiate("Ground"));
                grounds[groundIndex]?.transform.SetParent(groundFolder.transform);
                grounds[groundIndex].transform.position = new Vector3(groundTerm * groundIndex, YPosition, 0);
            }

            indexList.Remove(groundIndex);
        }

        for(int i = indexList.Count-1; i >= 0; i--) 
        {
            Managers.GetManager<ResourceManager>().Destroy(grounds[indexList[i]]);
            grounds.Remove(indexList[i]);
        }


        currentIndex= index;


        if (buildingPresetPathList.Count > 0)
        {

            // 중간
            Random.InitState(Mathf.RoundToInt(index / buildingPresetPathList.Count));
            int random = (int)(Random.value * 1000);

            if (centerBuilding)
                Managers.GetManager<ResourceManager>().Destroy(centerBuilding);
            Vector3 position = grounds[currentIndex].transform.position;
            position.y = Random.Range(-1f, -4f);
            centerBuilding = Managers.GetManager<ResourceManager>().Instantiate(buildingPresetPathList[(random + index) % buildingPresetPathList.Count]);
            centerBuilding.transform.parent = groundFolder.transform;
            centerBuilding.transform.position = position;

            // 왼쪽
            Random.InitState(Mathf.RoundToInt((index-1) / buildingPresetPathList.Count));
            random = (int)(Random.value * 1000);

            if (leftBuilding)
                Managers.GetManager<ResourceManager>().Destroy(leftBuilding);
            position = grounds[currentIndex-1].transform.position;
            position.y = Random.Range(-1f, -4f);
            leftBuilding = Managers.GetManager<ResourceManager>().Instantiate(buildingPresetPathList[(random + index-1) % buildingPresetPathList.Count]);
            leftBuilding.transform.parent = groundFolder.transform;
            leftBuilding.transform.position = position;

            //오른쪽
            Random.InitState(Mathf.RoundToInt((index + 1) / buildingPresetPathList.Count));
            random = (int)(Random.value * 1000);

            if (rightBuilding)
                Managers.GetManager<ResourceManager>().Destroy(rightBuilding);
            position = grounds[currentIndex+1].transform.position;
            position.y = Random.Range(-1f, -4f);
            rightBuilding = Managers.GetManager<ResourceManager>().Instantiate(buildingPresetPathList[(random + index + 1) % buildingPresetPathList.Count]);
            rightBuilding.transform.parent = groundFolder.transform;
            rightBuilding.transform.position = position;
        }
    }

    public void SetCenterGround(GameObject ground)
    {
        grounds.Add(0,ground);

        for (int i = 1; i < groundCount; i++)
        {
            int groundIndex = ((i % 2) == 0 ? -1 : 1) * (Mathf.FloorToInt((i - 1) / 2) + 1);
            grounds.Add(groundIndex, Managers.GetManager<ResourceManager>().Instantiate("Ground"));
            grounds[groundIndex]?.transform.SetParent(groundFolder.transform);
            grounds[groundIndex].transform.position = new Vector3(groundTerm * groundIndex, YPosition, 0);
        }
    }

    public void AddBuildingPreset(string path)
    {
        buildingPresetPathList.Add(path);
    }
    public void AddMoreBackBuildingPreset(string path)
    {
        moreBackBuildingPresetPathList.Add(path);
    }

    public int GetIndex(float x)
    {
        int index = Mathf.RoundToInt(x / (groundTerm ));
        return index;
    }
}

