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
  
}
