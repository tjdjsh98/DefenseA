using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
    Character _character;
    EnemyAI enemyAI;
    [SerializeField]GameObject _heart;
    protected Material _heartMaterial;
    float _lightingValue;
    static int _lightingID = Shader.PropertyToID("_Lighting");

    bool _isStartSelfDestruct;
    float _selfDestructTime;

    [SerializeField]Define.Range _explosionRange;
    [SerializeField]float _selfDestructDuration = 5;
    [SerializeField]float _explosionPower = 50;

    float _lightingTime;

    private void Awake()
    {
        _character = GetComponent<Character>();
        enemyAI = GetComponent<EnemyAI>();
        enemyAI.MoveRate = 0.4f;
        _heartMaterial = _heart.GetComponent<SpriteRenderer>().material;

        _isStartSelfDestruct = false;
    }

    private void OnDrawGizmosSelected()
    {
        Util.DrawRangeOnGizmos(gameObject, _explosionRange, Color.yellow);
    }

    private void Update()
    {
        if (enemyAI.Target != null &&!_isStartSelfDestruct)
        {
            _isStartSelfDestruct = true;
            enemyAI.MoveRate = 1f;
            _lightingTime = 0;
        }


        if (_isStartSelfDestruct)
        {
            if(_selfDestructDuration > _selfDestructTime)
            {
                _lightingTime += Time.deltaTime * _selfDestructTime/ _selfDestructDuration * 10f;
                if((_selfDestructDuration - _selfDestructTime) < 0.5f)
                    _heartMaterial.SetFloat(_lightingID,20);
                else
                    _heartMaterial.SetFloat(_lightingID,( Mathf.Abs(Mathf.Cos(_lightingTime * Mathf.PI)+1) * 20));
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
            Define.CharacterMask, (hit) =>
            {
                Character character = hit.collider.GetComponent<Character>();

                if (character != null && character.CharacterType != _character.CharacterType)
                {
                    _character.Attack(character, _character.AttackPower, _explosionPower, character.transform.position - _character.transform.position, hit.point, 1);
                }
                return false;
            });

        _character.Damage(_character, _character.MaxHp, 0, Vector3.zero, transform.position, 0);
    }
}
