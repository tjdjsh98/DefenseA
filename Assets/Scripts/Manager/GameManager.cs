using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class GameManager : ManagerBase
{
    #region ĳ����
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

    [Header("���� ����")]
    [SerializeField] bool _stop;
    [SerializeField] MapData _mapData;
    public string LevelName=>_mapData != null ? _mapData.name : "";
    public float MapSize => _mapData.mapSize;
    float _farDistance;

    public bool IsLoadEnd { set; get; }
    public Action<MapData> LoadNewSceneHandler { set; get; }

    int _panicLevel = 0;
    public int PanicLevel =>_panicLevel;
    public float MaxMental { get; } = 100;
    public float Mental { set; get; } = 100;

    // �÷��� ���� ����
    [Header("����ġ")]
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

    [Header("Ÿ�Ӷ���")]
    [SerializeField] bool _isSkip;
    [SerializeField] PlayableDirector _playableDirector;
    [SerializeField] PlayableAsset _enteracneTimeline;
    bool _isPlayTimeline;
    public bool IsPlayTimeline => _isPlayTimeline;



    // �� ��ȯ ���� ����
    [Header("�����ú���")]
    List<GameObject> _enemySpawnList = new List<GameObject>();
    List<Wave> _timeWaveList;
    List<Wave> _distanceWaveList;
    float _totalTime;
    float _stageTime;

    [Header("ī�� ������")]
    List<CardData> _remainCardSelectionList;
    List<CardData> _earnCardSelectionList;
    Dictionary<CardName, int> _cardSelectionCount = new Dictionary<CardName, int>();
    [SerializeField] List<PriorCard> _priorCardList = new List<PriorCard>();

    [HideInInspector]public List<ShopItemData> ShopItemDataList => _mapData == null?null : _mapData.shopItemDataList;

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
            _timeWaveList = _mapData.timeWave.ToList();
            _distanceWaveList = _mapData.distanceWave.ToList();
            float distance = 0;
            if(_mapData.randomEvent.Count > 0)
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
        Mental -= Time.deltaTime;
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

        // ���� ���� ���� �����ϸ� ���� ����������
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

                    // ü�� ����
                    if (enemy.IsGroup)
                        enemy.GetComponent<EnemyGroup>().SetHp(timeWave.hpMultiply);
                    else
                        enemyCharacter.SetHp((int)(enemyCharacter.MaxHp * timeWave.hpMultiply));


                    // ��ġ ����
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

                    // �� �� �����߰�
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
    // ī�带 �����Ͽ� �ɷ�ġ �߰�
    public void SelectCardData(CardData data)
    {

        if (!_cardSelectionCount.ContainsKey(data.CardName))
        {
            _cardSelectionCount.Add(data.CardName, 0);

            // ����ī�尡 ��� �����Ǿ��ٸ� ����ī�忡 �߰�
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

        // ���׷��̵尡 ��� �Ϸ� �� ���� ī�忡�� ����
        if (data.MaxUpgradeCount <= _cardSelectionCount[data.CardName])
        {
            _remainCardSelectionList.Remove(data);
        }

        // TODO
        // �ɷ�����

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
        _wall.transform.position = GetGroundTop(new Vector3(-43, 0, 0)).Value;
        _creature.transform.position = GetGroundTop(new Vector3(-46, 0, 0)).Value;
        SceneManager.LoadScene(mapData.name);

        _mapData = mapData;
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
    public CardName cardName;
    public List<CardName> priorCardList = new List<CardName>();
}
