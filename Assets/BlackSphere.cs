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

        Vector3 offset = _owner.transform.position;
        offset.x += _offset.x * _owner.transform.lossyScale.x > 0 ? 1 : -1;
        offset.y += Mathf.Sin(Time.time + gameObject.GetInstanceID()) + _offset.y;
        transform.position = Vector3.Lerp(transform.position, offset, 0.01f);
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
            Managers.GetManager<ResourceManager>().Destroy(gameObject);
        }
    }
}
