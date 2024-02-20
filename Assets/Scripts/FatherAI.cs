using MoreMountains.FeedbacksForThirdParty;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Unity.Cinemachine;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.Text;


public class FatherAI : MonoBehaviour
{
    Character _character;
    Player _player;

    /*
     * 0 = 일반 공격 적 인식 범위
     * 1 = 일반공격
     * 2 = 관통공격
     * 3 = 바닥에서 치솟는 스피어 공격
     */
    [SerializeField] int _debugAttackRangeIndex;
    [SerializeField] List<Define.Range> _attackRangeList = new List<Define.Range>();

    float _playerDistance = 5f;
    Character _enemyToAttack;
    bool _isMove = false;

    // 관통공격
    [Header("관통공격")]
    [SerializeField]PenetrateAttack _penetrateAttack;
    [SerializeField] GameObject _fArmIK;
    Character _closeOne;


    // 일반공격 변수
    int _normalAttackDamage = 1;
    int NormalAttackDamage => _normalAttackDamage + IncreasedNormalAttackDamage;
    float _normalAttackElapsed = 0;
    float _normalAttackCoolTime = 5f;
    float NormalAttackCoolTime => DecreasedNormalAttackCoolTimePercentage > 0? _normalAttackCoolTime / (1 + (DecreasedNormalAttackCoolTimePercentage / 100)):  _normalAttackCoolTime * (1- (DecreasedNormalAttackCoolTimePercentage / 100));
    public int IncreasedNormalAttackDamage{ set; get; }
    public float IncreasedNormalAttackSpeedPercentage { set; get; }
    public float DecreasedNormalAttackCoolTimePercentage { set; get; }


    // 스피어 공격 변수
    [field:SerializeField]public bool IsUnlockSpear { set; get; } = false;
    float _spearAttackElapsed = 0;
    [SerializeField]float _spearAttackCoolTime = 3f;

    // 쇼크웨이브 공격 변수
    float _shockwaveElasped = 0;
    float _shockwaveCoolTime = 20;
    public float ShockwaveCoolTime => DecreasedShockwaveCoolTimePercentage > 0 ? _shockwaveCoolTime / (1 + (DecreasedShockwaveCoolTimePercentage/100)) : _shockwaveCoolTime*(1- (DecreasedShockwaveCoolTimePercentage / 100));
    public float DecreasedShockwaveCoolTimePercentage { set; get; }
    public int ShockwaveHitCount { set; get; } = 1;
    public float ShockwaveRange { set; get; } = 20;
    [field:SerializeField]public bool IsUnlockShockwave { set; get; } = false;

    // 반경 테스트 변수
    Vector3 _tc;
    float _tr;

