using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackSphere : MonoBehaviour
{
    Character _owner;
    MMFollowTarget _mTarget;
    Vector3 _offset;

    private void Awake()
    {
        _mTarget = GetComponent<MMFollowTarget>();
    }

    public void Init(Character owner)
    {
        _owner = owner;
        if (_mTarget)
        {
            _mTarget.ChangeFollowTarget(owner.transform);
            _offset = new Vector3(Random.Range(-4f, -3f), Random.Range(3f, 5f),0);
        }
    }

    public void Update()
    {
        if(_mTarget && _owner)
        {
            Vector3 offset = _offset;
            offset.x *= _owner.transform.localScale.x < 0 ? -1 : 1;
            _mTarget.Offset = offset;
        }
    }
}
