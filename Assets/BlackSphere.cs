using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class BlackSphere : MonoBehaviour
{
    Character _owner;
    Vector3 _offset;
    bool _isMoveToDestination;

    // 공격 상태일 떄
    bool _isAttackMode;
    Vector3 _attackDirection;
    Define.Range _attackRange = new Define.Range() { center = Vector3.zero, size = Vector3.one, figureType = Define.FigureType.Circle };
    float _attackTime;
    float _attackDuration= 5;
    [SerializeField]float _speed= 50;
    float _attackDelay;
    public void Init(Character owner, Vector3 offset)
    {
        _owner = owner;
        _offset = offset;
        _offset.x += Random.Range(-.5f, .5f);
        _offset.y += Random.Range(-.5f, .5f);
    }

    public void Update()
    {
        if(_owner== null) return;
        if (_isMoveToDestination) return;

        if (!_isAttackMode)
        {
            Vector3 offset = _owner.transform.position;
            offset.x += _offset.x * _owner.transform.lossyScale.x > 0 ? 1 : -1;
            offset.y += Mathf.Sin(Time.time + gameObject.GetInstanceID()) + _offset.y;
            transform.position = Vector3.Lerp(transform.position, offset, 0.01f);
        }
        else
        {
            _attackTime += Time.deltaTime;
            if (_attackDelay > _attackTime) return;

            transform.position += _attackDirection * Time.deltaTime*_speed;
            GameObject go = Util.RangeCast2D(gameObject,_attackRange,Define.CharacterMask);

            if(go!= null) 
            {
                Character character = go.GetComponent<Character>();

                if(character&& character.CharacterType == Define.CharacterType.Enemy)
                {
                    character.Damage(_owner, 5, 10, _attackDirection);

                    _owner = null;
                    _isMoveToDestination = false;
                    _isAttackMode = false;
                    Managers.GetManager<ResourceManager>().Destroy(gameObject);
                }
            }
            if (_attackTime >= _attackDuration)
            {
                _owner = null;
                _isMoveToDestination = false;
                _isAttackMode = false;
                Managers.GetManager<ResourceManager>().Destroy(gameObject);
            }

        }
    }

    public void MoveToDestinationAndDestroy(GameObject target,float duration)
    {
        _isMoveToDestination = true;

        StartCoroutine(CorMoveToDestination(target,duration, true));
    }

    public void CancelMoveToDestination()
    {
        _isMoveToDestination = false;

    }

    IEnumerator CorMoveToDestination(GameObject target,float duration, bool isDestroy = false)
    {
        duration *= 60f;
        for(int i = 0; i <= duration; i++)
        {
            transform.position = Vector3.Lerp(transform.position, target.transform.position, (float)i / duration);
            if (!_isMoveToDestination) break;
            yield return null;
        }

        if(_isMoveToDestination && isDestroy)
        {
            _owner = null;
            _isMoveToDestination = false;
            _isAttackMode = false;
            Managers.GetManager<ResourceManager>().Destroy(gameObject);
        }
    }

    public void ChangeAttackMode(Character target,float delay = 0)
    {
        _attackDelay = delay;
        _attackTime = 0;
        _isAttackMode = true;
        _attackDirection = (target.transform.position - transform.position).normalized;

    }
}
