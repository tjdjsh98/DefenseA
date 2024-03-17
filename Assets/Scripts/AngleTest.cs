using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Matrix4x4 = UnityEngine.Matrix4x4;
using Quaternion = UnityEngine.Quaternion;

public class AngleTest : MonoBehaviour
{
    [SerializeField] Define.Range _range;


    private void OnDrawGizmos()
    {
        Util.DrawRangeOnGizmos(gameObject, _range,Color.red);
    }

    private void Update()
    {
        GameObject go = Util.RangeCast2D(gameObject, _range, Define.CharacterMask);

        if(go != null)
        {
            Debug.Log(go);
        }
    }
}
