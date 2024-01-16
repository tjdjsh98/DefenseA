using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    Character _character;
    [SerializeField]Define.Range _attackRange;

    Character _target;

    private void Awake()
    {
        _character = GetComponent<Character>();

        _character.CharacterAttack += Attack;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Define.Range range = _attackRange;
        range.center.x = transform.localScale.x > 0 ? range.center.x : -range.center.x;
        Gizmos.DrawWireCube(transform.position + range.center, range.size);
    }
    void Update()
    {
        if (_character.IsAttack || _character.IsStun) return;

        CheckTarget();
        Move();
        PlayAttack();
    }

    private void Move()
    {
        if (_target != null) return;

        _character.Move(Vector2.left);
    }

    private void CheckTarget()
    {
        GameObject[] gameObjects = Util.BoxcastAll2D(gameObject,_attackRange);

        if(gameObjects.Length > 0 ) 
        { 
            foreach(var gameObject in gameObjects)
            {
                if (gameObject == this.gameObject) continue;
                _target = gameObject.GetComponent<Character>();
                if (_target)
                {
                    if (_target.CharacterType == Define.CharacterType.Enemy)
                        _target = null;
                    else
                        return;
                }
            }
        }

        _target = null;
    }

    private void PlayAttack()
    {
        if (_target == null) return;

        _character.Attack(_target);
    }

    public void Attack(Character character)
    {
        character.Damage(_character, 1, 1, Vector3.zero);
    }
}
