using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Runtime.CompilerServices;
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
            EnemyNameDefine enemyOrigin = Managers.GetManager<DataManager>().GetData<EnemyNameDefine>((int)Define.EnemyName.Spider);
            EnemyNameDefine enemy = Managers.GetManager<ResourceManager>().Instantiate(enemyOrigin);
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
        if (IsPlayTimeline) return;
        if (_currentWave < _waveList.Count-1)
        {
            if (_totalTime > _waveList[_currentWave + 1].time)
                _currentWave++;
        }

        if (_genTime > _waveList[_currentWave].genTime)
        {
            _genTime = 0;
            if (_waveList[_currentWave].enemyList == null) return;

            EnemyNameDefine enemyOrigin = Managers.GetManager<DataManager>().GetData<EnemyNameDefine>((int)_waveList[_currentWave].enemyList.GetRandom());
            EnemyNameDefine enemy = Managers.GetManager<ResourceManager>().Instantiate(enemyOrigin);
            Character enemyCharacter = enemy.GetComponent<Character>();
            if (enemy.IsGroup)
                enemy.GetComponent<EnemyGroup>().SetHp(_waveList[_currentWave].hpMultiply);
            else
                enemyCharacter.SetHp((int)(enemyCharacter.MaxHp * _waveList[_currentWave].hpMultiply));

            Vector3 randomPosition = Vector3.zero;
            float distance = 50;
            if (enemy.EnemyName == Define.EnemyName.BatGroup)
            {
                randomPosition.x = Player.transform.position.x + Random.Range(0,2)==0?-30:30;
                randomPosition.y = Player.transform.position.y + 20;
            }
            else if (enemyCharacter!= null && enemyCharacter.IsEnableFly)
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
            enemy.transform.position =  randomPosition;
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

        if (data.CardSelectionType == Define.CardType.Weapon)
        {
            WeaponCardSelection weaponCardSelection = data as WeaponCardSelection;

            WeaponSwaper swaper = Player.GetComponent<WeaponSwaper>();

            swaper.ChangeNewWeapon(weaponCardSelection.WeaponSlotIndex, weaponCardSelection.WeaponName);
        }
        else
        {
            DaughterCardData daughterCardData = data as DaughterCardData;
            FatherCardData fatherCardData = data as FatherCardData;
            DogCardData dogCardData = data as DogCardData;
            if(daughterCardData != null)
            {
                Daughter.AddMaxHp(daughterCardData.IncreaseHp);
                Daughter.IncreasedRecoverHpPower += daughterCardData.IncreaseRecoverHpPower;
                Daughter.IncreasedDamageReducePercentage += daughterCardData.IncreaseDamageReducePercentage;
                Daughter.AttackPoint += daughterCardData.IncreaseAttackPoint;

                Player.IsUnlockLastShot|= daughterCardData.UnlockLastShot;
                Player.IsUnlockFastReload |= daughterCardData.UnlockFastReload;
                Player.IsUnlockAutoReload |= daughterCardData.UnlockAutoReload;
                Player.IsUnlockExtraAmmo |= daughterCardData.UnlockExtraAmmo;
                Player.DecreasedFireDelayPercent += daughterCardData.DecreaseFireDelayPercentage;
                Player.IncreasedReloadSpeedPercent += daughterCardData.IncreaseReloadSpeedPercentage;
                Player.IncreasedReboundControlPowerPercent += daughterCardData.IncreaseReboundControlPowerPercentage;
                Player.IncreasedReboundRecoverPercent += daughterCardData.IncreaseReboundControlPowerPercentage;
                Player.IncreasedPenerstratingPower += daughterCardData.IncreasePenerstratingPower;
            }
            if (fatherCardData != null)
            {
                Father.AddMaxHp(fatherCardData.IncreaseHp);
                Father.IncreasedRecoverHpPower += fatherCardData.IncreaseRecoverHpPower;
                Father.IncreasedDamageReducePercentage += fatherCardData.IncreaseDamageReducePercentage;
                Father.AttackPoint += fatherCardData.IncreaseAttackPoint;

                FatherAI.IncreasedNormalAttackSpeedPercentage += fatherCardData.IncreaseNormalAttackSpeedPercentage;
                FatherAI.IsUnlockShockwave |= fatherCardData.UnlockShockwave;
                FatherAI.IncreasedShockwaveDamagePercentage += fatherCardData.IncreaseShockwaveDamagePercentage;
                FatherAI.IncreasedShockwaveRangePercentage += fatherCardData.IncreaseShockwaveRangePercentage;
                FatherAI.DecreasedShockwaveCoolTimePercentage += fatherCardData.DecreaseShockwaveCoolTimePercentage;
                FatherAI.ShockwaveCount += fatherCardData.IncreaseShockwaveCount;
                FatherAI.IsUnlockStempGround |= fatherCardData.UnlockStempGround;
                FatherAI.IncreasedStempGroundDamagePercentage += fatherCardData.IncreaseStempGroundDamagePercentage;
                FatherAI.IncreasedStempGroundRangePercentage += fatherCardData.IncreaseStempGroundRangePercentage;
            }
            if (dogCardData != null)
            {
                Dog.AddMaxHp(dogCardData.IncreaseHp);
                Dog.IncreasedRecoverHpPower += dogCardData.IncreaseRecoverHpPower;
                Dog.IncreasedDamageReducePercentage += dogCardData.IncreaseDamageReducePercentage;
                Dog.AttackPoint += dogCardData.IncreaseAttackPoint;

                DogAI.ReflectionDamage += dogCardData.IncreaseReflectionDamage;
                DogAI.DecreasedReviveTimePercetage += dogCardData.DecreaseReviveTimePercentage;
                DogAI.IsUnlockExplosionWhenDead |= dogCardData.UnlockExplosionWhenDead;
                DogAI.ExplosionDamage += dogCardData.IncreaseExplosionDamage;
                DogAI.ExplosionRange += dogCardData.IncreaseExplosionRange;
                DogAI.IsReviveWhereDaughterPosition |= dogCardData.UnlockReviveWhereDaughterPosition;
            }
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

