using TMPro;
using UnityEngine;

public class BezierProjection : Projectile
{
    Vector3[] _positions = new Vector3[4];

    float _injectTime = 0;

    protected override void Update()
    {
        if (_isAttack)
        {
            _injectTime += Time.deltaTime;
            if (_injectTime < 5 && !_isTouchGround)
            {
                transform.position = BezierCurve(_positions, _injectTime / 5);
                RotateBody();
                CheckCollision();
            }
            else if (_injectTime > 5)
            {
                Managers.GetManager<ResourceManager>().Destroy(gameObject);
                _isAttack = false;
                
            }
        }
    }

    protected override void RotateBody()
    {
        if (_injectTime < 3)
        {
            float angle = Mathf.Atan2((_prePostion - transform.position).y, (_prePostion - transform.position).x);
            transform.rotation = Quaternion.Euler(0, 0, angle * Mathf.Rad2Deg);
            _boxSize.angle = angle * Mathf.Rad2Deg;
        }
    }
    Vector3 BezierCurve(Vector3[] vectors , float ratio)
    {
        for (int count = 0; count < vectors.Length -1; count++)
        {
            for (int i = 0; i < vectors.Length -1 - count; i++)
            {
                vectors[i] = Vector3.Lerp(vectors[i], vectors[i+1], ratio);
            }
        }

        return vectors[0];
    }
    public void SetPositions(Vector3[] positions)
    {
        _positions = positions;
    }


    public override void Fire(Character attacker, Vector3 direction)
    {
        _prePostion = transform.position;
        _isAttack = true;
        _attacker = attacker;
        _injectTime = 0;
        _isTouchGround= false;
    }
}
