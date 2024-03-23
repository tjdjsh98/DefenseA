using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class PenetrateAttack : MonoBehaviour
{
    Character _character;
    LineRenderer _lineRenderer;
    bool _isStart;
    float _distance;
    [SerializeField]float _speed = 50;
    Vector3 _direction;
    Vector3 _endPoint;

    [SerializeField]Define.Range _attackRange;

    float _time;

    private void Awake()
    {
        _lineRenderer= GetComponent<LineRenderer>();
        gameObject.SetActive(false);
    }

    private void OnDrawGizmosSelected()
    {
        Util.DrawRangeOnGizmos(gameObject, _attackRange,Color.yellow);
    }
    private void Update()
    {
        if(_isStart)
        {
            _endPoint += _speed * _direction.normalized * Time.deltaTime;
            _time += Time.deltaTime;

            if(_time > _distance/_speed)
            {
                _isStart = false;
                gameObject.SetActive(false);
            }

            _attackRange.center = _endPoint;
            Vector3 dest = _endPoint;

            _lineRenderer.SetPosition(1,  _endPoint);
            List<RaycastHit2D> hits = Util.RangeCastAll2D(gameObject, _attackRange);

            foreach (var hit in hits)
            {
                Character c = hit.collider.GetComponent<Character>();

                if (c != null && c.CharacterType == Define.CharacterType.Enemy)
                {
                    _character.Attack(c, 2, 10, _direction, hit.point);
                }
            }
        }

    }
    public void StartAttack(Character character, Vector3 direction, float distance)
    {
        _character = character;
        _isStart =true;
        direction.z = 0;
        _direction = direction;
        _distance = distance;

        _time = 0;
        _endPoint = Vector3.zero;
        _lineRenderer.SetPosition(0, Vector3.zero);
        _lineRenderer.SetPosition(1, Vector3.zero);
        gameObject.SetActive(true);
    }
}
