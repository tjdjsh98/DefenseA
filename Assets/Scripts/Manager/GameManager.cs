using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class GameManager : ManagerBase
{
    #region ĳ����
    public Player Player { set; get; }

    public CreatureAI CreatureAI { set; get; }

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
    [SerializeField] bool _addSkill;
    [SerializeField] bool _addNpcCard;
    [SerializeField] bool _immdiatelyEnterBeyondDeath;
    
    [Header("���� ����")]
    [SerializeField] bool _stop;
    [SerializeField] MapData _mapData;
    public string LevelName=>_mapData != null ? _mapData.name : "";
    public float MapSize => IsStartBeyondDead?1000000: _mapData.mapSize;
    float _farDistance;

    public bool IsLoadEnd { set; get; }
    public Action<MapData> LoadNewSceneHandler { set; get; }

    public int Money { set; get; } = 0;
    public int EnableRestockCount { set; get; }

    #region ��Ż, �д�
    int _panicLevel = 0;
    public int PanicLevel =>_panicLevel;
    public float MaxMental { get; } = 100;
    public float Mental { set; get; } = 100;
    public float MentalAccelerationPercentage = 0;
    public int DeathCount { set; get; }
    public Character Boss { get; set; }

    #endregion

    [Header("Ÿ�Ӷ���")]
    [SerializeField] bool _isSkip;
    [SerializeField] PlayableDirector _playableDirector;
    [SerializeField] PlayableAsset _enteracneTimeline;
    bool _isPlayTimeline;
    public bool IsPlayTimeline => _isPlayTimeline;

    // �� ��ȯ ���� ����
    [Header("�����ú���")]
    List<GameObject> _enemySpawnList = new List<GameObject>();
    List<Wave> _distanceWaveList = new List<Wave>();
    List<Wave> _bossWaveList = new List<Wave>();
    List<Wave> _mentalWaveList = new List<Wave>();
    float _totalTime;
    public float TotalTime => _totalTime;
    float _stageTime;
    public float StageTime => _stageTime;
    // �дп� ���� ���� ������
    public float EnemyStatusMultifly => _mapData != null ?(_mapData.initMutifly + (PanicLevel) * _mapData.addMultifly): 1;

    public int HuntingCount { set; get; }

    [Header("�ٸ����� ������Ʈ")]
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

    GameObject _helpBox;
    TextMeshPro _helpText;

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


    public float BeyondDeathInterval { get; set; } = 90f;
    float _nextBeyondDeath ;
    public float NextBeyondDeath => _nextBeyondDeath;
    SpriteRenderer _dark;
    SpriteRenderer _darkGround;

    public bool IsStartBeyondDead { set; get; }

    // ������ ���̺�
    public int SaveItemCount { get; set; } = 1;
    public List<ItemInfo> SaveItemList { get; set; } = new List<ItemInfo>();

    // �ָ����� ���ʽ�
    List<GameObject> _walletList = new List<GameObject>();

    public override void Init()
    {
        _nextBeyondDeath = BeyondDeathInterval;
        if(GameObject.Find("HelpBox"))
            _helpBox = GameObject.Find("HelpBox").gameObject;
        if (_helpBox && _helpBox.transform.Find("HelpText"))
        {
            _helpText = _helpBox.transform.Find("HelpText").GetComponent<TextMeshPro>();
        }
        Inventory = new Inventory();
        LoadItemData();
        LoadMainCharacters();

        _cameraController = Camera.main.GetComponent<CameraController>();
        LoadMapData();
        LoadObjects();

    }
    public override void ManagerUpdate()
    {

        // �ӽ�
        if (Input.GetKeyDown(KeyCode.O))
        {
            Money += 500;
            UIItemUpgrade uIStatus = Managers.GetManager<UIManager>().GetUI<UIItemUpgrade>();
                uIStatus.Open();
        }
        _totalTime += Time.deltaTime;
        Debuging();

        if (_nextBeyondDeath < _stageTime)
        {
            LoadBeyondDeath();
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
        Mental -= Time.deltaTime *1.5f * (1 + (MentalAccelerationPercentage / 100f));
     
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

        DistanceWave();
        BossWave();
        MentalWave();

        if (Player)
            _farDistance = _farDistance < Player.transform.position.x ? Player.transform.position.x : _farDistance;


        if (Boss == null &&Player && !_girl.IsDead)
        {
            if (Player.transform.position.x > MapSize)
            {
                GameEnding();
            }

        }
    }

    void LoadItemData()
    {
        List<ItemData> itemList = Managers.GetManager<DataManager>().GetDataList<ItemData>(item=>
        {
            if (item.ItemName.ToString().Contains('_')) return false;

            return true;
        });
        
        for(int i = 0; i < 4;i++)
            RankItemDataList.Add(new List<ItemData>());

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
        _distanceWaveList.Clear();
        _bossWaveList.Clear();
        _mentalWaveList.Clear();

        // �� ��ȯ ������
        if (_mapData)
        {
            foreach (var waveData in _mapData.distanceWave)
            {
                DistanceWaveData timeWaveData = waveData as DistanceWaveData;
                if (timeWaveData != null)
                {
                    _distanceWaveList.Add(new Wave() { waveData = timeWaveData, elapsedTime = timeWaveData.genTime });
                }
            }
            foreach (var waveData in _mapData.bossWave)
            {
                BossWaveData distanceWaveData = waveData as BossWaveData;
                if (distanceWaveData != null)
                {
                    _bossWaveList.Add(new Wave() { waveData = distanceWaveData, elapsedTime = distanceWaveData.genTime });
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

            //// �̺�Ʈ ��ġ
            //float distance = 0;
            //if (_mapData.randomEvent.Count > 0)
            //{
            //    // ���� ���� �Ÿ����� ��ġ
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

        for (int distance = 200; distance < _mapData.mapSize; distance += 200)
        {
            if (_walletList.Count <= (distance / 200) -1)
            {
                _walletList.Add(null);
            }

            if (_walletList[(distance / 200) - 1] == null)
            {
                GameObject go = Managers.GetManager<ResourceManager>().Instantiate("Prefabs/Wallet");
                go.transform.position = GetGroundTop(new Vector3(distance,0,0)).Value; 
                _walletList.Add(go);
            }

        }

        _farDistance = 0;
        _stageTime = 0;
        _nextBeyondDeath = BeyondDeathInterval;
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
        if (_immdiatelyEnterBeyondDeath)
        {
            LoadBeyondDeath();
            _immdiatelyEnterBeyondDeath = false;
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
            Inventory.RemoveItem(_itemName);
            _removeItem = false;
        }
        if (_addSkill)
        {
            Managers.GetManager<UIManager>().GetUI<UICardSelection>().OpenSkillCardSelection();
            _addSkill = false;
        }
        if (_addNpcCard)
        {
            Managers.GetManager<UIManager>().GetUI<UICardSelection>().OpenNpcSelection();
            _addNpcCard = false;
        }
    }
    void DistanceWave()
    {
        if (IsPlayTimeline) return;
        foreach (var distanceWave in _distanceWaveList)
        {
            DistanceWaveData distanceWaveData = distanceWave.waveData as DistanceWaveData;
            if (_farDistance > distanceWaveData.endDistance || _farDistance < distanceWaveData.startDistance)
                continue;

            if (distanceWave.elapsedTime > distanceWave.waveData.genTime)
            {
                distanceWave.elapsedTime = 0;

                if (distanceWaveData.enemyName == Define.EnemyName.None)
                {
                    Vector3 genLocalPosition = distanceWaveData.genLocalPosition;
                    if (Mathf.Abs(_farDistance - _girl.transform.position.x) > 100)
                    {
                        genLocalPosition.x = -distanceWaveData.genLocalPosition.x;
                    }
                    Vector3? topPosition = GetGroundTop(CameraController.transform.position + genLocalPosition);
                    if (topPosition.HasValue)
                    {
                        GameObject preset = Managers.GetManager<ResourceManager>().Instantiate(distanceWaveData.enemyPreset);
                        preset.transform.position = topPosition.Value;
                        for(int i = 0; i < preset.transform.childCount; i++)
                        {
                            // �� ��ü ü�� ����
                            Character character = preset.transform.GetChild(i).GetComponent<Character>();
                            character.SetHp(Mathf.RoundToInt(character.MaxHp * EnemyStatusMultifly));
                        }
                        _enemySpawnList.Add(preset);
                    }
                }
                else
                {
                    EnemyNameDefine enemyOrigin = Managers.GetManager<DataManager>().GetData<EnemyNameDefine>((int)distanceWaveData.enemyName);
                    EnemyNameDefine enemy = Managers.GetManager<ResourceManager>().Instantiate(enemyOrigin);
                    if (enemy)
                    {
                        // ��ġ ����
                        Vector3? topPosition = GetGroundTop(CameraController.transform.position + distanceWaveData.genLocalPosition);
                        if (topPosition.HasValue)
                        {
                            Character enemyCharacter = enemy.GetComponent<Character>();

                            // ü�� ����
                            if (enemy.IsGroup)
                                enemy.GetComponent<EnemyGroup>().SetHp(1 + PanicLevel * _mapData.addMultifly);
                            else
                                enemyCharacter.SetHp(Mathf.RoundToInt(enemyCharacter.MaxHp * EnemyStatusMultifly));


                            enemyCharacter.transform.position = topPosition.Value;

                            // �� �� �����߰�
                            if (enemy)
                                _enemySpawnList.Add(enemy.gameObject);
                        }
                        _enemySpawnList.Add(enemy.gameObject);
                    }
                }
            }
            else
            {
                distanceWave.elapsedTime += Time.deltaTime;
            }
        }
    }
    void BossWave()
    {
        foreach (var wave in _bossWaveList)
        {
            BossWaveData bossWaveData = wave.waveData as BossWaveData;
            // ��Ż �� ��� ��ü
            if (bossWaveData.distance <= _panicLevel)
            {
                EnemyNameDefine enemyOrigin = Managers.GetManager<DataManager>().GetData<EnemyNameDefine>((int)bossWaveData.enemyName);

                GameObject enemy = GenerateCharacter(enemyOrigin.gameObject, _cameraController.transform.position + bossWaveData.genLocalPosition);

                Boss = enemy.GetComponent<Character>();
                Boss.CharacterDeadHandler += () =>
                {
                    _nextBeyondDeath += 120f;
                };
                _bossWaveList.Remove(wave);
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

    public GameObject GenerateCharacter(GameObject go, Vector3 position,bool isTopGround = false)
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
            position.z = 0;
            enemy.transform.position = position;
            if (character)
            {
                character.SetHp(Mathf.RoundToInt(character.MaxHp * EnemyStatusMultifly));
                character.AttackPower = Mathf.RoundToInt(character.AttackPower * EnemyStatusMultifly);
            }
        }
        {
            Character[] characters = enemy.GetComponentsInChildren<Character>();
            foreach (var character in characters)
            {
                if (character)
                {
                    character.SetHp(Mathf.RoundToInt(character.MaxHp * EnemyStatusMultifly));
                    character.AttackPower = Mathf.RoundToInt(character.AttackPower * EnemyStatusMultifly);
                }
            }
        }
        _enemySpawnList.Add(enemy);
        return enemy;
    }
    public GameObject GetRandomEnemy()
    {
        return _enemySpawnList.GetRandom();
    }
    public GameObject GetCloseEnemyFromGirl()
    {
        GameObject go = null;
        float distance = 0;
        foreach (var enemy in _enemySpawnList)
        {
            if (go == null)
            {
                go = enemy;
                distance = (go.transform.position - Girl.transform.position).magnitude;
            }
            else if((go.transform.position - Girl.transform.position).magnitude > (enemy.transform.position - Girl.transform.position).magnitude)
            {
                go = enemy;
                distance = (go.transform.position - Girl.transform.position).magnitude;
            }
        }
        return go;
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
        _stageTime = 0;
        _nextBeyondDeath = BeyondDeathInterval;
        _panicLevel = 0;
        Mental = 100f;
        MentalAccelerationPercentage += 30f;
        StartCoroutine(CorPlayBeyondDeath());
    }

    IEnumerator CorPlayBeyondDeath()
    {
        DeathCount++;
        if (_dark == null) 
            _dark = Managers.GetManager<ResourceManager>().Instantiate("Prefabs/Dark").gameObject.GetComponent<SpriteRenderer>();

        float time = 0;
        GroundGroup groundGroup = null;
        if(GameObject.Find("Grounds") != null)
            groundGroup = GameObject.Find("Grounds").GetComponent<GroundGroup>();

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
            groundGroup?.SetColor(new Color((3-time) / 3, (3 - time) / 3, (3 - time) / 3));

            yield return null;
        }
        IsStartBeyondDead = true;
        
        foreach (var enemy in _enemySpawnList)
        {
            Managers.GetManager<ResourceManager>().Destroy(enemy);
        }
        _enemySpawnList.Clear();
        Vector3 deadPosition = _girl.transform.position;

        // ����â ����
        if (_helpBox)
        {
            Destroy(_helpBox);
            _helpBox = null;
        }

        Player.PlayRevive();
        if (_creature.IsDead)
            CreatureAI.ForceRevive();

        _girl.Hp += _girl.MaxHp;
        _creature.Hp += _creature.MaxHp;

        Managers.GetManager<UIManager>().GetUI<UIInGame>().ShowText("���� ���� �������� ��� ������ϴ�.");

        yield return new WaitForSeconds(1f);
        UICardSelection uiCardSelection = Managers.GetManager<UIManager>().GetUI<UICardSelection>();
        uiCardSelection.OpenNpcSelection();

        while (uiCardSelection.isActiveAndEnabled)
        {
            yield return new WaitForSeconds(0.3f);
        }

        // ������ �ִ� ������ ��� ����
        List<ItemInfo> itemList = Inventory.GetItemInfoList(info=> 
        { 
            if(info.ItemData.ItemName == ItemName.�Ƹ��������� || info.ItemData.ItemName == ItemName.�Ƹ���������_A|| info.ItemData.ItemName == ItemName.�Ƹ���������_B) return false;
            return !SaveItemList.Contains(info); 
        });
        foreach (var info in itemList)
        {
            Inventory.RemoveItem(info);
        }


        //ī�� ���ÿ� ���� ȿ��
        Card card = Managers.GetManager<CardManager>().GetCard(CardName.���㵷);
        {
            if(card != null && card.rank >= 0)  
            {
                Money += (card.rank + 1) * 50;
            }
        }


        List<GameObject> npcList = new List<GameObject>();
        
        card = Managers.GetManager<CardManager>().GetCard(CardName.���ǱⰭȭ);
        {
            
            GameObject go = null;
            if (card == null || card.rank < 0)
            {
                go = Managers.GetManager<ResourceManager>().Instantiate("Prefabs/Event/VendingMechineLv0");
            }
            else
            {
                int lv = card.rank + 1;
                
                go = Managers.GetManager<ResourceManager>().Instantiate($"Prefabs/Event/VendingMechineLv{lv}");
            }



            go.transform.position = _girl.transform.position + Vector3.right * 70;
            go.GetComponent<SortingGroup>().sortingLayerName = "Character";
            go.GetComponent<SortingGroup>().sortingOrder = 1001;
            npcList.Add(go);
        }
        if (Managers.GetManager<CardManager>().GetIsHaveAbility(CardName.���ⰭȭNPC))
        {
            GameObject go = Managers.GetManager<ResourceManager>().Instantiate("Prefabs/Event/��ȭNPC");
            go.transform.position = _girl.transform.position + Vector3.right * 80;
            go.GetComponent<SortingGroup>().sortingLayerName = "Character";
            go.GetComponent<SortingGroup>().sortingOrder = 1001;
            npcList.Add(go);
        }

        if (Managers.GetManager<CardManager>().GetIsHaveAbility(CardName.��������))
        {
            GameObject go = Managers.GetManager<ResourceManager>().Instantiate("Prefabs/Event/ũ���İ�ȭNPC");
            go.transform.position = _girl.transform.position + Vector3.right * 90;
            go.GetComponent<SortingGroup>().sortingLayerName = "Character";
            go.GetComponent<SortingGroup>().sortingOrder = 1001;
            npcList.Add(go);
        }

        if (Managers.GetManager<CardManager>().GetIsHaveAbility(CardName.�����۰�ȭNPC))
        {
            GameObject go = Managers.GetManager<ResourceManager>().Instantiate("Prefabs/Event/�����۰�ȭNPC");
            go.transform.position = _girl.transform.position + Vector3.right * 100;
            go.GetComponent<SortingGroup>().sortingLayerName = "Character";
            go.GetComponent<SortingGroup>().sortingOrder = 1001;
            npcList.Add(go);
        }
        if (Managers.GetManager<CardManager>().GetIsHaveAbility(CardName.�����ۼ��̺�NPC))
        {
            GameObject go = Managers.GetManager<ResourceManager>().Instantiate("Prefabs/Event/�κ��丮���̹�NPC");
            go.transform.position = _girl.transform.position + Vector3.right * 110;
            go.GetComponent<SortingGroup>().sortingLayerName = "Character";
            go.GetComponent<SortingGroup>().sortingOrder = 1001;
            npcList.Add(go);
        }

        // �ҳ� ĳ���Ͱ� �����Ÿ� �̻� ������ ���� �ٽ� ����

        while (_girl.transform.position.x - deadPosition.x < 140)
        {
            yield return new WaitForSeconds(1);
        }


        // NPC���� �ٽ� ����
        foreach(var go in npcList) 
            Managers.GetManager<ResourceManager>().Destroy(go);

        float distance = _girl.transform.position.x;
        _girl.transform.position = new Vector3(0, _girl.transform.position.y);

        _creature.transform.position -= Vector3.right * distance;
        _cameraController.transform.position -= Vector3.right * distance;
        time = 0;
        IsStartBeyondDead = false;
        while (time < 3)
        {
            time += Time.deltaTime;
            _dark.color = new Color(0, 0, 0, (3-time) / 3);
            groundGroup?.SetColor(new Color(time / 3, time / 3, time / 3));

            yield return null;
        }

      
        // ���� �� ����
        Money = 0;

        LoadMapData();
        girlGroup.sortingLayerName = preSortingLayerName;
        girlGroup.sortingOrder = preSortingOrder;

        creatureGroup.sortingLayerName = creaturePreSortingLayerName;
        creatureGroup.sortingOrder = creaturePreSortingOrder;

        _stop = false;
    }
    public void GameEnding()
    {
        Managers.GetManager<UIManager>().GetUI<UIEnding>().Open();
    }
}