using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OctoFly : MonoBehaviour
{
    EnemyAI _enemyAI;

    public float _selfDestructionTime = 5;
    public float _selfDestructionElasped;

    [SerializeField] Define.EnemyName _enemyName;
    [SerializeField] int _summonCount;

    private void Awake()
    {
        _enemyAI = GetComponent<EnemyAI>();
    }

    public void Update()
    {
        if (_enemyAI.Target != null)
        {
            _selfDestructionElasped += Time.deltaTime;
            if (_selfDestructionTime <= _selfDestructionElasped)
            {
                SeflDestuction();
            }
        }
    }

    void SeflDestuction()
    {
        for(int i =0; i < _summonCount;i++)
        {
            EnemyNameDefine enemyName = Managers.GetManager<ResourceManager>().Instantiate<EnemyNameDefine>((int)_enemyName);
            Character character = enemyName.GetComponent<Character>();
            character.transform.position = transform.position;
            character.SetHp(Mathf.RoundToInt( character.MaxHp * Managers.GetManager<GameManager>().EnemyStatusMultifly));
            character.AddForce(new Vector2(Random.Range(-1, 1), 1).normalized * 80);
        }

        Managers.GetManager<ResourceManager>().Destroy(gameObject);
    }
}
