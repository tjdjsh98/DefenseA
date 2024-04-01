using DuloGames.UI.Tweens;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameManager : ManagerBase
{
    #region 캐릭터
    public Player Player { set; get; }

    public CreatureAI CreatureAI;

    Character _girl;
    public Character Girl { get { if (Player && _girl == null) _girl = Player.GetComponent<Character>(); return _girl; } }

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

    public int Money { set; get; }

    int _panicLevel = 0;
    public int PanicLevel =>_panicLevel;
    public float MaxMental { get; } = 100;
    public float Mental { set; get; } = 100;

   
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
    List<Wave> _timeWaveList = new List<Wave>();
    List<Wave> _distanceWaveList = new List<Wave>();
    List<Wave> _mentalWaveList = new List<Wave>();
    float _totalTime;
    float _stageTime;

    public int HuntingCount { set; get; }

    [Header("카드 선택지")]
    List<CardData> _remainCardSelectionList;
    List<CardData> _earnCardSelectionList;
    Dictionary<CardName, int> _cardSelectionCount = new Dictionary<CardName, int>();
    [SerializeField] List<PriorCard> _priorCardList = new List<PriorCard>();

    [SerializeField] List<GameObject> _subObjects;
    [SerializeField] List<GameObject> _mainObjects;

    [HideInInspector]public List<ShopItemData> ShopItemDataList => _mapData == null?null : _mapData.shopItemDataList;

    CameraController _cameraController;
    public CameraController CameraController
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
                foreach (var cardData in d.PriorCards)
                    prior.priorCardDataList.Add(cardData);
                _priorCardList.Add(prior);
                return false;
            }
        });

        _cameraController = Camera.main.GetComponent<CameraController>();
        LoadMapData();
    }

    void LoadMapData()
    {
        _timeWaveList.Clear();
        _distanceWaveList.Clear();
        _mentalWaveList.Clear();

        // 적 소환 데이터
        if (_mapData)
        {
            foreach (var waveData in _mapData.timeWave)
            {
                TimeWaveData timeWaveData = waveData as TimeWaveData;
                if (timeWaveData != null)
                {
                    _timeWaveList.Add(new Wave() { waveData = timeWaveData, elapsedTime = timeWaveData.genTime });
                }
            }
            foreach (var waveData in _mapData.distanceWave)
            {
                DistanceWaveData distanceWaveData = waveData as DistanceWaveData;
                if (distanceWaveData != null)
                {
                    _distanceWaveList.Add(new Wave() { waveData = distanceWaveData, elapsedTime = distanceWaveData.genTime });
                }
            }
            foreach (var waveData in _mapData.mentalWave)
            {
                MentalWaveData mentalWaveData = waveData as MentalWaveData;
                if (mentalWaveData != null)
                {
                    _mentalWaveList.Add(new Wave() { waveData = mentalWaveData, elapsedTime = mentalWaveData.genTime });
                }
            }

            // 이벤트 배치
            float distance = -_mapData.randomEventInterval;
            if (_mapData.randomEvent.Count > 0)
            {
                while (distance < _mapData.mapSize)
                {
                    distance += _mapData.randomEventInterval + Random.Range(-20, 20);
                    Vector3? position = GetGroundTop(new Vector3(distance, 0));
                    if (position.HasValue)
                    {
                        GameObject go = Managers.GetManager<ResourceManager>().Instantiate(_mapData.randomEvent.GetRandom());
                        go.transform.position = position.Value;
                    }
                }
            }
        }

        if (!_isSkip)
        {
            _playableDirector.playableAsset = _enteracneTimeline;
            _playableDirector.Play();
            Invoke("OffTimeline", (float)_playableDirector.duration - 0.1f);
            _isPlayTimeline = true;
        }

        LoadObjects();
        _stageTime = 0;
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
        _creature.transform.position = GetGroundTop(new Vector3(-46, 0, 0)).Value;
    }

    void LoadObjects()
    {
        float distance = 5;
        while (distance < _mapData.mapSize)
        {
            Vector3? position = GetGroundTop(Vector3.one * distance);
            if (position.HasValue) {
                GameObject go = Managers.GetManager<ResourceManager>().Instantiate(_mainObjects.GetRandom());
                go.transform.position = position.Value;
            }
            distance += Random.Range(25, 40);
        }
        distance = 3.5f;
        while (distance < _mapData.mapSize)
        {
            Vector3? position = GetGroundTop(Vector3.one * distance);
            if (position.HasValue)
            {
                GameObject go = Managers.GetManager<ResourceManager>().Instantiate(_subObjects.GetRandom());
                go.transform.position = position.Value;
            }
            distance += Random.Range(15, 25);
        }
    }

    void OffTimeline()
    {
        _isPlayTimeline = false;
        _creature?.SetVelocityForcibly(Vector3.zero);
        _girl?.SetVelocityForcibly(Vector3.zero);
    }
    public override void ManagerUpdate()
    {
        Mental -= Time.deltaTime;
        _totalTime += Time.deltaTime;
        _stageTime += Time.deltaTime;
        if (Mental <= 0)
        {
            _panicLevel++;
            Mental += 100;
        }
        if( Mental > 100)
        {
            _panicLevel--;
            Mental -= 100;
        }
        if (_summonDummy)
        {
            EnemyNameDefine enemyOrigin = Managers.GetManager<DataManager>().GetData<EnemyNameDefine>((int)Define.EnemyName.Slime);
            EnemyNameDefine enemy = Managers.GetManager<ResourceManager>().Instantiate(enemyOrigin);
            enemy.transform.position = Player.transform.position + Vector3.right * 10;
            Character enemyCharacter = enemy.GetComponent<Character>();
            enemyCharacter.SetHp(_dummyHp);
            _summonDummy = false;
        }
        if (Input.GetKeyDown(KeyCode.I))
        {
            UIStatus uIStatus = Managers.GetManager<UIManager>().GetUI<UIStatus>();
            if (uIStatus.gameObject.activeSelf)
                uIStatus.Close();
            else
                uIStatus.Open();
        }


        if (_stop) return;

        TimeWave();
        DistanceWave();
        MentalWave();

        if (Player)
            _farDistance = _farDistance < Player.transform.position.x ? Player.transform.position.x : _farDistance;

       
    }
    void TimeWave()
    {
        if (IsPlayTimeline) return;
        foreach (var timeWave in _timeWaveList)
        {
            TimeWaveData timeWaveData = timeWave.waveData as TimeWaveData;
            if (_stageTime > timeWaveData.endTime || _stageTime < timeWaveData.startTime)
                continue;

            if (timeWave.elapsedTime > timeWave.waveData.genTime)
            {
                timeWave.elapsedTime = 0;

                if (timeWaveData.enemyName == Define.EnemyName.None) return;

                EnemyNameDefine enemyOrigin = Managers.GetManager<DataManager>().GetData<EnemyNameDefine>((int)timeWaveData.enemyName);
                EnemyNameDefine enemy = Managers.GetManager<ResourceManager>().Instantiate(enemyOrigin);
                if (enemy)
                {
                    Character enemyCharacter = enemy.GetComponent<Character>();

                    // 체력 설정
                    if (enemy.IsGroup)
                        enemy.GetComponent<EnemyGroup>().SetHp(timeWaveData.hpMultiply);
                    else
                        enemyCharacter.SetHp((int)(enemyCharacter.MaxHp * timeWaveData.hpMultiply));


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
            DistanceWaveData distanceWaveData = wave.waveData as DistanceWaveData;
            if (distanceWaveData.distance <= _farDistance)
            {
                EnemyNameDefine enemyOrigin = Managers.GetManager<DataManager>().GetData<EnemyNameDefine>((int)distanceWaveData.enemyName);
                EnemyNameDefine enemy = Managers.GetManager<ResourceManager>().Instantiate(enemyOrigin);
                if (!enemy.IsGroup)
                {
                    Vector3? topPosition = GetGroundTop(new Vector3(_farDistance, 0) + distanceWaveData.genLocalPosition).Value;
                    if (topPosition.HasValue)
                    {
                        Character character = enemy.GetComponent<Character>();
                        enemy.transform.position = topPosition.Value;
                        character.SetHp(Mathf.RoundToInt(character.Hp * distanceWaveData.hpMultiply));
                    }
                }
                else
                {
                    Vector3? topPosition = GetGroundTop(new Vector3(_farDistance, 0) + distanceWaveData.genLocalPosition).Value;
                    if (topPosition.HasValue)
                    {
                        EnemyGroup group = enemy.GetComponent<EnemyGroup>();
                        group.transform.position = topPosition.Value;
                        group.SetHp(distanceWaveData.hpMultiply);
                    }
                }
                _distanceWaveList.Remove(wave);
                return;
            }
        }
    }

    void MentalWave()
    {
        foreach (var wave in _mentalWaveList)
        {
            MentalWaveData mentalWaveData = wave.waveData as MentalWaveData;
            if (mentalWaveData.genMentalLevelOrMore > PanicLevel)
                continue;

            if (wave.elapsedTime < wave.waveData.genTime)
            {
                wave.elapsedTime += Time.deltaTime;
            }
            else
            {
                wave.elapsedTime = 0;

                EnemyNameDefine enemyOrigin = Managers.GetManager<DataManager>().GetData<EnemyNameDefine>((int)wave.waveData.enemyName);
                EnemyNameDefine enemy = Managers.GetManager<ResourceManager>().Instantiate(enemyOrigin);
                if (!enemy.IsGroup)
                {
                    Vector3? topPosition = GetGroundTop(new Vector3(_farDistance, 0) + mentalWaveData.genLocalPosition).Value;
                    if (topPosition.HasValue)
                    {
                        Character character = enemy.GetComponent<Character>();
                        enemy.transform.position = topPosition.Value;
                        character.SetHp(Mathf.RoundToInt(character.Hp * mentalWaveData.hpMultiply));
                    }
                }
                else
                {
                    Vector3? topPosition = GetGroundTop(new Vector3(_farDistance, 0) + mentalWaveData.genLocalPosition).Value;
                    if (topPosition.HasValue)
                    {
                        EnemyGroup group = enemy.GetComponent<EnemyGroup>();
                        group.transform.position = topPosition.Value;
                        group.SetHp(mentalWaveData.hpMultiply);
                    }
                }

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
        }
        _cardSelectionCount[data.CardName]++;

        // 선행카드가 모두 만족되었다면 남은카드에 추가
        foreach (var prior in _priorCardList)
        {
            bool success = true;
            foreach (var priorCardData in prior.priorCardDataList)
            {
                if (!_cardSelectionCount.ContainsKey(priorCardData.priorCardName))
                {
                    success = false;
                    break;
                }
                if (_cardSelectionCount[priorCardData.priorCardName] < priorCardData.priorUpgradeCount)
                {
                    success = false;
                    break;
                }
            }
            if (success)
            {
                _remainCardSelectionList.Add(Managers.GetManager<DataManager>().GetData<CardData>((int)prior.cardName));
            }
        }

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
            Managers.GetManager<AbilityManager>().ApplyCardAbility(data);
        }
    }
    public int GetCardSelectionCount(CardName cardSelection)
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
        _creature.transform.position = GetGroundTop(new Vector3(-46, 0, 0)).Value;
        SceneManager.LoadScene(mapData.name);

        _mapData = mapData;
        LoadMapData();
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
    public CardName cardName;
    public List<PriorCardData> priorCardDataList = new List<PriorCardData>();
}
