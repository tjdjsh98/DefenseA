using System.Collections;
using UnityEngine;
using static DuloGames.UI.UITooltipLines;
using UnityEngine.Animations.Rigging;

public class Boss3AI : MonoBehaviour
{
    Character _character;
    EnemyAI _enemyAI;
    Player _player;
    Rigidbody2D _rigidbody;
    LineRenderer _lineRenderer;

    [SerializeField] bool _debug;
    [SerializeField] Define.Range _meleeAttackRange;
    GameObject _attackTarget;

    SpriteRenderer _attackRangeSprite;

    [SerializeField]int _sequence = 0;
    [SerializeField] int _attackSequence = 0;
    float _sequenceTime;

    Coroutine _predictJumpPathCoroutine;
    Coroutine _predictCoroutine;

    [SerializeField]float _jumpPowerValue = 120;
    Vector3 _jumpDirection;
    float _jumpPower;

    private void Awake()
    {
        _character = GetComponent<Character>();
        _enemyAI = GetComponent<EnemyAI>();
        _player = Managers.GetManager<GameManager>().Player;
        _rigidbody = GetComponent<Rigidbody2D>();   
        _lineRenderer = GetComponentInChildren<LineRenderer>();
        if (transform.Find("AttackRange"))
        {
            _attackRangeSprite = transform.Find("AttackRange").GetComponent<SpriteRenderer>();
        }
        else
        {
            _attackRangeSprite = Managers.GetManager<ResourceManager>().Instantiate("AttackRange").GetComponent<SpriteRenderer>();
            _attackRangeSprite.transform.SetParent(transform);
        }
        _attackRangeSprite.transform.localPosition = _meleeAttackRange.center;
        _attackRangeSprite.transform.localScale = _meleeAttackRange.size;

    }
    private void OnDrawGizmosSelected()
    {
        if (!_debug) return;

        Util.DrawRangeOnGizmos(gameObject, _meleeAttackRange, Color.red);
    }

    private void Update()
    {
        // 적을 찾아서 이동
        if (_sequence == 0)
        {
            GameObject target = null;
            _sequenceTime += Time.deltaTime;
            if(_sequenceTime > 2.5f)
            {
                if (Mathf.Abs(_player.transform.position.x - transform.position.x) > 20)
                {
                    _sequenceTime = 0;
                    _sequence = 20;
                    return;
                }
            }
            if (_enemyAI.Target == null)
            {
                if (_player == null)
                {
                    _player = Managers.GetManager<GameManager>().Player;
                }
                target = _player.gameObject;
            }
            else
            {
                target = _enemyAI.Target.gameObject;
            }
            _character.Move(target.transform.position - transform.position);

            Util.RangeCastAll2D(gameObject, _meleeAttackRange, Define.CharacterMask, (hit) =>
            {
                if (_attackTarget) return false;

                Character character = hit.collider.GetComponent<Character>();
                if (character != null && character.CharacterType == Define.CharacterType.Player && !character.IsDead)
                {
                    _sequence = 10;
                    _sequenceTime = 0;
                    _attackTarget = character.gameObject;
                    return false;
                }
                return false;
            });
        }

        MeleeAttackSequence();
        JumpToPlayerSequence();
    }

    #region 점프 공격
    void JumpToPlayerSequence()
    {
        if (_sequence == 20)
        {
            _sequenceTime += Time.deltaTime;
            if(_sequenceTime > 1)
            {
                _sequenceTime = 0;
                _sequence++;
            }
        }
        if (_sequence == 21)
        {
            _character.IsSuperArmer = true;
            _character.TurnBody(_player.transform.position - transform.position);
            _predictJumpPathCoroutine = StartCoroutine(CorPredictJumpPath());
            _sequence++;
          
        }
        if (_sequence == 22)
        {
            if (_predictJumpPathCoroutine == null)
            {
                if (_attackTarget)
                    _sequence++;
                else
                    _sequence = 25;
            }
        }

        if (_sequence == 23)
        {
            _character.IsIgnoreBreak = true;
            _character.Jump(_jumpDirection , _jumpPower);
            _sequence++;
        }
        if (_sequence == 24)
        {
            _sequenceTime += Time.deltaTime;
            if(_character.IsContactGround && _sequenceTime > 1)
            {
                _sequenceTime = 0;
                _sequence++;

                _character.IsIgnoreBreak = false;
            }
        }
        if (_sequence == 25)
        {
            _sequenceTime+= Time.deltaTime;
            if (_sequenceTime > 2)
            {
                _sequenceTime = 0;
                _sequence = 0;
                _attackTarget = null;
                _character.IsSuperArmer = false;
            }
        }
    }

