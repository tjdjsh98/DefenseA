using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using UnityEngine;

public class Turret : MonoBehaviour
{
    [SerializeField] protected Define.Range _attackRange;

    Character _target = null;

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Define.Range range = _attackRange;
        range.center.x = transform.localScale.x > 0 ? range.center.x : -range.center.x;
        Gizmos.DrawWireCube(transform.position + range.center, range.size);
    }

    private void Update()
    {
        CheckTarget();

    }

    void CheckTarget()
    {
        List<RaycastHit2D> hits = Util.RangeCastAll2D(gameObject, _attackRange);

        GameObject closeGameObject = null;

        for (int i = 0; i < hits.Count; i++)
        {
            GameObject gameObject = hits[i].collider.gameObject;
            Character character = gameObject.GetComponent<Character>();

            if(character == null) continue;
            if (closeGameObject == null)
            {
                closeGameObject = hits[i].collider.gameObject;
            }
            if((hits[i].transform.position - transform.position).magnitude
                < (closeGameObject.transform.position - transform.position).magnitude)
            {
                closeGameObject = hits[i].collider.gameObject;
            }
        }

        if(closeGameObject != null)
        {
            _target = closeGameObject.GetComponent<Character>();
        }
    }

    void AimTarget()
    {
        Vector3 distance = _target.transform.position - transform.position;
        float angle = Mathf.Atan2(distance.y, distance.x) * Mathf.Rad2Deg;


        Vector3 mousePosition = Input.mousePosition;


    }
}