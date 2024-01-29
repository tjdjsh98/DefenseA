using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingEnemy : EnemyAI
{
    [SerializeField] float _flyHeight = 1f;
    protected override void Awake()
    {
        base.Awake();
    }
  
    public bool CheckGround()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, _flyHeight, LayerMask.GetMask("Ground"));

        return hit.collider != null;
    }
}
