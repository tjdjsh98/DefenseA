using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlobEnemy : MonoBehaviour
{
    Character _character;

    [SerializeField] LineRenderer[] _lineRenderers = new LineRenderer[3];
    [SerializeField] Vector3[] _targetPositions = new Vector3[3];

    int _sequence = 0;
    float _elaspedTime;

    float _lightingTime = 3;

    private void Awake()
    {
        _character = GetComponent<Character>();
        foreach(var lineRenderer in _lineRenderers)
        {
            lineRenderer.enabled = false;
        }
    }

    private void Update()
    {
        if (_sequence == 0)
        {
            _elaspedTime+= Time.deltaTime;

            if (_elaspedTime > 3)
            {
                _sequence++;
                _elaspedTime = 0;
            }
        }
        if(_sequence== 1)
        {
            for(int i = 0; i < _targetPositions.Length; i++)
            {
                Vector3? topPostion = Managers.GetManager<GameManager>().GetGroundTop(transform.position + Vector3.right*(i - 1) * Random.Range(25, 40));
                if (topPostion.HasValue)
                {
                    _lineRenderers[i].enabled = true;
                    _lineRenderers[i].SetPosition(0, transform.position);
                    _lineRenderers[i].SetPosition(1, topPostion.Value);

                }
            }
            _sequence++;
        }
        if(_sequence== 2)
        {
            _elaspedTime+= Time.deltaTime;
            if (_elaspedTime > _lightingTime)
            {
                _elaspedTime = 0;
                _sequence++;
            }
        }
        if (_sequence == 3)
        {
            for (int i = 0; i < _lineRenderers.Length; i++)
            {
                _lineRenderers[i].enabled = false;
                RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, _lineRenderers[i].GetPosition(1) - transform.position, (_lineRenderers[i].GetPosition(1) - transform.position).magnitude,
                    Define.CharacterMask);

                if(hits.Length > 0)
                {
                    foreach(var hit in hits)
                    {
                        Character character = hit.collider.GetComponent<Character>();
                        if (character != null && !character.IsDead && character.CharacterType == Define.CharacterType.Player)
                        {
                            _character.Attack(character, _character.AttackPower, 50, character.transform.position-transform.position , hit.point, 0.1f);
                        }
                    }
                }
            }
            _sequence = 0;
        }
    }
}
