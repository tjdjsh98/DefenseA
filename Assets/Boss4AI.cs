using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.TextCore.Text;

public class Boss4AI : MonoBehaviour
{
    Character _character;
    [SerializeField] LineRenderer[] _lineRenderers;
    [SerializeField]GameObject _lightPoint;

    int _sequence = 0;
    float _sequenceTime = 0;
    float _sequenceElaspedTime;

    private void Awake()
    {
        _character = GetComponent<Character>();
    }
    void Update()
    {
        if(_sequence==0)
            _sequence = 10;
        LightAttackSequence();
    }

    void LightAttackSequence()
    {
        if (_sequence == 10)
        {
            _sequenceElaspedTime += Time.deltaTime;

            if (_sequenceElaspedTime > 3)
            {
                _sequence++;
                _sequenceElaspedTime = 0;
            }
        }
        if (_sequence == 11)
        {
            float interval = Random.Range(5,15);
            for (int i = 0; i < _lineRenderers.Length; i++)
            {
                Vector3 distance = Managers.GetManager<GameManager>().Girl.transform.position - _lightPoint.transform.position;
                float angle = Mathf.Atan2(distance.y, distance.x) + (i-_lineRenderers.Length/2)* interval * Mathf.Deg2Rad;
                _lineRenderers[i].enabled = true;
                _lineRenderers[i].SetPosition(0, _lightPoint.transform.position);
                Vector3 direction = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0);
                _lineRenderers[i].SetPosition(1, _lightPoint.transform.position + direction*100f);

            }
            _sequence++;
        }
        if (_sequence == 12)
        {
            _sequenceElaspedTime += Time.deltaTime;
            if (_sequenceElaspedTime > 2)
            {
                _sequenceElaspedTime = 0;
                _sequence++;
            }
        }
        if (_sequence == 13)
        {
            for (int i = 0; i < _lineRenderers.Length; i++)
            {
                _lineRenderers[i].enabled = false;
                RaycastHit2D[] hits = Physics2D.RaycastAll(transform.position, _lineRenderers[i].GetPosition(1) - transform.position, (_lineRenderers[i].GetPosition(1) - transform.position).magnitude,
                    Define.CharacterMask);

                if (hits.Length > 0)
                {
                    foreach (var hit in hits)
                    {
                        Character character = hit.collider.GetComponent<Character>();
                        if (character != null && !character.IsDead && character.CharacterType == Define.CharacterType.Player)
                        {
                            _character.Attack(character, _character.AttackPower, 50, character.transform.position - transform.position, hit.point, 0.1f);
                        }
                    }
                }
            }
            _sequence = 0;
        }
    }
}
