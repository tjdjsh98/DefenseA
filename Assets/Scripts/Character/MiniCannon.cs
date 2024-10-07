using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class MiniCannon : MonoBehaviour
{
    [SerializeField] bool _debug;
    LineRenderer _lineRenderer;

    Character _girl;
    Character Girl { get
        {
            if (_girl == null)
                _girl = Managers.GetManager<GameManager>().Girl;

            return _girl;
        } }

    Character _cannon;

    [SerializeField] GameObject _firePoint;

    public float FireInterval { get; set; }
    [SerializeField] float _originFirePower;
    public int FireCount { get; set; } = 1;
    public bool IsExplosion { get; set; }
    public float ExplosionProbability = 0;
    float _firePower;
    Vector3 _fireDirection;
    GameObject _target;

    float _playerToCannonDistance = 10f;

    int _sequence = 0;
    float _sequenceTime = 0;
    private void Awake()
    {
        _cannon = GetComponent<Character>();
        _lineRenderer= GetComponentInChildren<LineRenderer>();

        StartCoroutine(CorSearchTarget());
        _cannon.AttackHandler += OnAttack;
    }

    private void OnAttack(Character target, int damage, float power, Vector3 direction, Vector3 point, float stunTime)
    {
        if (IsExplosion)
        {
            if (Random.Range(0, 100) < ExplosionProbability)
            {
                Managers.GetManager<GameManager>().Inventory.Explosion(_cannon, point, 5);
            }
        }
    }

    private void Update()
    {
        AI();
    }

    void AI()
    {
        // 플레이어 추적
        if (_sequence == 0)
        {
            _cannon.TurnBody(Girl.transform.position - transform.position);
            if (_playerToCannonDistance < Mathf.Abs(Girl.transform.position.x - transform.position.x))
            {
                _cannon.Move((Girl.transform.position - transform.position) *(Mathf.Abs(Girl.transform.position.x - transform.position.x)- _playerToCannonDistance));
            }

            if (_target && (_sequenceTime += Time.deltaTime) > FireInterval)
            {
                _sequenceTime = 0;
                _sequence++;
            }
        }

        // 발사할 적 찾음, 발사 준비
        if (_sequence == 1)
        {
            _cannon.TurnBody(_target.transform.position - transform.position);
            _cannon.IsAttack = true;
            _cannon.IsEnableMove = false;
            _cannon.IsEnableTurn= false;
            _sequence++;
        }

        // 발사
        if (_sequence == 2)
        {
            for (int i = 0; i < FireCount; i++)
            {
                Projectile parabola = Managers.GetManager<ResourceManager>().Instantiate<Projectile>((int)Define.ProjectileName.Parabola);
                parabola.transform.position = _firePoint.transform.position;
                parabola.Init(50, _firePower , _cannon.AttackPower, Define.CharacterType.Enemy, 0, 0.5f);
                parabola.Fire(_cannon, _firePower - (FireCount-1)*10 + i *10, _fireDirection );
            }
            _sequence++;
        }

        // 발사 후 잠시 대기
        if(_sequence == 3)
        {
            if ((_sequenceTime += Time.deltaTime) > FireInterval)
            {
                _sequenceTime = 0;
                _sequence = 0;
                _target = null;
                _cannon.IsAttack = false;
                _cannon.IsEnableMove = true;
                _cannon.IsEnableTurn = true;
            }
        }
        
    }

    IEnumerator CorSearchTarget()
    {
        while (true)
        {
            if (_target == null)
            {
                for (int i = 30; i < 70; i += 5)
                {
                    float angle = i;

                    if (transform.localScale.x < -0)
                        angle = 180 - angle;

                    angle *= Mathf.Deg2Rad;


                    for (float tempPower = _originFirePower + 20; tempPower >= _originFirePower - 20; tempPower -= 10)
                    {
                        _target = PredictTrajectory(transform.position, new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)).normalized * tempPower);

                        if (_target != null)
                        {
                            _firePower = tempPower;
                            _fireDirection = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;

                            break;
                        }
                        yield return null;
                    }
                }
            }
       
            yield return null;
        }
    }
    GameObject PredictTrajectory(Vector3 startPos, Vector3 vel)
    {
        int step = 180;
        float deltaTime = Time.fixedDeltaTime;
        Vector3 gravity = Physics.gravity * 3;

        Vector3 position = startPos;
        Vector3 velocity = vel / 5;

        if (_debug)
        {
            _lineRenderer.positionCount = step + 1;
            _lineRenderer.SetPosition(0, position);
        }
        for (int i = 0; i < step; i++)
        {
            position += velocity * deltaTime + 0.5f * gravity * deltaTime * deltaTime;
            velocity += gravity * deltaTime;

            if (_debug)
            {
                _lineRenderer.SetPosition(i + 1, position);
            }


            RaycastHit2D[] hits = Physics2D.CircleCastAll(position, 0.01f, Vector2.zero, 0, LayerMask.GetMask("Character"));

            if (hits.Length > 0)
            {
                foreach (var hit in hits)
                {
                    Character character = hit.collider.GetComponent<Character>();

                    if (character == null || character.IsDead) continue;
                    if(character.CharacterType == Define.CharacterType.Enemy)
                        return hit.collider.gameObject;
                    
                }
            }
        }
        return null;
    }

}
