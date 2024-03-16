using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowParabola : MonoBehaviour
{
    Character _character;
    EnemyAI _enemyAI;

    [SerializeField]float _fireCoolTime;
    [SerializeField]float _fireDelay;
    [SerializeField] float _power;
    float _fireTime;

    float _gravityScale=1;
    float _mass=1;

    [SerializeField]LineRenderer _lineRenderer;
    [SerializeField] GameObject _firePoint;

    Vector3 _fireDirection;
    float _firePower;
    GameObject _target;

    private void Awake()
    {
        _character = GetComponent<Character>();
        _enemyAI = GetComponent<EnemyAI>();
    }

    private void Update()
    {
        if (_target == null)
        {
            if (_fireCoolTime > _fireTime)
            {
                _fireTime += Time.deltaTime;
            }
            else
            {
                for (int i = 90; i >= 45; i -= 5)
                {
                    float angle = i;

                    if (transform.localScale.x < -0)
                        angle = 180 - angle;

                    angle *= Mathf.Deg2Rad;


                    for (float tempPower = _power -5; tempPower <= _power + 5 ; tempPower++)
                    {
                        _target = PredictTrajectory(_firePoint.transform.position, new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)).normalized * tempPower);

                        if (_target != null)
                        {
                            _fireTime = 0;
                            _character.IsAttack = true;
                            _fireDirection = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;
                            _firePower = tempPower;
                            return;
                        }
                    }
                }
            }
        }
        else
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
                    projectile.Init(1, _firePower, 1, Define.CharacterType.Player);
                    projectile.Fire(_character, _fireDirection);
                }

                _target = null;
                _fireTime = 0;
            }
        }

    }

    GameObject PredictTrajectory(Vector3 startPos, Vector3 vel)
    {
        int step = 180;
        float deltaTime = Time.fixedDeltaTime;
        Vector3 gravity = Physics.gravity * _gravityScale;

        Vector3 position = startPos;
        Vector3 velocity = vel / _mass;

        _lineRenderer.positionCount = step + 1;
        _lineRenderer.SetPosition(0, position);
        for (int i = 0; i < step; i++)
        {
            position += velocity * deltaTime + 0.5f * gravity * deltaTime * deltaTime;
            velocity += gravity * deltaTime;
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
