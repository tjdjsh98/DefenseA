using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using TreeEditor;
using UnityEngine;

public class Boar : MonoBehaviour
{
    Character _character;
    EnemyAI _enemyAI;

    [SerializeField] SpriteRenderer _face;

    int _preSequence = -1;
    int _sequence;
    float _sequenceTime;

    float _preSpeed;
    [SerializeField] float _dashSpeed = 20;

    [SerializeField] float _dashCoolTime = 5;
    float _dashCoolTimeElapsed = 0;

    [SerializeField] float _dashDelayTime = 2;
    float _dashTime;
    float _dashDuration = 2;
    Vector3 _dashDirection;

    [SerializeField] GameObject[] _injectionPositions;

    int _shellingCount;

    List<GameObject> _dashAttackList = new List<GameObject>();

    private void Awake()
    {
        
        _character = GetComponent<Character>();
        _enemyAI = GetComponent<EnemyAI>();
        
    }

    private void Update()
    {
        // 다음에 무슨 공격을 하지 결정
        if (_sequence == 0)
        {
            int random = Random.Range(0, 3);

            if (random == 0 && _preSequence == 1) random++;
            if (random == 1 && _preSequence == 10) random++;
            if (random == 2 && _preSequence == 0) random++;

            // 휴식
            if (random == 0)
            {
                _sequence = 1;
                _face.color = Color.white;
            }
            // 돌진
            else if (random == 1)
            {
                _sequence = 10;
                _face.color = Color.red;
            }
            // 포격
            if (random == 2)
            {
                _sequence = 20;
                _face.color = Color.blue;
            }
            _preSequence = _sequence;
        }

        // 휴식
        if(_sequence== 1)
        {
            _sequenceTime += Time.deltaTime;
            if (_sequenceTime > 3)
            {
                _sequenceTime = 0;
                _sequence = 0;
            }
        }
        DashSequence();
        ShellingSequence();

       
    }

    void DashSequence()
    {
        if (_sequence == 10)
        {
            if (_enemyAI.Target != null && _enemyAI.IsTargetInRange)
            {
                _sequence++;
                _dashDirection = _enemyAI.Target.transform.position - transform.position;
            }
        }
        else if (_sequence == 11)
        {
            if (_dashCoolTime > _dashCoolTimeElapsed)
            {
                _dashCoolTimeElapsed += Time.deltaTime;
            }
            else
            {
                _dashCoolTimeElapsed = 0;
                _sequence++;
            }
        }
        else if (_sequence == 12)
        {
            _character.IsAttack = true;
            _preSpeed = _character.Speed;
            _character.SetSpeed(_dashSpeed);
            _character.IsSuperArmer = true;
            _sequence++;
            _character.SetVelocity(new Vector2(_dashSpeed * transform.lossyScale.x, 0));
        }
        else if (_sequence == 13)
        {
            _dashTime += Time.deltaTime;
            _character.Move(_dashDirection);
            Util.RangeCastAll2D(gameObject, _enemyAI.AttackRange, Define.CharacterMask,
                (hit) =>
                {
                    if (_dashAttackList.Contains(hit.collider.gameObject)) return false;
                    _dashAttackList.Add(hit.collider.gameObject);
                    Character character = hit.collider.GetComponent<Character>();
                    if (character != null&&!character.IsDead && _character.CharacterType != character.CharacterType)
                    {
                        _character.Attack(character, _character.AttackPower, 100, new Vector3(_dashDirection.x > 0 ? 1 : -1, 1, 0), hit.point, 0.5f);
                    }
                    return false;
                });

        if (_dashTime >= _dashDuration)
            {
                _character.SetSpeed(_preSpeed);
                _dashTime = 0;
                _dashCoolTimeElapsed = 0;
                _character.IsAttack = false;
                _character.IsSuperArmer = false;
                _sequence = 0;
                _dashAttackList.Clear();
            }
        }
    }

    void ShellingSequence()
    {
        if (_sequence == 20)
        {
            _sequenceTime += Time.deltaTime;
            if(_sequenceTime > 2)
            {
                _sequenceTime = 0;
                _sequence++;
            }
        }
        else if (_sequence == 21)
        {
            _sequenceTime+= Time.deltaTime;
            if(_sequenceTime > 0.2f)
            {
                _sequenceTime = 0;
                _shellingCount++;
                Vector3[] position = new Vector3[4];

                position[0] = transform.position + Vector3.up*2;
                position[1] = position[0] + new Vector3(-transform.lossyScale.x * Random.Range(10, 20), Random.Range(10, 20));
                position[3] = transform.position + Vector3.right * transform.lossyScale.x * 5* _shellingCount;
                position[2] = position[3] + new Vector3(Random.Range(10,15) * -transform.lossyScale.x, Random.Range(5, 7), 0) ;

                BezierProjection bezierProjection = Managers.GetManager<ResourceManager>().Instantiate<Projectile>((int)Define.ProjectileName.Bezier) as BezierProjection;
                bezierProjection.transform.position = position[0];
                bezierProjection.Init(10, 10, _character.AttackPower, Define.CharacterType.Player, 0, 0);
                bezierProjection.SetPositions(position);
                bezierProjection.Fire(_character, Vector3.zero);

                if (_shellingCount > 10)
                {
                    _shellingCount = 0;
                    _sequence = 0;
                }
            }
        }
    }
}
