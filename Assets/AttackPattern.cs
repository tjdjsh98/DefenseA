using System.Collections;
using System.Collections.Generic;
using UnityEditorInternal;
using UnityEngine;

public class AttackPattern : MonoBehaviour
{
    Character _owner;

    [SerializeField]bool _debug;

    [SerializeField]bool _isStart;
    [SerializeField] Define.Range _attackRange;
    [SerializeField] float _delay;
    [SerializeField] Define.CharacterType _attackingCharacterType;
    [SerializeField] int _damage;
    [SerializeField] float _power;
    [SerializeField] Vector3 _direction;
    [SerializeField] float _stunTime;

    Coroutine _attackPatternCoroutine;

    private void OnDrawGizmos()
    {
        if (!_debug) return;

        Util.DrawRangeOnGizmos(gameObject,_attackRange,Color.red);
    }

    private void Update()
    {
        if (!_isStart) return;

        if(_attackPatternCoroutine == null)
            _attackPatternCoroutine = StartCoroutine(CorPlayerAttackPattern(_delay));

        _isStart = false;
    }

    public void StartPattern(Character owner, float delay, Define.CharacterType attackingCharacterType,
        int damage,float power, Vector3 direction,float stunTime)
    {
        _isStart = true;
        _owner = owner;
        _delay = delay;
        _damage = damage;
        _power = power;
        _direction = direction;
        _stunTime = stunTime;
        _attackingCharacterType= attackingCharacterType;
    }

    IEnumerator CorPlayerAttackPattern(float delay)
    {
        yield return new WaitForSeconds(_delay);

        GameObject[] gameObjects = Util.RangeCastAll2D(gameObject, _attackRange, Define.CharacterMask);

        foreach(var gameObject in gameObjects)
        {
            Character character= gameObject.GetComponent<Character>();
            if(character == null) continue;
            
            if(character.CharacterType == _attackingCharacterType)
            {
                if (_owner)
                    _owner.Attack(character, _damage, _power, _direction, _stunTime);
                else
                    character.Damage(_owner,_damage,_power,_direction,_stunTime);
            }
        }

        Managers.GetManager<ResourceManager>().Destroy(gameObject);
    }
}