    private void Awake()
    {
        _character = GetComponent<Character>();
        Managers.GetManager<GameManager>().FatherAI = this;
        if(_attackRangeList.Count < Define.FatherSkillCount)
        {
            for(int i = _attackRangeList.Count; i < Define.FatherSkillCount;i++) 
                _attackRangeList.Add(new Define.Range());
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        if (_attackRangeList.Count <= _debugAttackRangeIndex) return;

        Define.Range range = _attackRangeList[_debugAttackRangeIndex];
        range.center.x = transform.lossyScale.x > 0 ? range.center.x : - range.center.x;

        if(range.figureType == Define.FigureType.Box)
            Gizmos.DrawWireCube(transform.position + range.center, range.size);
        else if (range.figureType == Define.FigureType.Circle)
            Gizmos.DrawWireSphere(transform.position + range.center, range.size.x);
        Gizmos.DrawWireSphere(_tc, _tr);
    }

    private void Update()
    {
        if (_character.IsAttack) return;

        if (Input.GetKeyDown(KeyCode.O))
        {
           
            GameObject[] gos = Util.RangeCastAll2D(gameObject, _attackRangeList[(int)Define.FatherSkill.PenerstrateRange]);

            Character closeOne = null;
            float distance = 100;
            if (gos.Length > 0)
            {
                foreach (var go in gos)
                {
                    Character c = go.GetComponent<Character>();
                    if (c != null && c.CharacterType == Define.CharacterType.Enemy)
                    {
                        if ((transform.position - c.GetCenter()).magnitude < distance)
                        {
                            closeOne = c;
                            distance = (transform.position - c.GetCenter()).magnitude;
                        }
                    }
                }
            }

            if (closeOne != null) {
                _closeOne = closeOne;
                _character.TurnBody(closeOne.transform.position - transform.position);
                _character.IsAttack = true;
                _character.AnimatorSetTrigger("PenerstrateAttack");

            }
        }

        if (_player == null)
            _player = Managers.GetManager<GameManager>().Player;

        if(_normalAttackElapsed < NormalAttackCoolTime)
        {
            _normalAttackElapsed += Time.deltaTime;
            FollwerPlayer();
        }
        else
        {
            FindEnemyToNormalAttack();
            if (_enemyToAttack == null)
            {
                FollwerPlayer();
            }
            else
            {
                PlayNormalAttack();
            }
        }

        if (IsUnlockSpear)
        {
            _spearAttackElapsed += Time.deltaTime;
            if (_spearAttackElapsed > _spearAttackCoolTime)
            {
                SpearAttack();
                _spearAttackElapsed = 0;
            }
        }
        if (IsUnlockShockwave)
        {
            if (_shockwaveElasped > ShockwaveCoolTime)
            {
                if (ShockwaveHitCount >= 1)
                    StartCoroutine(CorShockwaveAttack());
                if (ShockwaveHitCount >= 2)
                    StartCoroutine(CorShockwaveAttack(0.2f,1));
                _shockwaveElasped = 0;
            }else
            {
                _shockwaveElasped += Time.deltaTime;
            }

        }
    }

    public void AimFrontArmToEnemy()
    {
        Animation anim = GetComponent<Animation>();
        AnimationCurve curve;

        // create a new AnimationClip
        AnimationClip clip = new AnimationClip();
        clip.legacy = true;

        // create a curve to move the GameObject and assign to the clip
        Keyframe[] keys;
        keys = new Keyframe[3];
        keys[0] = new Keyframe(0.0f, 0.0f);
        keys[1] = new Keyframe(1.0f, 1.5f);
        keys[2] = new Keyframe(2.0f, 0.0f);
        curve = new AnimationCurve(keys);
        clip.SetCurve("", typeof(Transform), "localPosition.x", curve);

        // update the clip to a change the red color
        curve = AnimationCurve.Linear(0.0f, 1.0f, 2.0f, 0.0f);
        clip.SetCurve("", typeof(Material), "_Color.r", curve);
        _fArmIK.transform.position = _closeOne.transform.position;
    }

    public void StartPenerstrateAttack()
    {
        _penetrateAttack.StartAttack(_character, _closeOne.GetCenter() - _penetrateAttack.transform.position, 20);

    }
    void FollwerPlayer()
    {
        if(_player ==null) return;

        float distacne = _player.transform.position.x - transform.position.x;
        if (Mathf.Abs(distacne) > _playerDistance)
        {
            _character.Move(Vector3.right * (distacne + (distacne > 0? -_playerDistance : _playerDistance))/(_playerDistance));
        }

    }

    void PlayNormalAttack()
    {
        if(_enemyToAttack)
        {
            Define.Range range = _attackRangeList[(int)Define.FatherSkill.NormalAttackRange];
            range.center.x = transform.lossyScale.x > 0 ? range.center.x : -range.center.x;
            float distance = ((Vector2)_enemyToAttack.transform.position - ((Vector2)range.center + (Vector2)transform.position)).magnitude;

            if(distance > Mathf.Abs(range.center.x) + range.size.x/2)
            {
                _character.Move(Vector3.right * (_enemyToAttack.transform.position.x - transform.position.x));
            }
            else
            {
                _character.TurnBody(_enemyToAttack.transform.position - transform.position);
                _normalAttackElapsed = 0;

                if(Random.Range(0,2) == 0) 
                    _character.AnimatorSetTrigger("NormalAttack");
                else
                    _character.AnimatorSetTrigger("AirBorneAttack");

                _character.SetAnimationSpeed(IncreasedNormalAttackSpeedPercentage > 0?1 + IncreasedNormalAttackSpeedPercentage / 100 : 1f/(1-IncreasedNormalAttackSpeedPercentage/100));
                _character.IsEnableMove = false;
                _character.IsEnableTurn = false;
                _enemyToAttack = null;
            }
        }
    }

    public void NormalAttack()
    {
        GameObject[] gos = Util.RangeCastAll2D(gameObject, _attackRangeList[(int)Define.FatherSkill.NormalAttackRange], LayerMask.GetMask("Character"));

        if (gos.Length > 0)
        {
            foreach (var go in gos)
            {
                Character c = go.GetComponent<Character>();
                if (c != null && c.CharacterType == Define.CharacterType.Enemy)
                {
                    c.Damage(_character, NormalAttackDamage, 100, c.transform.position - transform.position);
                }
            }
        }
    }
    public void AirBorneAttack()
    {
        GameObject[] gos = Util.RangeCastAll2D(gameObject, _attackRangeList[(int)Define.FatherSkill.NormalAttackRange], LayerMask.GetMask("Character"));

        if (gos.Length > 0)
        {
            foreach (var go in gos)
            {
                Character c = go.GetComponent<Character>();
                if (c != null && c.CharacterType == Define.CharacterType.Enemy)
                {
                    c.Damage(_character, NormalAttackDamage, 100, Vector3.up,0.4f);
                }
            }
        }
    }

    public void FinishNormalAttack()
    {
        _character.IsEnableMove = true;
        _character.IsEnableTurn = true;
        _character.SetAnimationSpeed(1);
    }


    void FindEnemyToNormalAttack()
    {
        if(_enemyToAttack != null) return;
        GameObject[] gos = Util.RangeCastAll2D(gameObject, _attackRangeList[(int)Define.FatherSkill.FindingRange], LayerMask.GetMask("Character"));

        if (gos.Length > 0)
        {
            Character closeOne = null;
            float distance = 1000;
            foreach (var go in gos)
            {
                Character c = go.GetComponent<Character>();
                if (c != null && c.CharacterType == Define.CharacterType.Enemy)
                {
                    if ((c.transform.position - transform.position).magnitude < distance)
                    {
                        closeOne = c;
                        distance = (c.transform.position - transform.position).magnitude;
                    }
                }
            }
            _enemyToAttack = closeOne;
        }
    }

    void SpearAttack()
    {
        if (!IsUnlockSpear) return;

        GameObject[] gos = Util.RangeCastAll2D(gameObject, _attackRangeList[2]);

        if(gos.Length > 0) 
        {
            _spearAttackElapsed = 0;
            foreach (var go in gos)
            {
                Character c = go.GetComponent<Character>();
                if (c != null && c.CharacterType == Define.CharacterType.Enemy)
                {
                    GameObject spear = Managers.GetManager<ResourceManager>().Instantiate("Spear");
                    spear.transform.position = c.transform.position;
                    spear.GetComponent<AttackObject>().StartAttack(_character, 10);
                    return;
                }
            }
        }
    }

    IEnumerator CorShockwaveAttack(float later = 0, int num = 0)
    {
        if(later != 0)
            yield return new WaitForSeconds(later);
        List<GameObject> characterList = new List<GameObject>();
        characterList.Clear();
        Vector3 center = transform.position;
        _tc = center;
        float radius = 0;
        Camera.main.GetComponent<CameraController>().ShockWave(center,30,num);
        while (radius < ShockwaveRange) {
            radius += Time.deltaTime*30;
            _tr = radius;
            RaycastHit2D[] hits = Physics2D.CircleCastAll(center, radius, Vector2.zero, 0);

            if (hits.Length > 0)
            {
                foreach (var hit in hits)
                {
                    if (characterList.Contains(hit.collider.gameObject)) continue;

                    characterList.Add(hit.collider.gameObject);

                    Character character = hit.collider.gameObject.GetComponent<Character>();
                    if (character != null && character.CharacterType == Define.CharacterType.Enemy)
                    {
                        character.Damage(_character, 1, 10, character.transform.position - center);
                    }
                }
            }
            yield return null;
        }

        Camera.main.GetComponent<CameraController>().StopShockwave(num);
    }
}
