using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
    Character _character;
    EnemyAI enemyAI;

    bool _isStartSelfDestruct;
    float _selfDestructTime;

    [SerializeField]Define.Range _explosionRange;
    [SerializeField]float _selfDestructDuration = 3;
    [SerializeField]float _explosionPower = 50;
    [SerializeField]int _explosionDamage = 5;
    private void Awake()
    {
        _character = GetComponent<Character>();
        enemyAI = GetComponent<EnemyAI>();
        _isStartSelfDestruct = false;
    }

    private void Update()
    {
        if(enemyAI.Target!= null) _isStartSelfDestruct = true;


        if (_isStartSelfDestruct)
        {
            if(_selfDestructDuration > _selfDestructTime)
            {
                _selfDestructTime += Time.deltaTime;
            }
            else
            {
                _selfDestructTime = 0;

                Explosion();
            }
        }
    }
    void Explosion()
    {
        Util.RangeCastAll2D(gameObject, _explosionRange,
            Define.CharacterMask, (go) =>
            {
                Character character = go.GetComponent<Character>();

                if (character != null && character.CharacterType != _character.CharacterType)
                {
                    _character.Attack(character, _explosionDamage, _explosionPower, character.transform.position - _character.transform.position, 1);
                }
                return false;
            });

        _character.Damage(_character, _character.MaxHp, 0, Vector3.zero, 0);
    }
}
