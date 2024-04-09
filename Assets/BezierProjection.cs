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
            if (_injectTime < 5)
            {
                _injectTime += Time.deltaTime;
                transform.position = BezierCurve(_positions, _injectTime / 5);
            }
            else
            {
                Managers.GetManager<ResourceManager>().Destroy(gameObject);
                _isAttack = false;
            }
            base.Update();
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
    }
}
