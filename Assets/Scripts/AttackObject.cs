using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackObject : MonoBehaviour
{
    [SerializeField] Define.Range _attackRange;

    bool _isStartAttack;
    Character _owner;
    int _damage;
    private void Awake()
    {
        Vector3 scale = transform.localScale;
        scale.y = 0;
        transform.localScale = scale;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawWireCube(transform.position + _attackRange.center, _attackRange.size);
    }

    private void Update()
    {
        if(_isStartAttack)
        {
            Vector3 scale = transform.localScale;
            if (scale.y < 3)
            {
                scale.y += Time.deltaTime * 10;
                transform.localScale = scale;
            }
            else
            {
                scale.y = 3;
                transform.localScale = scale;
                Attack();
                _isStartAttack = false;
            }
        }

        if(!_isStartAttack && transform.localScale.y == 3)
        {
            Managers.GetManager<ResourceManager>().Destroy(gameObject);
        }
    }
    public void StartAttack(Character owner,int damage)
    {
        _isStartAttack = true;
        _owner= owner;
        _damage = damage;
        
    }

    void Attack()
    {
        GameObject[] gos = Util.RangeCastAll2D(gameObject, _attackRange);

        foreach (var go in gos)
        {
            Character c = go.GetComponent<Character>();
            if (c != null && c.CharacterType == Define.CharacterType.Enemy)
            {
                _owner.Attack(c, _damage, 5, Vector3.up);

            }
        }
    }
}
