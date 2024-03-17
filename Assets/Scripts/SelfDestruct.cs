using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfDestruct : MonoBehaviour
{
    Character _character;
    EnemyAI enemyAI;
    protected SpriteRenderer _attackRangeSprite;

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

        if (transform.Find("AttackRange"))
        {
            _attackRangeSprite = transform.Find("AttackRange").GetComponent<SpriteRenderer>();
        }
        else
        {
            _attackRangeSprite = Managers.GetManager<ResourceManager>().Instantiate("AttackRange").GetComponent<SpriteRenderer>();
            _attackRangeSprite.transform.SetParent(transform);
            _attackRangeSprite.gameObject.SetActive(false);
        }

        _isStartSelfDestruct = false;
    }

    private void OnDrawGizmosSelected()
    {
        Util.DrawRangeOnGizmos(gameObject, _explosionRange, Color.yellow);
    }

    private void Update()
    {
        if (enemyAI.Target != null && !_isStartSelfDestruct)
        {
            _isStartSelfDestruct = true;
            _character.SetSpeed(10);
            _attackRangeSprite.color = new Color(1, 0, 0, 0);
            _attackRangeSprite.transform.localScale = _explosionRange.size;
            _attackRangeSprite.transform.localPosition = _explosionRange.center;
            _attackRangeSprite.gameObject.SetActive(true);
        }


        if (_isStartSelfDestruct)
        {
            if(_selfDestructDuration > _selfDestructTime)
            {
                _selfDestructTime += Time.deltaTime;
                Color color = _attackRangeSprite.color;
                if (_selfDestructDuration != 0)
                    color.a = _selfDestructTime / _selfDestructDuration;
                        _attackRangeSprite.color = color;
            }
            else
            {
                _attackRangeSprite.gameObject.SetActive(false);
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
