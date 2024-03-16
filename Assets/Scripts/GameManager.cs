using MoreMountains.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
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
    public string LevelName=>_mapData != null ? _mapData.name : "";
    public float MapSize => _mapData.mapSize;
    float _farDistance;

    public bool IsLoadEnd { set; get; }
    public Action<MapData> LoadNewSceneHandler { set; get; }

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
    List<Wave> _timeWaveList;
    List<Wave> _distanceWaveList;

    [SerializeField] int _currentWave = 0;
    bool _isStartWave;
    public bool IsStartWave => _isStartWave;
    float _totalTime;
    float _stageTime;

    [Header("카드 선택지")]
    List<CardData> _remainCardSelectionList;
    List<CardData> _earnCardSelectionList;
    Dictionary<Define.CardName, int> _cardSelectionCount = new Dictionary<Define.CardName, int>();
    [SerializeField] List<PriorCard> _priorCardList = new List<PriorCard>();

    // 땅과 관련변수
    [Header("상점 관련 변수")]
    [field: SerializeField] public List<ShopItem> ShopItemList;


    CameraController _cameraController;
    CameraController CameraController
    {
        get
        {
            if(_cameraController == null)
                _cameraController = Camera.main.GetComponent<CameraController>();
            return _cameraController;
        }
    }


    public override void Init()
    {
        LoadMainCharacters();

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
        
        if (_mapData)
        {
            _timeWaveList = _mapData.timeWave;
            _distanceWaveList = _mapData.distanceWave;
        }

        if (!_isSkip)
        {
            _playableDirector.playableAsset = _enteracneTimeline;
            _playableDirector.Play();
            Invoke("OffTimeline", (float)_playableDirector.duration - 0.1f);
            _isPlayTimeline = true;
        }

        _cameraController = Camera.main.GetComponent<CameraController>();
        IsLoadEnd = true;
    }

    void LoadMainCharacters()
    {
        if (GameObject.Find("Girl") != null)
        {
            _girl = GameObject.Find("Girl").gameObject.GetComponent<Character>();
        }
        else
        {
            _girl = Managers.GetManager<ResourceManager>().Instantiate("Prefabs/MainCharacter/Girl").GetComponent<Character>();
        }
        DontDestroyOnLoad(_girl);
        Player = _girl.GetComponent<Player>();
        if (GameObject.Find("Wall") != null)
        {
            _wall = GameObject.Find("Wall").gameObject.GetComponent<Character>();
        }
        else
        {
            _wall = Managers.GetManager<ResourceManager>().Instantiate("Prefabs/MainCharacter/Wall").GetComponent<Character>();
        }
        DontDestroyOnLoad(_wall);
        WallAI = _wall.GetComponent<WallAI>();
        if (GameObject.Find("Creature") != null)
        {
            _creature = GameObject.Find("Creature").gameObject.GetComponent<Character>();
        }
        else
        {
            _creature = Managers.GetManager<ResourceManager>().Instantiate("Prefabs/MainCharacter/Creature").GetComponent<Character>();
        }
        DontDestroyOnLoad(_creature);
        CreatureAI = _creature.GetComponent<CreatureAI>();


        _girl.transform.position = GetGroundTop(new Vector3(-40, 0, 0)).Value;
        _wall.transform.position = GetGroundTop(new Vector3(-43, 0, 0)).Value;
        _creature.transform.position = GetGroundTop(new Vector3(-46, 0, 0)).Value;
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
        _stageTime += Time.deltaTime;
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

        // 벽이 맵의 끝에 도착하면 다음 스테이지로
        if (Wall && Wall.transform.position.x > MapSize)
        {
            if (_mapData.nextMapData != null)
                LoadScene(_mapData.nextMapData);
        }
    }

    void TimeWave()
    {
        if (IsPlayTimeline) return;
        foreach (var timeWave in _timeWaveList)
        {
            if (_stageTime > timeWave.endTime || _stageTime < timeWave.startTime)
                continue;

            if (timeWave.elapsedTime > timeWave.genTime)
            {
                timeWave.elapsedTime = 0;

                if (timeWave.enemyList == null) return;

                EnemyNameDefine enemyOrigin = Managers.GetManager<DataManager>().GetData<EnemyNameDefine>((int)timeWave.enemyList.GetRandom());
                EnemyNameDefine enemy = Managers.GetManager<ResourceManager>().Instantiate(enemyOrigin);
                if (enemy)
                {
                    Character enemyCharacter = enemy.GetComponent<Character>();

                    // 체력 설정
                    if (enemy.IsGroup)
                        enemy.GetComponent<EnemyGroup>().SetHp(timeWave.hpMultiply);
                    else
                        enemyCharacter.SetHp((int)(enemyCharacter.MaxHp * timeWave.hpMultiply));


                    // 위치 설정
                    Vector3 position = GetRightOutScreenPosition();
                    position.x += 10;
                    Vector3? topPosition = GetGroundTop(position);
                    if (topPosition.HasValue)
                        position.y = GetGroundTop(position).Value.y;

                    if (enemyCharacter.IsEnableFly)
                    {
                        FlyingEnemy flyingEnemy = enemyCharacter.GetComponent<FlyingEnemy>();
                        if (flyingEnemy)
                            position.y += flyingEnemy.FlyingHeight;
                    }

                    enemyCharacter.transform.position = position;

                    // 맵 적 스폰추가
                    if (enemy)
                        _enemySpawnList.Add(enemy.gameObject);
                }
            }
            else
            {
                timeWave.elapsedTime += Time.deltaTime;
            }
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
                        Vector3? topPosition = GetGroundTop(new Vector3(_farDistance, 0) + wave.genLocalPosition).Value;
                        if (topPosition.HasValue)
                        {
                            Character character = enemy.GetComponent<Character>();
                            enemy.transform.position = topPosition.Value;
                            character.SetHp(Mathf.RoundToInt(character.Hp * wave.hpMultiply));
                        }
                    }
                    else
                    {
                        Vector3? topPosition = GetGroundTop(new Vector3(_farDistance, 0) + wave.genLocalPosition).Value;
                        if (topPosition.HasValue)
                        {
                            EnemyGroup group = enemy.GetComponent<EnemyGroup>();
                            group.transform.position = topPosition.Value;
                            group.SetHp(wave.hpMultiply);
                        }
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

                if (girlCardData.UnlockAbility != Define.GirlAbility.None && !Player.AbilityUnlocks.ContainsKey(girlCardData.UnlockAbility))
                    Player.AbilityUnlocks.Add(girlCardData.UnlockAbility, true);
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
                Wall.transform.localScale += new Vector3(wallCardData.SizeUpPercentage / 100, wallCardData.SizeUpPercentage / 100, 0);

                if (wallCardData.UnlockAbility != Define.WallAbility.None && !WallAI.AbilityUnlocks.ContainsKey(wallCardData.UnlockAbility))
                    WallAI.AbilityUnlocks.Add(wallCardData.UnlockAbility, true);
            }
        }
    }
    public int GetCardSelectionCount(Define.CardName cardSelection)
    {
        int count = 0;
        _cardSelectionCount.TryGetValue(cardSelection, out count);

        return count;
    }

    public Vector3? GetGroundTop(Vector3 position)
    {
        position.y = 50;
        RaycastHit2D hit = Physics2D.Raycast(position, Vector2.down, 500, LayerMask.GetMask("Ground"));

        if (hit.collider == null) return null;

        return hit.point;
    }

    int count = 0;
    public void LoadScene(MapData mapData)
    {
        if (!IsLoadEnd) return;

        count++;
        if (count > 20)
        {
            Debug.Log("count");
            return;
        }
        StartCoroutine(CorLoadScene(mapData));
    }

    IEnumerator CorLoadScene(MapData mapData)
    {
        IsLoadEnd = false;

        Managers.GetManager<UIManager>().GetUI<UIInGame>().LoadSceneFadeOut();
        yield return new WaitForSeconds(2);
        _girl.transform.position = GetGroundTop(new Vector3(-40, 0, 0)).Value;
        _wall.transform.position = GetGroundTop(new Vector3(-43, 0, 0)).Value;
        _creature.transform.position = GetGroundTop(new Vector3(-46, 0, 0)).Value;
        SceneManager.LoadScene(mapData.name);

        _mapData = mapData;
        if (_mapData)
        {
            _timeWaveList = _mapData.timeWave;
            _distanceWaveList = _mapData.distanceWave;
        }

        if (!_isSkip)
        {
            _playableDirector.playableAsset = _enteracneTimeline;
            _playableDirector.Play();
            Invoke("OffTimeline", (float)_playableDirector.duration - 0.1f);
            _isPlayTimeline = true;
        }
        _stageTime = 0;

        IsLoadEnd = true;

        LoadNewSceneHandler?.Invoke(mapData);
    }
    public Vector3 GetRightOutScreenPosition()
    {
        Vector3 position = CameraController.transform.position;
        position.z = 0;
        position.x += _cameraController.GetCameraWidth() / 2;
        return position;
    }
}


[System.Serializable]
public class PriorCard
{
    public Define.CardName cardName;
    public List<Define.CardName> priorCardList = new List<Define.CardName>();
}
