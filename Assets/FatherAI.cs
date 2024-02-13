using MoreMountains.FeedbacksForThirdParty;
using System.Collections;
using System.Collections.Generic;
using System.Data;
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

    [SerializeField]PenetrateAttack _penetrateAttack;
    float _playerDistance = 5f;

    Character _enemyToAttack;

    float _normalAttackElapsed = 0;
    float _normalAttackCoolTime = 5f;

    bool _isMove = false;

    float _spearAttackElapsed = 0;
    float _spearAttackCoolTime = 3f;

    float _shockwaveElasped = 0;
    public float ShockwaveCoolTime = 20;

    public bool IsUnlockSpear { set; get; } = false;
    [field:SerializeField]public bool IsUnlockShockwave { set; get; } = false;

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
        if (Input.GetKeyDown(KeyCode.O))
        {
            GameObject[] gos = Util.BoxcastAll2D(gameObject, _attackRangeList[(int)Define.FatherSkill.PenerstrateRange]);

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
                _penetrateAttack.StartAttack(_character, closeOne.GetCenter() - transform.position, 20);
            }
        }

        if (_player == null)
            _player = Managers.GetManager<GameManager>().Player;

        if(_normalAttackElapsed < _normalAttackCoolTime)
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
            _shockwaveElasped += Time.deltaTime;
            if (_shockwaveElasped > ShockwaveCoolTime)
            {
                StartCoroutine(CorShockwaveAttack());
                _shockwaveElasped = 0;
            }

        }
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
                if(Random.Range(0,2) == 3)
                    _character.AnimatorSetTrigger("NormalAttack");
                else
                    _character.AnimatorSetTrigger("AirBorneAttack");
                _character.IsEnableMove = false;
                _character.IsEnableTurn = false;
                _enemyToAttack = null;
            }
        }
    }

    public void NormalAttack()
    {
        GameObject[] gos = Util.BoxcastAll2D(gameObject, _attackRangeList[(int)Define.FatherSkill.NormalAttackRange], LayerMask.GetMask("Character"));

        if (gos.Length > 0)
        {
            foreach (var go in gos)
            {
                Character c = go.GetComponent<Character>();
                if (c != null && c.CharacterType == Define.CharacterType.Enemy)
                {
                    c.Damage(_character, 1, 100, c.transform.position - transform.position);
                }
            }
        }
    }
    public void AirBorneAttack()
    {
        GameObject[] gos = Util.BoxcastAll2D(gameObject, _attackRangeList[(int)Define.FatherSkill.NormalAttackRange], LayerMask.GetMask("Character"));

        if (gos.Length > 0)
        {
            foreach (var go in gos)
            {
                Character c = go.GetComponent<Character>();
                if (c != null && c.CharacterType == Define.CharacterType.Enemy)
                {
                    c.Damage(_character, 1, 100, Vector3.up,0.4f);
                }
            }
        }
    }

    public void FinishNormalAttack()
    {
        _character.IsEnableMove = true;
        _character.IsEnableTurn = true;
    }


    void FindEnemyToNormalAttack()
    {
        if(_enemyToAttack != null) return;
        GameObject[] gos = Util.BoxcastAll2D(gameObject, _attackRangeList[(int)Define.FatherSkill.FindingRange], LayerMask.GetMask("Character"));

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

        GameObject[] gos = Util.BoxcastAll2D(gameObject, _attackRangeList[2]);

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

    IEnumerator CorShockwaveAttack()
    {
        List<GameObject> characterList = new List<GameObject>();
        characterList.Clear();
        Vector3 center = transform.position;
        _tc = center;
        float radius = 0;
        Camera.main.GetComponent<CameraController>().ShockWave(center,30);
        while (radius < 30) {
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

        Camera.main.GetComponent<CameraController>().StopShockwave();
    }
}