    IEnumerator CorPredictJumpPath()
    {
        for (int i = 40; i <= 90; i += 10)
        {
            if (_attackTarget != null) break;

            float angle = i;

            if (transform.localScale.x < -0)
                angle = 180 - angle;

            angle *= Mathf.Deg2Rad;


            for (float tempPower = _jumpPowerValue - 20; tempPower <= _jumpPowerValue + 20; tempPower += 5)
            {
                if (_attackTarget != null) break;

                _jumpDirection = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle));
                yield return StartCoroutine(CorPredictTrajectory(transform.position, _jumpDirection*tempPower));

                if (_attackTarget != null)
                {
                    _jumpPower = tempPower;
                    _character.IsAttack = true;
                    _jumpPower = tempPower;
                }
                yield return null;
            }
        }

        _predictJumpPathCoroutine = null;
    }


    IEnumerator CorPredictTrajectory(Vector3 startPos, Vector3 vel)
    {
        int maxStep = 60;
        float deltaTime = Time.fixedDeltaTime;
        Vector3 gravity = Physics.gravity * _rigidbody.gravityScale;

        Vector3 position = startPos;
        Vector3 velocity = vel/_rigidbody.mass;

        if (_debug)
        {
            _lineRenderer.positionCount = maxStep + 1;
            _lineRenderer.SetPosition(0, position);
        }

        for (int i = 0; i < maxStep; i++)
        {
            Vector3 prePosition = position;

            for (int j = 0; j < 5; j++)
            {
                position += velocity * deltaTime + 0.5f * gravity * deltaTime * deltaTime;
                velocity += gravity * deltaTime;
            }
            if (_debug)
                _lineRenderer.SetPosition(i + 1, position);

            RaycastHit2D[] hits = Physics2D.RaycastAll(prePosition, (prePosition - position).normalized, (prePosition - position).magnitude, Define.CharacterMask);

            if (hits.Length > 0)
            {
                foreach (var hit in hits)
                {
                    if (hit.collider.gameObject == _player.gameObject)
                    {
                        _attackTarget =  hit.collider.gameObject;
                    }
                }
            }
            if (_attackTarget != null) break;
        }
            yield return null;
    }
    #endregion

    #region 근접 공격
    void MeleeAttackSequence()
    {
        // 공격 범위 내 적을 찾았다면 공격 준비
        if (_sequence == 10)
        {
            _character.IsSuperArmer = true;
            _character.IsAttack = true;
            _character.SetAnimatorTrigger("AttackReady");
            _sequence++;
        }
        if (_sequence == 11)
        {
            if (_attackSequence == 1)
            {
                _character.TurnBody(_attackTarget.transform.position - transform.position);
                _sequence++;
            }
        }
        if (_sequence == 12)
        {
            if (_attackSequence == 2)
            {
                _character.TurnBody(_attackTarget.transform.position - transform.position);
                _sequence++;
            }
        }
        if (_sequence == 13)
        {
            if (_attackSequence == 3)
            {
                _character.TurnBody(_attackTarget.transform.position - transform.position);
                _sequence++;
            }
        }
        if (_sequence == 14)
        {
            if (_attackSequence == 0)
            {
                _sequenceTime += Time.deltaTime;
                if (_sequenceTime > 3)
                {
                    _sequenceTime = 0;
                    _sequence = 0;
                    _attackTarget = null;
                    _character.IsSuperArmer = false;
                }
            }
        }
    }

    public void MoveToTarget(float time)
    {
        StartCoroutine(CorMoveToTarget(time));
    }
    IEnumerator CorMoveToTarget(float time)
    {
        if (_attackTarget != null)
        {
            float elapsed = 0;
            Vector3 initPostion = transform.position;
            Vector3 destination = _attackTarget.transform.position - _meleeAttackRange.center;
            if (Mathf.Abs(initPostion.x - destination.x) > 15)
            {
                destination = transform.position + Vector3.right* transform.localScale.x * 15;
            }
            destination.y = transform.position.y;

            while (elapsed <= time)
            {
                elapsed += Time.deltaTime;
                transform.position = Vector3.Lerp(initPostion, destination, elapsed / time);
                yield return null;
            }
        }

    }

    public void MeleeAttack()
    {
        Debug.Log("atack");
        Util.RangeCastAll2D(gameObject, _meleeAttackRange, Define.CharacterMask, (hit) =>
        {
            Character character = hit.collider.GetComponent<Character>();
            if (character != null && character.CharacterType == Define.CharacterType.Player && !character.IsDead)
            {
                _character.Attack(character, _character.AttackPower, 100, Vector3.right * transform.localScale.x, hit.point, 0.3f);
            }
            return false;
        });
    }

    public void StartShowAttackRange(float time)
    {
        StartCoroutine(CorShowAttackRange(time));
    }
    IEnumerator CorShowAttackRange(float time)
    {
        float elapsedTime = 0;
        while (true)
        {
            elapsedTime += Time.deltaTime;
            if (time < elapsedTime)
            {
                break;
            }
            _attackRangeSprite.color = new Color(1, 0, 0, elapsedTime/time);
            yield return null;
        }
        Debug.Log(elapsedTime);
        _attackRangeSprite.color = new Color(1, 0, 0, 0);

    }
    public void NextAttackSequence()
    {
        _attackSequence++;
        _character.SetAnimatorInt("AttackSequence",_attackSequence);
    }
    public void ResetAttackSequence()
    {
        _attackSequence = 0;
        _character.SetAnimatorInt("AttackSequence",_attackSequence);
    }
    #endregion
}
