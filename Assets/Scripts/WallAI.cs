using MoreMountains.FeedbacksForThirdParty;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.Timeline;

public class WallAI : MonoBehaviour
{
    Character _character;
    BoxCollider2D _boxCollider;
    GameObject _model;
    GameObject _soulModel;

    [SerializeField] WeaponSwaper _weaponSwaper;

    [SerializeField] Define.Range _attackRange;
    [SerializeField] GameObject _sitPosition;
    public GameObject SitPosition => _sitPosition;


    [Header("이동을 하기 위핸 조건 변수")]
    [SerializeField] Define.Range _detecctRange;
    [SerializeField] float _playerDistance;
    bool _isDetectEnemy;
    Coroutine _detectEnemyCoroutine;

    [SerializeField]float _reviveCoolTime = 30;
    public float OriginalReviveTime => _reviveCoolTime;
    public float ReviveTime => DecreasedReviveTimePercetage > 0 ? 
        _reviveCoolTime /(1 + DecreasedReviveTimePercetage/100) : _reviveCoolTime * (1 - DecreasedReviveTimePercetage / 100);
    float _reviveElapsedTime = 0;

    // 능력 해금
    public Dictionary<Define.WallAbility, bool> AbilityUnlocks { set; get; } = new Dictionary<Define.WallAbility, bool>();


    // 폭파능력
    [field:Header("폭파 능력")]
    [field:SerializeField]public float ExplosionRange { set; get; } = 10;
    [field: SerializeField] public float ExplosionPower { set; get; } = 50;
    [field: SerializeField] public int ExplosionDamage { set; get; } = 50;

    // 베리어
    float _barrierCoolTime;
    float _barrierDurationTime = 10;
    float _barrierElaspedTime;
    float _barrierRange = 30;
    Barrier _barrier;

    // 죽음 효과
    int _brickIndex = 0;
    [SerializeField] List<GameObject> _wallBricks= new List<GameObject>();
    [SerializeField] List<Vector3> _wallBricksPosition= new List<Vector3>();
    [SerializeField] LineRenderer _lineRenderer;
    float _onewayTime;
    float _onewayElapsedTime;
    bool _isBrickAttach;

    // 추가적 능력치
    public int ReflectionDamage { set; get; } = 0;
    public float DecreasedReviveTimePercetage { set; get; }

    bool _isSoulForm;
    [SerializeField] bool _brickTest = false;
   


    private void Awake()
    {
        _character = GetComponent<Character>();
        _boxCollider = GetComponent<BoxCollider2D>();
        _model = transform.Find("Model").gameObject;
        _soulModel = transform.Find("SoulModel").gameObject;

        Managers.GetManager<GameManager>().WallAI = this;

        _character.CharacterDamaged += OnCharacterDamaged;
        _character.CharacterDeadHandler += OnCharacterDead;
    }

    private void OnCharacterDead()
    {
        _character.AnimatorSetBool("Dead", true);
        if (AbilityUnlocks.TryGetValue(Define.WallAbility.SelfDestruct,out bool value) && value)
        {

            Effect effectOrigin = Managers.GetManager<DataManager>().GetData<Effect>((int)Define.EffectName.Explosion);
            Effect effect = Managers.GetManager<ResourceManager>().Instantiate(effectOrigin);
            effect.SetProperty("Radius", (ExplosionRange - 10) / 2);
            effect.SetProperty("BubbleCount", (int)Mathf.Pow(ExplosionRange / 5, 2));
            effect.transform.position = transform.position;

            effect.Play(_character.GetCenter());

            GameObject[] gameObjects = Util.RangeCastAll2D(gameObject, new Define.Range()
            {
                center = _character.GetCenter(),
                size = new Vector3(ExplosionRange, ExplosionRange, ExplosionRange),
                figureType = Define.FigureType.Circle
            });

            foreach (var gameObject in gameObjects)
            {
                Character character = gameObject.GetComponent<Character>();
                if (character && character.CharacterType == Define.CharacterType.Enemy)
                {
                    _character.Attack(character, ExplosionDamage, ExplosionPower, character.transform.position
                        - _character.GetCenter(), 1);
                }
            }
        }

        _wallBricks.Clear();
        for (int i = 0; i < 6; i++)
        {
            GameObject go = Managers.GetManager<ResourceManager>().Instantiate("Prefabs/WallBrick");
            go.transform.position = transform.position + Vector3.up;
            go.GetComponent<Rigidbody2D>().AddForce((new Vector3(UnityEngine.Random.Range(-1f, 1f), 1)).normalized * 100, ForceMode2D.Impulse);
            _wallBricks.Add(go);
        }
        _onewayTime = _reviveCoolTime / _wallBricks.Count/2;
    }

