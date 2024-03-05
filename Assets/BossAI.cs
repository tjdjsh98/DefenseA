using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossAI : MonoBehaviour
{
    Character _character;
    [SerializeField] bool _stop;

    float _attackInterval = 5;
    float _idleElapsed = 0;

    Coroutine _attackPatternCoroutine;



    private void Awake()
    {
        _character = GetComponent<Character>();
    }
    void Update()
    {
        if (_stop) return;
        if (_attackPatternCoroutine != null) return;

        _idleElapsed+= Time.deltaTime;
        if(_idleElapsed > _attackInterval)
        {
            _idleElapsed = 0;
            _attackPatternCoroutine = StartCoroutine(AttackPattern1());
        }
    }

    IEnumerator AttackPattern1()
    {
        Vector3 postion = transform.position;

        for (int i = 0; i < 3; i++)
        {
            postion.x -= 10;
            postion.y = 10;

            RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 100, LayerMask.GetMask("Ground"));

            if (hit)
            {
                postion.y = hit.point.y;
                AttackPattern attackPattern = Managers.GetManager<ResourceManager>().Instantiate("Prefabs/AttackPattern/PatternAttack1").GetComponent<AttackPattern>();

                attackPattern.transform.position = postion;
                attackPattern.StartPattern(_character, 2, Define.CharacterType.Player, 1, 1, Vector3.up, 1);
            }
        }

        yield return null;
    }
}
