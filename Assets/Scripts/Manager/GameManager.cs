using DuloGames.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.TextCore.Text;
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
    [SerializeField] bool _destroyDummy;
    GameObject _dummy;
    [SerializeField] int _dummyHp;
    [SerializeField] ItemName _itemName;
    [SerializeField] bool _addItem;
    [SerializeField] bool _removeItem;
    [SerializeField] float _timeScale = 1;
    
    [Header("게임 진행")]
    [SerializeField] bool _stop;
    [SerializeField] MapData _mapData;
    public string LevelName=>_mapData != null ? _mapData.name : "";
    public float MapSize => IsStartBeyondDead?1000000: _mapData.mapSize;
    float _farDistance;

    public bool IsLoadEnd { set; get; }
    public Action<MapData> LoadNewSceneHandler { set; get; }

    public int Money { set; get; } = 0;

    #region 멘탈, 패닉
    int _panicLevel = 0;
    public int PanicLevel =>_panicLevel;
    public float MaxMental { get; } = 100;
    public float Mental { set; get; } = 100;
    public float MentalAccelerationPercentage = 0;

    public Character Boss { get; set; }

    #endregion

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
    public float StatusMultifly => _mapData != null ?(_mapData.initMutifly + (_stageTime / _mapData.multiflyInterval) * _mapData.addMultifly): 1;

    public int HuntingCount { set; get; }

    [Header("꾸며지는 오브젝트")]
    [SerializeField] List<GameObject> _subObjects;
    [SerializeField] List<GameObject> _mainObjects;

    GameObject _objectFolder;
    GameObject ObjectFolder
    {
        get
        {
            if (_objectFolder == null)
            {
                _objectFolder = new GameObject("ObjectFolder");
            }

            return _objectFolder;
        }
    }


    public List<List<ItemData>> RankItemDataList = new List<List<ItemData>>();

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


    [field: SerializeField] public Inventory Inventory { set; get; }

    SpriteRenderer _dark;
    SpriteRenderer _darkGround;

    public bool IsStartBeyondDead { set; get; }
    public override void Init()
    {
        Inventory = new Inventory();
        LoadItemData();
        LoadMainCharacters();

        _cameraController = Camera.main.GetComponent<CameraController>();
        LoadMapData();

    }
    public override void ManagerUpdate()
    {
        Debuging();
        Mental -= Time.deltaTime * Util.CalcPercentage(MentalAccelerationPercentage);
        _totalTime += Time.deltaTime;
        _stageTime += Time.deltaTime;
        if (Mental <= 0)
        {
            _panicLevel++;
            Mental += 100;
        }
        if (Mental > 100)
        {
            _panicLevel--;
            Mental -= 100;
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            UIStatus uIStatus = Managers.GetManager<UIManager>().GetUI<UIStatus>();
            if (uIStatus.gameObject.activeSelf)
                uIStatus.Close();
            else
                uIStatus.Open();
        }

        Inventory.InventoryUpdate();
        if (_stop) return;

        TimeWave();
        DistanceWave();
        MentalWave();

        if (Player)
            _farDistance = _farDistance < Player.transform.position.x ? Player.transform.position.x : _farDistance;


    }

    void LoadItemData()
    {
        List<ItemData> itemList = Managers.GetManager<DataManager>().GetDataList<ItemData>();
        
        foreach (var item in itemList)
        {
            int rank = item.Rank;

            for (int i = RankItemDataList.Count; i <= rank; i++)
            {
                RankItemDataList.Add(new List<ItemData>());
            }
            RankItemDataList[rank].Add(item);
        }
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

            //// 이벤트 배치
            //float distance = 0;
            //if (_mapData.randomEvent.Count > 0)
            //{
            //    // 상점 일정 거리마다 배치
            //    while (distance < _mapData.mapSize)
            //    {
            //        distance += 150;
            //        Vector3? position = GetGroundTop(new Vector3(distance, 0));
            //        if (position.HasValue)
            //        {
            //            GameObject go = Managers.GetManager<ResourceManager>().Instantiate(_mapData.randomEvent[0]);
            //            go.transform.position = position.Value;
            //        }
            //    }
            //    distance = 0;

            //    while (distance < _mapData.mapSize)
            //    {
            //        distance += Random.Range( _mapData.randomEventInterval-20, _mapData.randomEventInterval + 20);
            //        Vector3? position = GetGroundTop(new Vector3(distance, 0));
            //        if (position.HasValue)
            //        {
            //            if (_mapData.randomEvent.Count > 1)
            //            {
            //                GameObject go = Managers.GetManager<ResourceManager>().Instantiate(_mapData.randomEvent[Random.Range(1, _mapData.randomEvent.Count)]);
            //                go.transform.position = position.Value;
            //            }
            //        }
            //    }

            //}
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
                if (go)
                {
                    go.transform.position = position.Value;
                    go.transform.SetParent(ObjectFolder.transform);

                }
            }
            distance += Random.Range(50, 60);
        }
        distance = 3.5f;
        while (distance < _mapData.mapSize)
        {
            Vector3? position = GetGroundTop(Vector3.one * distance);
            if (position.HasValue)
            {
                GameObject go = Managers.GetManager<ResourceManager>().Instantiate(_subObjects.GetRandom());
                if (go)
                {
                    go.transform.position = position.Value;
                    go.transform.SetParent(ObjectFolder.transform);
                }
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
  
    void Debuging()
    {
        if (Time.timeScale != 0)
        {
            if (_timeScale <= 0)
                Time.timeScale = 0.01f;
            else
                Time.timeScale = _timeScale;
        }
        if (_summonDummy)
        {
            EnemyNameDefine enemyOrigin = Managers.GetManager<DataManager>().GetData<EnemyNameDefine>((int)Define.EnemyName.Slime);
            EnemyNameDefine enemy = Managers.GetManager<ResourceManager>().Instantiate(enemyOrigin);
            enemy.transform.position = Player.transform.position + Vector3.right * 10;
            _dummy = enemy.gameObject;
            Character enemyCharacter = enemy.GetComponent<Character>();
            enemyCharacter.SetHp(_dummyHp);
            _summonDummy = false;

            _enemySpawnList.Add(enemy.gameObject);
        }
        if (_destroyDummy)
        {
            if(_dummy)
                Managers.GetManager<ResourceManager>().Destroy(_dummy);
            _destroyDummy = false;
        }
        if (_addItem)
        {
            Inventory.AddItem(Managers.GetManager<DataManager>().GetData<ItemData>((int)(_itemName)));
            _addItem = false;
        }
        if(_removeItem)
        {
            Inventory.RemoveItem(Managers.GetManager<DataManager>().GetData<ItemData>((int)(_itemName)));
            _removeItem = false;
        }
       
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

                if (timeWaveData.enemyName == Define.EnemyName.None)
                {
                    Vector3 genLocalPosition = timeWaveData.genLocalPosition;
                    if (Mathf.Abs(_farDistance - _girl.transform.position.x) > 50)
                    {
                        genLocalPosition.x = -timeWaveData.genLocalPosition.x;
                    }
                    Vector3? topPosition = GetGroundTop(CameraController.transform.position + genLocalPosition);
                    if (topPosition.HasValue)
                    {
                        GameObject preset = Managers.GetManager<ResourceManager>().Instantiate(timeWaveData.enemyPreset);
                        preset.transform.position = topPosition.Value;
                        for(int i = 0; i < preset.transform.childCount; i++)
                        {
                            // 각 개체 체력 설정
                            Character character = preset.transform.GetChild(i).GetComponent<Character>();
                            character.SetHp(Mathf.RoundToInt(character.MaxHp * (1 + PanicLevel * _mapData.addMultifly)));
                        }
                        _enemySpawnList.Add(preset);
                    }
                }
                else
                {
                    EnemyNameDefine enemyOrigin = Managers.GetManager<DataManager>().GetData<EnemyNameDefine>((int)timeWaveData.enemyName);
                    EnemyNameDefine enemy = Managers.GetManager<ResourceManager>().Instantiate(enemyOrigin);
                    if (enemy)
                    {
                        // 위치 설정
                        Vector3? topPosition = GetGroundTop(CameraController.transform.position + timeWaveData.genLocalPosition);
                        if (topPosition.HasValue)
                        {
                            Character enemyCharacter = enemy.GetComponent<Character>();

                            // 체력 설정
                            if (enemy.IsGroup)
                                enemy.GetComponent<EnemyGroup>().SetHp(1 + PanicLevel * _mapData.addMultifly);
                            else
                                enemyCharacter.SetHp(Mathf.RoundToInt(enemyCharacter.MaxHp * (1 + PanicLevel * _mapData.addMultifly)));


                            enemyCharacter.transform.position = topPosition.Value;

                            // 맵 적 스폰추가
                            if (enemy)
                                _enemySpawnList.Add(enemy.gameObject);
                        }
                        _enemySpawnList.Add(enemy.gameObject);
                    }
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

                GenerateCharacter(enemyOrigin.gameObject, _cameraController.transform.position + distanceWaveData.genLocalPosition);
               
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
            if (mentalWaveData.genMentalLevelMore > PanicLevel || mentalWaveData.genMentalLevelLess < PanicLevel)
                continue;

            if (wave.elapsedTime < wave.waveData.genTime)
            {
                wave.elapsedTime += Time.deltaTime;
            }
            else
            {
                wave.elapsedTime = 0;

                if (wave.waveData.enemyName == Define.EnemyName.None)
                {

                    Vector3 genLocalPosition = mentalWaveData.genLocalPosition;
                    if (Mathf.Abs(_farDistance - _girl.transform.position.x) > 50)
                    {
                        genLocalPosition.x = -mentalWaveData.genLocalPosition.x;
                    }
                    GenerateCharacter(mentalWaveData.enemyPreset, CameraController.transform.position + genLocalPosition, true);
                }
                else
                {
                    EnemyNameDefine enemyOrigin = Managers.GetManager<DataManager>().GetData<EnemyNameDefine>((int)wave.waveData.enemyName);

                    GenerateCharacter(enemyOrigin.gameObject, CameraController.transform.position + mentalWaveData.genLocalPosition, true);
                }
            }
        }

    }
  
    public Vector3? GetGroundTop(Vector3 position)
    {
        position.y = 50;
        RaycastHit2D hit = Physics2D.Raycast(position, Vector2.down, 500, LayerMask.GetMask("Ground"));

        if (hit.collider == null) return null;

        return hit.point;
    }

    public void GenerateCharacter(GameObject go, Vector3 position,bool isTopGround = false)
    {
        if (isTopGround)
        {
            Vector3? topPosition = GetGroundTop(position);
            if(topPosition.HasValue)
                position = topPosition.Value;
        }

        GameObject enemy = Managers.GetManager<ResourceManager>().Instantiate(go);
        {
            Character character = enemy.GetComponent<Character>();
            enemy.transform.position = position;
            if (character)
            {
                character.SetHp(Mathf.RoundToInt(character.MaxHp * (1 + PanicLevel * _mapData.addMultifly)));
                character.AttackPower = Mathf.RoundToInt(character.AttackPower * (1 + PanicLevel * _mapData.addMultifly));
            }
        }
        {
            Character[] characters = enemy.GetComponentsInChildren<Character>();
            foreach (var character in characters)
            {
                if (character)
                {
                    character.SetHp(Mathf.RoundToInt(character.MaxHp * (1 + PanicLevel * _mapData.addMultifly)));
                    character.AttackPower = Mathf.RoundToInt(character.AttackPower * (1 + PanicLevel * _mapData.addMultifly));
                }
            }
        }
        _enemySpawnList.Add(enemy);
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

        _stop = true;

        Managers.GetManager<UIManager>().GetUI<UIInGame>().LoadSceneFadeOut();
        yield return new WaitForSeconds(2);
        _girl.transform.position = GetGroundTop(new Vector3(-40, 0, 0)).Value;
        _creature.transform.position = GetGroundTop(new Vector3(-46, 0, 0)).Value;
        SceneManager.LoadScene(mapData.name);

        _mapData = mapData;
        LoadMapData();
        LoadNewSceneHandler?.Invoke(mapData);
        _stop = false;
    }
    public Vector3 GetRightOutScreenPosition()
    {
        Vector3 position = CameraController.transform.position;
        position.z = 0;
        position.x += _cameraController.GetCameraWidth() / 2;
        return position;
    }

    public void GameOver()
    {
        Managers.Destroy();
        SceneManager.LoadScene("MainMenu");
    }

    public override void ManagerDestroy()
    {
        Destroy(_girl.gameObject);
        Destroy(_creature.gameObject);
    }

    public void LoadBeyondDeath()
    {
        _stop = true;
        StartCoroutine(CorPlayBeyondDeath());
    }

    IEnumerator CorPlayBeyondDeath()
    {
        if (_dark == null) 
            _dark = Managers.GetManager<ResourceManager>().Instantiate("Prefabs/Dark").gameObject.GetComponent<SpriteRenderer>();

        float time = 0;

        GroundGroup groundGroup = GameObject.Find("Grounds").GetComponent<GroundGroup>();

        SortingGroup girlGroup = _girl.GetComponent<SortingGroup>();
        SortingGroup creatureGroup = _creature.GetComponent<SortingGroup>();
       
        string preSortingLayerName = girlGroup.sortingLayerName;
        int preSortingOrder = girlGroup.sortingOrder;

        string creaturePreSortingLayerName = creatureGroup.sortingLayerName;
        int creaturePreSortingOrder = creatureGroup.sortingOrder;

        girlGroup.sortingLayerName = "Character";
        girlGroup.sortingOrder = 1003;
        creatureGroup.sortingLayerName = "Character";
        creatureGroup.sortingOrder = 1002;

        
        while (time < 3)
        {
            time += Time.deltaTime;
            _dark.color = new Color(0, 0, 0, time / 3);
            groundGroup.SetColor(new Color((3-time) / 3, (3 - time) / 3, (3 - time) / 3));
            yield return null;
        }
        IsStartBeyondDead = true;

        foreach (var enemy in _enemySpawnList)
        {
            Managers.GetManager<ResourceManager>().Destroy(enemy);
        }
        _enemySpawnList.Clear();
        Vector3 deadPosition = _girl.transform.position;

        Player.PlayRevive();

        GameObject go = Managers.GetManager<ResourceManager>().Instantiate("Prefabs/Event/VendingMechine");
        go.transform.position = _girl.transform.position + Vector3.right * 70;
        go.GetComponent<SortingGroup>().sortingLayerName = "Character";
        go.GetComponent<SortingGroup>().sortingOrder = 1001;

        while (_girl.transform.position.x - deadPosition.x < 140)
        {
            yield return new WaitForSeconds(1);
        }

        LoadMapData();

        Managers.GetManager<ResourceManager>().Destroy(go);

        float distance = - _girl.transform.position.x;
        _girl.transform.position = new Vector3(0, _girl.transform.position.y);

        _creature.transform.position += Vector3.right * distance;
        _cameraController.transform.position += Vector3.right * distance;
        time = 0;
        _farDistance = 0;
        IsStartBeyondDead = false;
        while (time < 3)
        {
            time += Time.deltaTime;
            _dark.color = new Color(0, 0, 0, (3-time) / 3);
            groundGroup.SetColor(new Color(time / 3, time / 3, time / 3));

            yield return null;
        }
        girlGroup.sortingLayerName = preSortingLayerName;
        girlGroup.sortingOrder = preSortingOrder;

        creatureGroup.sortingLayerName = creaturePreSortingLayerName;
        creatureGroup.sortingOrder = creaturePreSortingOrder;

        _stop = false;
    }

}