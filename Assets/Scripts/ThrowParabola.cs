using MoreMountains.FeedbacksForThirdParty;
using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using TMPro.EditorUtilities;
using UnityEditor.Rendering;
using UnityEngine;

public class ThrowParabola : MonoBehaviour
{
    [SerializeField] bool _debug;

    Character _character;
    EnemyAI _enemyAI;

    int _step = 0;

    [SerializeField] float _fireCoolTime;
    float _fireCheckTime;
    [SerializeField] float _fireDelay;
    [SerializeField] float _afterFireDelay = 1;
    [SerializeField] float _power;
    float _fireTime;

    float _gravityScale = 3;
    float _mass = 5;

    [SerializeField] LineRenderer _lineRenderer;
    [SerializeField] GameObject _firePoint;

    Vector3 _fireDirection;
    float _firePower;
    GameObject _target;

    private void Awake()
    {
        _character = GetComponent<Character>();
        _enemyAI = GetComponent<EnemyAI>();

        if (!_debug)
            _lineRenderer.gameObject.SetActive(false);
        else
            _lineRenderer.gameObject.SetActive(true);

        StartCoroutine(CorSearchTarget());
    }

    IEnumerator CorSearchTarget()
    {
        while (true)
        {
            if (_enemyAI.Target != null && _step == 0)
            {
                _character.TurnBody(_enemyAI.Target.transform.position - transform.position);
                if (_target == null)
                {
                    if (_fireCoolTime > _fireTime)
                    {
                        _fireTime += Time.deltaTime;
                    }
                    else
                    {
                        _fireCoolTime = 0;
                        for (int i = 90; i >= 45; i -= 5)
                        {
                            if (_target != null) break;

                            float angle = i;

                            if (transform.localScale.x < -0)
                                angle = 180 - angle;

                            angle *= Mathf.Deg2Rad;


                            for (float tempPower = _power - 5; tempPower <= _power + 5; tempPower++)
                            {
                                _target = PredictTrajectory(_firePoint.transform.position, new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)).normalized * tempPower);

                                if (_target != null)
                                {
                                    _character.SetAnimatorBool("Attack", true);
                                    _fireTime = 0;
                                    _character.IsAttack = true;
                                    _fireDirection = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;
                                    _firePower = tempPower;
                                    _step = 1;
                                    break;
                                }
                                yield return null;
                            }
                        }
                    }
                }

            }
            if (_step == 1)
            {
                if (_fireDelay > _fireTime)
                {
                    _fireTime += Time.deltaTime;
                }
                else
                {
                    Projectile projectile = Managers.GetManager<ResourceManager>().Instantiate<Projectile>((int)Define.ProjectileName.Parabola);

                    if (projectile)
                    {
                        projectile.transform.position = _firePoint.transform.position;
                        projectile.Init(1, _firePower, _character.AttackPower, Define.CharacterType.Player);
                        projectile.Fire(_character, _firePower, _fireDirection);
                    }
                    _step = 2;
                    _target = null;
                    _fireTime = 0;
                    _character.SetAnimatorBool("Attack", false);
                }
            }

            if (_step == 2)
            {
                if (_afterFireDelay > _fireTime)
                {
                    _fireTime += Time.deltaTime;
                }
                else
                {
                    _fireTime = 0;
                    _character.IsAttack = false;
                    _step = 0;
                }
            }

            yield return null;
        }
    }
    GameObject PredictTrajectory(Vector3 startPos, Vector3 vel)
    {
        int step = 60;
        float deltaTime = Time.fixedDeltaTime;
        Vector3 gravity = Physics.gravity * _gravityScale;

        Vector3 position = startPos;
        Vector3 velocity = vel / _mass;

        if (_debug)
        {
            _lineRenderer.positionCount = step + 1;
            _lineRenderer.SetPosition(0, position);
        }
        for (int i = 0; i < step; i++)
        {
            position += velocity * deltaTime + 0.5f * gravity * deltaTime * deltaTime;
            velocity += gravity * deltaTime;
            position += velocity * deltaTime + 0.5f * gravity * deltaTime * deltaTime;
            velocity += gravity * deltaTime;
            if(_debug)
                _lineRenderer.SetPosition(i + 1, position);


            RaycastHit2D[] hits = Physics2D.CircleCastAll(position, 0.01f, Vector2.zero, 0, LayerMask.GetMask("Character"));

            if (hits.Length > 0)
            {
                foreach (var hit in hits)
                {
                    if (hit.collider.gameObject.tag == "Player")
                    {
                        return hit.collider.gameObject;
                    }
                }
            }
        }
        return null;
    }

}
