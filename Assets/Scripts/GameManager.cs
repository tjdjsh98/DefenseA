using MoreMountains.Tools;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.TextCore.Text;

public class GameManager : ManagerBase
{
    #region 캐릭터
    public Player Player { set; get; }

    public CreatureAI CreatureAI;

    Character _girl;
    public Character Girl { get { if (Player && _girl == null) _girl = Player.GetComponent<Character>(); return _girl; } }

    public WallAI WallAI { set; get; }
    Character _wall;
    public Character Wall { get { if (WallAI && _wall == null) _wall = WallAI?.GetComponent<Character>(); return _wall; } }

    Character _creature;
    public Character Creature { get { if (CreatureAI && _creature == null) _creature = CreatureAI?.GetComponent<Character>(); return _creature; } }
    #endregion

    [Header("Debug")]
    [SerializeField] bool _summonDummy;
    [SerializeField] int _dummyHp;

    [Header("게임 진행")]
    [SerializeField] bool _stop;
    [SerializeField] MapData _mapData;
    [SerializeField]Map _map;
    public float MapSize => _map.MapSize;
    float _farDistance;


    // 플레이 경험 관련
    [Header("경험치")]
    [SerializeField] bool _allMaxExpFive;
    [SerializeField] int _exp;
    int _maxExp = 3;
    public int Exp
    {
        set
        {
            _exp = value;
            if (_exp >= MaxExp)
            {
                _exp = _exp - MaxExp;
                Level++;
                _maxExp = Mathf.RoundToInt(_maxExp * 1.5f);
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
            return _maxExp;
        }
    }
    public int Level { set; get; }

    [Header("타임라인")]
    [SerializeField] bool _isSkip;
    [SerializeField] PlayableDirector _playableDirector;
    [SerializeField] PlayableAsset _enteracneTimeline;
    bool _isPlayTimeline;
    public bool IsPlayTimeline => _isPlayTimeline;



    // 적 소환 관련 변수
    [Header("적관련변수")]
    List<GameObject> _enemySpawnList = new List<GameObject>();
    [SerializeField] List<Wave> _timeWaveList;
    [SerializeField] List<Wave> _distanceWaveList;

    [SerializeField] int _currentWave = 0;
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
    [SerializeField] List<PriorCard> _priorCardList = new List<PriorCard>();

    // 땅과 관련변수
    [Header("상점 관련 변수")]
    [field: SerializeField] public List<ShopItem> ShopItemList;
    public override void Init()
    {
        _remainCardSelectionList = Managers.GetManager<DataManager>().GetDataList<CardData>((d) =>
        {
            if (d.IsStartCard)
            {
                return true;
            }
            else
            {
                PriorCard prior = new PriorCard();
                prior.cardName = d.CardName;
                foreach (var card in d.PriorCards)
                    prior.priorCardList.Add(card);
                _priorCardList.Add(prior);
                return false;
            }
        });
        _map = new Map(60f);
        foreach (var buildingName in _mapData.buildingNameList)
        {
            _map.AddBuildingPreset(buildingName);
        }
        foreach (var buildingName in _mapData.moreBackBuildingNameList)
        {
            _map.AddMoreBackBuildingPreset(buildingName);
        }

        if (_mapData)
        {
            _timeWaveList = _mapData.timeWave.ToList();
            _distanceWaveList = _mapData.distanceWave.ToList();
        }

        if (!_isSkip)
        {
            _playableDirector.playableAsset = _enteracneTimeline;
            _playableDirector.Play();
            Invoke("OffTimeline", (float)_playableDirector.duration - 0.1f);
            _isPlayTimeline = true;
        }
    }
    void OffTimeline()
    {
        _isPlayTimeline = false;
        _creature?.SetVelocityForcibly(Vector3.zero);
        _girl?.SetVelocityForcibly(Vector3.zero);
        _wall?.SetVelocityForcibly(Vector3.zero);
    }
    public override void ManagerUpdate()
    {
        _totalTime += Time.deltaTime;
        HandleGround();
        if (_summonDummy)
        {
            EnemyNameDefine enemyOrigin = Managers.GetManager<DataManager>().GetData<EnemyNameDefine>((int)Define.EnemyName.Slime);
            EnemyNameDefine enemy = Managers.GetManager<ResourceManager>().Instantiate(enemyOrigin);
            enemy.transform.position = Player.transform.position + Vector3.right * 10;
            Character enemyCharacter = enemy.GetComponent<Character>();
            enemyCharacter.SetHp(_dummyHp);
            _summonDummy = false;
        }

        if (_stop) return;

        TimeWave();
        DistanceWave();

        if (Player)
            _farDistance = _farDistance < Player.transform.position.x ? Player.transform.position.x : _farDistance;

        if (Player && Player.transform.position.x > MapSize)
        {
            _stop = true;

            for (int i = _enemySpawnList.Count - 1; i >= 0; i--)
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

    void TimeWave()
    {
        if (IsPlayTimeline) return;
        if (_currentWave < _timeWaveList.Count - 1)
        {
            if (_totalTime > _timeWaveList[_currentWave + 1].time)
                _currentWave++;
        }

        if (_genTime > _timeWaveList[_currentWave].genTime)
        {
            _genTime = 0;
            if (_timeWaveList[_currentWave].enemyList == null) return;

            EnemyNameDefine enemyOrigin = Managers.GetManager<DataManager>().GetData<EnemyNameDefine>((int)_timeWaveList[_currentWave].enemyList.GetRandom());
            EnemyNameDefine enemy = Managers.GetManager<ResourceManager>().Instantiate(enemyOrigin);
            if (enemy)
            {
                Character enemyCharacter = enemy.GetComponent<Character>();
                if (enemy.IsGroup)
                    enemy.GetComponent<EnemyGroup>().SetHp(_timeWaveList[_currentWave].hpMultiply);
                else
                    enemyCharacter.SetHp((int)(enemyCharacter.MaxHp * _timeWaveList[_currentWave].hpMultiply));

                Vector3 randomPosition = Vector3.zero;
                float distance = 50;
                if (enemy.EnemyName == Define.EnemyName.BatGroup)
                {
                    randomPosition.x = Player.transform.position.x + Random.Range(0, 2) == 0 ? -30 : 30;
                    randomPosition.y = Player.transform.position.y + 20;
                }
                else if (enemyCharacter != null && enemyCharacter.IsEnableFly)
                {
                    float angle = 0;
                    if (Random.Range(0, 2) == 0)
                        angle = Random.Range(30, 70);
                    else
                        angle = Random.Range(110, 150);

                    angle = angle * Mathf.Deg2Rad;
                    randomPosition.x = Player.transform.position.x + Mathf.Cos(angle) * distance;
                    randomPosition.y = Player.transform.position.y + Mathf.Sin(angle) * distance;

                }
                else
                {
                    randomPosition.x = Player.transform.position.x + distance;
                    randomPosition.y = _map.YPosition + 1;
                }
                enemy.transform.position = randomPosition;
                if (enemy)
                    _enemySpawnList.Add(enemy.gameObject);
            }
        }
        else
        {
            _genTime += Time.deltaTime;
        }
    }

    void DistanceWave()
    {
        foreach (var wave in _distanceWaveList)
        {
            if (wave.distace <= _farDistance)
            {
                foreach (var enemyName in wave.enemyList)
                {
                    EnemyNameDefine enemyOrigin = Managers.GetManager<DataManager>().GetData<EnemyNameDefine>((int)enemyName);
                    EnemyNameDefine enemy = Managers.GetManager<ResourceManager>().Instantiate(enemyOrigin);
                    if (!enemy.IsGroup)
                    {
                        Character character = enemy.GetComponent<Character>();
                        enemy.transform.position = new Vector3(_farDistance, _map.YPosition) + wave.genLocalPosition;
                        character.SetHp(Mathf.RoundToInt(character.Hp * wave.hpMultiply));
                    }
                    else
                    {
                        EnemyGroup group = enemy.GetComponent<EnemyGroup>();
                        group.transform.position = new Vector3(_farDistance, _map.YPosition) + wave.genLocalPosition;
                        group.SetHp(wave.hpMultiply);
                    }
                }
                _distanceWaveList.Remove(wave);
                return;
            }
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

            // 선행카드가 모두 만족되었다면 남은카드에 추가
            foreach (var prior in _priorCardList)
            {
                prior.priorCardList.Remove(data.CardName);
                if (prior.priorCardList.Count <= 0)
                {
                    _remainCardSelectionList.Add(Managers.GetManager<DataManager>().GetData<CardData>((int)prior.cardName));
                }
            }
        }
        _cardSelectionCount[data.CardName]++;

        // 업그레이드가 모두 완료 시 남은 카드에서 삭제
        if (data.MaxUpgradeCount <= _cardSelectionCount[data.CardName])
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
            GirlCardData girlCardData = data as GirlCardData;
            CreatureCardData creatureCardData = data as CreatureCardData;
            WallCardData wallCardData = data as WallCardData;
            if (girlCardData != null)
            {
                Girl.AddMaxHp(girlCardData.IncreaseHp);
                Girl.IncreasedRecoverHpPower += girlCardData.IncreaseRecoverHpPower;
                Girl.IncreasedDamageReducePercentage += girlCardData.IncreaseDamageReducePercentage;
                Girl.AttackPower += girlCardData.IncreaseAttackPoint;

                if(girlCardData.UnlockAbility != Define.GirlAbility.None && !Player.AbilityUnlocks.ContainsKey(girlCardData.UnlockAbility))
                    Player.AbilityUnlocks.Add(girlCardData.UnlockAbility,true);
                Player.DecreasedFireDelayPercent += girlCardData.DecreaseFireDelayPercentage;
                Player.IncreasedReloadSpeedPercent += girlCardData.IncreaseReloadSpeedPercentage;
                Player.IncreasedReboundControlPowerPercent += girlCardData.IncreaseReboundControlPowerPercentage;
                Player.IncreasedReboundRecoverPercent += girlCardData.IncreaseReboundControlPowerPercentage;
                Player.IncreasedPenerstratingPower += girlCardData.IncreasePenerstratingPower;
            }
            if (creatureCardData != null)
            {
                Creature.AddMaxHp(creatureCardData.IncreaseHp);
                Creature.IncreasedRecoverHpPower += creatureCardData.IncreaseRecoverHpPower;
                Creature.IncreasedDamageReducePercentage += creatureCardData.IncreaseDamageReducePercentage;
                Creature.AttackPower += creatureCardData.IncreaseAttackPoint;

                if (creatureCardData.UnlockAbility != Define.CreatureAbility.None && !CreatureAI.AbilityUnlocks.ContainsKey(creatureCardData.UnlockAbility))
                    CreatureAI.AbilityUnlocks.Add(creatureCardData.UnlockAbility, true);
            }
            if (wallCardData != null)
            {
                Wall.AddMaxHp(wallCardData.IncreaseHp);
                Wall.IncreasedRecoverHpPower += wallCardData.IncreaseRecoverHpPower;
                Wall.IncreasedDamageReducePercentage += wallCardData.IncreaseDamageReducePercentage;
                Wall.AttackPower += wallCardData.IncreaseAttackPoint;

                if(wallCardData.UnlockAbility != Define.WallAbility.None && !WallAI.AbilityUnlocks.ContainsKey(wallCardData.UnlockAbility))
                    WallAI.AbilityUnlocks.Add(wallCardData.UnlockAbility,true);
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
public struct Wave
{
    public int time;
    public float genTime;

    public float distace;
    public Vector3 genLocalPosition;

    public List<Define.EnemyName> enemyList;
    public float hpMultiply;
}

[System.Serializable]
public class PriorCard
{
    public Define.CardName cardName;
    public List<Define.CardName> priorCardList = new List<Define.CardName>();
}
