using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyGroup : MonoBehaviour
{
    [SerializeField] List<EnemyAI> _enemyList = new List<EnemyAI>();
    [SerializeField] List<Character> _enemyCharacterList = new List<Character>();

    Vector3 _moveDirection = Vector3.zero;
    private void Awake()
    {
        _enemyList = GetComponentsInChildren<EnemyAI>().ToList();
        foreach(var enemy in _enemyList)
        {
            enemy.IsAutoMove = false;
            _enemyCharacterList.Add(enemy.GetComponent<Character>());
        }
      
    }

    public void SetHp(int hp)
    {
        foreach(var character in _enemyCharacterList)
        {
            character.SetHp(hp);
        }
    }
    public void SetHp(float multiply)
    {
        foreach (var character in _enemyCharacterList)
        {
            character.SetHp((int)(character.Hp * multiply));
        }
    }
    private void Update()
    {
        if (Managers.GetManager<GameManager>().IsPlayTimeline) return;
        if (_moveDirection != Vector3.zero)
        {
            foreach (var character in _enemyCharacterList)
            {
                character.Move(_moveDirection);
            }
        }
        else
        {
            if (Managers.GetManager<GameManager>().Player)
                _moveDirection = (Managers.GetManager<GameManager>().Player.transform.position - transform.position).normalized;
        }
    }
}