    private void OnCharacterDamaged(Character attacker, int damage, float power, Vector3 direction, float stunTIme)
    {
        if(ReflectionDamage != 0)
            _character.Attack(attacker, ReflectionDamage, 0, Vector3.zero, 0);

        if (AbilityUnlocks.TryGetValue(Define.WallAbility.GenerateSphere, out bool value) && value)
        {
            GameObject go = Managers.GetManager<ResourceManager>().Instantiate("Prefabs/BlackSphere");
            BlackSphere blackSphere = go.GetComponent<BlackSphere>();
            if(blackSphere != null)
            {
                blackSphere.transform.position = transform.position;
                blackSphere.Init(_character,new Vector3(-3,5));
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(transform.position + _attackRange.center, _attackRange.size);
        Util.DrawRangeOnGizmos(gameObject, _detecctRange, Color.green);
    }
    private void Update()
    {
        if (_brickTest)
        {
          
            _brickTest = false;
        }

        if (_character.IsDead)
        {
            float remainTime = _reviveCoolTime - _reviveElapsedTime;
            int remainCount = _wallBricks.Count - _brickIndex;
            Vector3 brickVector = _wallBricks[_brickIndex].transform.position - _lineRenderer.transform.position;

            if (!_isBrickAttach)
                _onewayElapsedTime += Time.deltaTime;
            else
                _onewayElapsedTime -= Time.deltaTime;
            if (_isBrickAttach)
            {
                _wallBricks[_brickIndex].transform.position = _lineRenderer.transform.position + _lineRenderer.GetPosition(1);
            }

            _lineRenderer.SetPosition(1, Vector3.Lerp(_lineRenderer.GetPosition(0), _wallBricks[_brickIndex].transform.position - _lineRenderer.transform.position, _onewayElapsedTime / _onewayTime));

            if(_onewayElapsedTime>= _onewayTime && !_isBrickAttach)
            {
                _isBrickAttach = true;
            }
            if (_onewayElapsedTime <= 0 && _isBrickAttach)
            {
                _onewayElapsedTime = 0;
                _isBrickAttach = false;
                Managers.GetManager<ResourceManager>().Destroy(_wallBricks[_brickIndex]);
                _brickIndex++;
            }
        }

        Revive();

        if (_character.IsDead) return;
        //if (Input.GetKeyDown(KeyCode.C))
        //{
        //    Transform();
        //}
        if (_isSoulForm)
        {
            Player player = Managers.GetManager<GameManager>().Player;

            _character.TurnBody(player.transform.position - transform.position);
            Vector3 offset = new Vector3(-2, 6, 0);
            offset.y += Mathf.Sin(Time.time * 0.5f + gameObject.GetInstanceID());
            if (player.transform.localScale.x < 0)
                offset.x = -offset.x;

            transform.position = Vector3.Lerp(transform.position, player.transform.position + offset, 0.01f);

            return;
        }
        MoveForward();
        Targeting();

        // 임시
        if (_barrier == null&&Input.GetKeyDown(KeyCode.T))
        {
            ActivateBarrier();
        }
        Barrier();
    }

    void ActivateBarrier()
    {
        _barrier = Managers.GetManager<ResourceManager>().Instantiate("Prefabs/Barrier").GetComponent<Barrier>();
        _barrier.transform.parent = transform;
        _barrier.transform.position= transform.position;
        _barrier?.StartExpansion(_character,_barrierRange,_barrierDurationTime);
    }

    void Barrier()
    {
        if(_barrier)
        {
            _barrierElaspedTime += Time.deltaTime;
            if (_barrierElaspedTime > _barrierDurationTime)
            {
                _barrierElaspedTime = 0;
                Managers.GetManager<ResourceManager>().Destroy(_barrier.gameObject);
                _barrier = null;
            }
        }
    }

    void Transform()
    {
        if (!_isSoulForm)
        {
            _boxCollider.enabled = false;
            _isSoulForm = true;
            _model.gameObject.SetActive(false);
            _soulModel.gameObject.SetActive(true);
            _character.ChangeEnableFly(true);
        }
        else
        {
            Vector3? position = Util.GetGroundPosition(transform.position);
            if (position != null)
                transform.position = Util.GetGroundPosition(transform.position).Value;
            _boxCollider.enabled = true;
            _isSoulForm = false;
            _model.gameObject.SetActive(true);
            _soulModel.gameObject.SetActive(false);
            _character.ChangeEnableFly(false);
        }
    }
 
    void Revive()
    {
        if (_character.IsDead)
        {
            if (_reviveElapsedTime < _reviveCoolTime)
            {
                _reviveElapsedTime += Time.deltaTime;
            }
            else
            {
                _character.Revive();
                _character.AnimatorSetBool("Dead",false);
                _reviveElapsedTime = 0;
                //if (AbilityUnlocks.TryGetValue(Define.WallAbility.SelfDestruct, out bool value) && value)
                
                //    transform.position = Managers.GetManager<GameManager>().Player.transform.position;
            }
        }

    }
    protected virtual void MoveForward()
    {
        if (_barrier) return;

        if (_detectEnemyCoroutine == null)
            _detectEnemyCoroutine = StartCoroutine(CorDetectEnemy());

        if(!_isDetectEnemy && ((transform.position.x - Managers.GetManager<GameManager>().Player.transform.position.x) < _playerDistance))
            _character.Move(Vector2.right);
    }

    IEnumerator CorDetectEnemy()
    {
        while (true)
        {
            if (_character.IsDead)
            {
                yield return new WaitForSeconds(0.2f);
                continue;
            }
            bool isDetect = false;
            GameObject[] gos = Util.RangeCastAll2D(gameObject, _detecctRange, LayerMask.GetMask("Character"));

            foreach (var go in gos)
            {
                if (go == null) continue;
                Character c = go.GetComponent<Character>();
                if (c != null && c.CharacterType == Define.CharacterType.Enemy)
                {
                    isDetect = true;
                    break;
                }
            }
            _isDetectEnemy = isDetect;
            yield return new WaitForSeconds(0.2f);
        }
    }

    public void Targeting()
    {
        List<GameObject> hands = _weaponSwaper.WeaponSlotList;

        for(int i =0; i < hands.Count; i++) 
        {
            if(_weaponSwaper.GetWeapon(i) == null) continue;

            GameObject[] gos = Util.RangeCastAll2D(hands[i], _attackRange);

            Character mostClose = null;
            float closeDistance = 100;
            foreach (var go in gos)
            {
                Character c = go.GetComponent<Character>();

                if (c != null && c.CharacterType == Define.CharacterType.Enemy)
                {
                    if((c.transform.position - hands[i].transform.position).magnitude < closeDistance)
                    {
                        closeDistance = (c.transform.position - hands[i].transform.position).magnitude;
                        mostClose= c;
                    }
                }
            }

            if(mostClose != null)
            {
                Vector3 distance = mostClose.transform.position - hands[i].transform.position;

                float angle = Mathf.Atan2(distance.y, distance.x) * Mathf.Rad2Deg;
                hands[i].transform.rotation = Quaternion.Euler(0, 0, angle);
                _weaponSwaper.GetWeapon(i).Fire(_character);
            }
        }
    }
}
