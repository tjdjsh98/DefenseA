using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class FireEnemyAI : EnemyAI
{
    [SerializeField] Projectile _parabola;

    [SerializeField]Vector3 _defaultFireDirection = new Vector3(-0.3f, 0.7f, 0f);
    [SerializeField] float _power = 10;

    protected override void CheckTargetInAttackRange()
    {
        GameObject go = PredictTrajectory(transform.position, _defaultFireDirection.normalized * _power);

        if(go != null)
        {
            _target = go.GetComponent<Character>();
        }
    }

    GameObject PredictTrajectory(Vector3 startPos, Vector3 vel)
    {
        int step = 180;
        float deltaTime = Time.fixedDeltaTime;
        Vector3 gravity = Physics.gravity;

        Vector3 position = startPos;
        Vector3 velocity = vel*0.95f;

        for (int i = 0; i < step; i++)
        {
            position += velocity * deltaTime + 0.5f * gravity * deltaTime * deltaTime;
            velocity += gravity * deltaTime;

            RaycastHit2D hit = Physics2D.CircleCast(position, 0.01f, Vector2.zero, 0, LayerMask.GetMask("Building"));

            if (hit.collider != null) return hit.collider.gameObject;
        }

        return null;
    }

    protected override void PlayAttack()
    {
        if (_attackElapsed < _attackDelay)
        {
            _attackElapsed += Time.deltaTime;
        }
        else
        {
            if (_target == null) return;

            _attackElapsed = 0;
            Projectile projectile = Instantiate(_parabola);
            projectile.transform.position = transform.position;
            projectile.Init(1, _power, 1,Define.CharacterType.Building);
            projectile.Fire(_character, _defaultFireDirection * _power);
        }
    }
}

