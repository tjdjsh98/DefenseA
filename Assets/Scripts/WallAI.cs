using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallAI : MonoBehaviour
{
    Character _character;
    public Character Character => _character;
    BoxCollider2D _boxCollider;
    GameObject _model;
    GameObject _soulModel;

    [SerializeField] WallAbility _wallAbility = new WallAbility();
    public WallAbility WallAbility => _wallAbility;

    [SerializeField] Define.Range _attackRange;
    [SerializeField] GameObject _sitPosition;
    public GameObject SitPosition => _sitPosition;


    [Header("�̵��� �ϱ� ���� ���� ����")]
    [SerializeField] Define.Range _detectRange;
    [SerializeField] float _playerDistance;
    bool _isDetectEnemy;
    Coroutine _detectEnemyCoroutine;

    [SerializeField]float _originalReviveCoolTime = 30;
    float _reviveCoolTime = 30;
    float _reviveElapsedTime = 0;

    // �߰��� �ɷ�ġ
    public int ReflectionDamage { set; get; } = 0;
    public float DecreasedReviveTimePercetage { set; get; }

    bool _isSoulForm;
    [SerializeField] bool _brickTest = false;

    // ���� ȿ��
    int _brickIndex = 0;
    [SerializeField] List<GameObject> _wallBricks = new List<GameObject>();
    [SerializeField] List<Vector3> _wallBricksPosition = new List<Vector3>();
    [SerializeField] LineRenderer _lineRenderer;
    float _onewayTime;
    float _onewayElapsedTime;
    bool _isBrickAttach;

    Dictionary<SkillName, Action<SkillSlot>> _skillDictionary = new Dictionary<SkillName, Action<SkillSlot>>();

    private void Awake()
    {
        _character = GetComponent<Character>();
        _boxCollider = GetComponent<BoxCollider2D>();
        _model = transform.Find("Model").gameObject;
        _soulModel = transform.Find("SoulModel").gameObject;
        _wallAbility.Init(this);

        _character.CharacterDeadHandler += OnCharacterDead;
    }
    private void OnDrawGizmos()
    {
        Util.DrawRangeOnGizmos(gameObject, _attackRange, Color.red);
        Util.DrawRangeOnGizmos(gameObject, _detectRange, Color.green);

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

            if (_onewayElapsedTime >= _onewayTime && !_isBrickAttach)
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
        WallAbility.AbilityUpdate();

    }
    void RegistSkill()
    {

    }

   

    public void UseSkill(SkillSlot skillSlot)
    {
        if (skillSlot.skillData == null) return;

        if (_skillDictionary.TryGetValue(skillSlot.skillData.skillName, out var func))
        {
            func?.Invoke(skillSlot);
        }
    }
    private void OnCharacterDead()
    {
        _character.AnimatorSetBool("Dead", true);
        //if (AbilityUnlocks.TryGetValue(WallAbilityName.SelfDestruct,out bool value) && value)
        //{

        //    Effect effectOrigin = Managers.GetManager<DataManager>().GetData<Effect>((int)Define.EffectName.Explosion);
        //    Effect effect = Managers.GetManager<ResourceManager>().Instantiate(effectOrigin);
        //    effect.SetProperty("Radius", (ExplosionRange - 10) / 2);
        //    effect.SetProperty("BubbleCount", (int)Mathf.Pow(ExplosionRange / 5, 2));
        //    effect.transform.position = transform.position;

        //    effect.Play(_character.GetCenter());

        //    GameObject[] gameObjects = Util.RangeCastAll2D(gameObject, new Define.Range()
        //    {
        //        center = _character.GetCenter(),
        //        size = new Vector3(ExplosionRange, ExplosionRange, ExplosionRange),
        //        figureType = Define.FigureType.Circle
        //    });

        //    foreach (var gameObject in gameObjects)
        //    {
        //        Character character = gameObject.GetComponent<Character>();
        //        if (character && character.CharacterType == Define.CharacterType.Enemy)
        //        {
        //            _character.Attack(character, ExplosionDamage, ExplosionPower, character.transform.position
        //                - _character.GetCenter(), 1);
        //        }
        //    }
        //}

        _reviveCoolTime = _originalReviveCoolTime;
        _reviveCoolTime -= _wallAbility.Salvation();

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
                _brickIndex = 0;
                //if (AbilityUnlocks.TryGetValue(Define.WallAbility.SelfDestruct, out bool value) && value)

                //    transform.position = Managers.GetManager<GameManager>().Player.transform.position;
            }
        }

    }
    protected virtual void MoveForward()
    {
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
            List<RaycastHit2D> hits = Util.RangeCastAll2D(gameObject, _detectRange, Define.CharacterMask);

            foreach (var hit in hits)
            {
                if (hit.collider == null) continue;
                Character c = hit.collider.GetComponent<Character>();
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

}
