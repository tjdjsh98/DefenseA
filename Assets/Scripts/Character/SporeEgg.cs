using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SporeEgg : MonoBehaviour
{
    Character _character;
    EnemyAI _enemyAI;
    bool _start;
    float _elasepdTime;

    private void Awake()
    {
        _character = GetComponent<Character>();
        _enemyAI = GetComponent<EnemyAI>();
    }
    void Update()
    {
        if (_enemyAI.Target != null)
        {
            _start = true;
        }
        if (_start)
        {
            _elasepdTime += Time.deltaTime;

            if(_elasepdTime>3)
            {
                for (int i = 0; i < 3; i++)
                {
                    EnemyNameDefine enemyOrigin = Managers.GetManager<DataManager>().GetData<EnemyNameDefine>((int)Define.EnemyName.WhiteBat);
                    GameObject enemy = Managers.GetManager<GameManager>().GenerateCharacter(enemyOrigin.gameObject, transform.position);
                    if (enemy)
                    {
                        enemy.GetComponent<Character>().AddForce(Random.insideUnitCircle * 200);
                    }
                    
                }
                _character.Dead();
            }
        }

    }
}
