using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeBead : MonoBehaviour
{
    Character _attacker;

    [SerializeField] bool _debug;
    [SerializeField] Define.Range _attackRange;
    [SerializeField] Define.Range _attractionaRange;

    GameObject _model;

    float _duration = 3;
    float _elasped = 0f;
    float _attackCoolTime = .5f;
    float _attackElapsed = 0;
    int _attackPower = 30;

    bool _isAttraction = false;

    private void Awake()
    {
        _model = transform.Find("Model").gameObject;
    }
    private void OnDrawGizmosSelected()
    {
        if (!_debug) return;

        Util.DrawRangeOnGizmos(gameObject, _attackRange, Color.red);
    }

    private void Update()
    {
        if (_elasped < _duration)
        {
            _elasped += Time.deltaTime;
            if ((_attackElapsed += Time.deltaTime) > _attackCoolTime)
            {
                _attackElapsed = 0;
                Util.RangeCastAll2D(gameObject, _attackRange, Define.CharacterMask, hit =>
                {
                    Character character = hit.collider.GetComponent<Character>();
                    if (character != null && !character.IsDead && character.CharacterType == Define.CharacterType.Enemy)
                    {
                        _attacker.AddtionalAttack(character, _attackPower, 0, Vector3.zero, hit.point, 0);
                    }
                    return false;
                });
            }


            if (_isAttraction)
            {
                Util.RangeCastAll2D(gameObject, _attractionaRange, Define.CharacterMask, hit =>
                {
                    Character character = hit.collider.GetComponent<Character>();
                    if (character != null && !character.IsDead && character.CharacterType == Define.CharacterType.Enemy)
                    {
                        character.SetVelocity(transform.position - character.transform.position);
                    }
                    return false;
                });
            }
        }
        else
        {
            _elasped = 0;
            Managers.GetManager<ResourceManager>().Destroy(gameObject);
        }
    }

    public void Play(Character attacker, Vector3 position,float radius, bool isAttraction = false)
    {
        _attackRange.size.x = radius;
        _model.transform.localScale = Vector3.one * radius;
        _attacker = attacker;
        _attackElapsed = 0;
        _elasped = 0;
        _isAttraction = isAttraction;
        transform.position = position;
    }

}
