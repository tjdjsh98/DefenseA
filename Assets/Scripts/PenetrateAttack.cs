using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
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
        Gizmos.color = Color.yellow;
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

            Vector3 dest = _endPoint;
            if (transform.lossyScale.x < 0) dest.x = -dest.x;
            _lineRenderer.SetPosition(1,  _endPoint);
            GameObject[] gos = Util.BoxcastAll2D(transform.position + dest, _attackRange);

            if (gos.Length > 0)
            {
                foreach (var g in gos)
                {
                    Character c = g.GetComponent<Character>();

                    if (c != null && c.CharacterType == Define.CharacterType.Enemy)
                    {
                        c.Damage(_character, 2, 10, _direction);
                    }
                }
            }
        }

    }
    public void StartAttack(Character character, Vector3 direction, float distance)
    {
        _character = character;
        _isStart =true;
        _direction = direction;
        _distance = distance;

        _time = 0;
        _endPoint = Vector3.zero;
        _lineRenderer.SetPosition(0, Vector3.zero);
        _lineRenderer.SetPosition(1, Vector3.zero);
        gameObject.SetActive(true);
    }
}
