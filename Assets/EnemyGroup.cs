using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemyGroup : MonoBehaviour
{
    [SerializeField] List<EnemyAI> _enemyList = new List<EnemyAI>();
    [SerializeField] List<Character> _enemyCharacterList = new List<Character>();
    List<Vector3> _initLocalPositions = new List<Vector3>();
    List<Vector3> _targetPositions = new List<Vector3>();

    [SerializeField] GameObject _leadSymbol;
    [SerializeField] Vector3 _leadDirection;

    [SerializeField] float _accel;
    [SerializeField] float _speed;
    Vector3 _velocity;

    bool _wait = false;

    private void Awake()
    {
        _enemyList = GetComponentsInChildren<EnemyAI>().ToList();
        foreach(var enemy in _enemyList)
        {
            enemy.IsAutoMove = false;
            _enemyCharacterList.Add(enemy.GetComponent<Character>());
            _initLocalPositions.Add(enemy.transform.localPosition);
            _targetPositions.Add(enemy.transform.localPosition);
            _enemyCharacterList[_enemyCharacterList.Count - 1].SetSpeed(_speed);
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

        

    
        bool allDestroyed = true; ;
        if (Managers.GetManager<GameManager>().Player)
        {
            Vector3 playerPosition = Managers.GetManager<GameManager>().Player.transform.position;

            // 리드하는 심볼의 방향 설정과 이동
            if (_leadSymbol.transform.position.x < playerPosition.x)
            {
                _leadDirection.x += Time.deltaTime * _accel;
            }
            else
            {
                _leadDirection.x -= Time.deltaTime * _accel;
            }
            if (_leadSymbol.transform.position.y < playerPosition.y)
            {
                _leadDirection.y += Time.deltaTime * _accel;
            }
            else
            {
                _leadDirection.y -= Time.deltaTime * _accel;
            }
            _leadDirection.x = Mathf.Clamp(_leadDirection.x, -1, 1);
            _leadDirection.y = Mathf.Clamp(_leadDirection.y, -1, 1);

            _velocity += _leadDirection * _speed * Time.deltaTime;
            _velocity.x = _leadDirection.normalized.x * _speed;
            _velocity.y = _leadDirection.normalized.y * _speed;
            _leadSymbol.transform.position += _velocity * Time.deltaTime;

            // 각 몬스터들의 위치 설정
            for (int i = 0; i < _enemyCharacterList.Count; i++)
            {
                if (_enemyCharacterList[i] == null) continue;

                Vector3 targetPosition = Vector3.zero;
                if ((_enemyCharacterList[i].transform.position - _leadSymbol.transform.position).magnitude > 3)
                {
                    targetPosition.x = _leadSymbol.transform.position.x + _initLocalPositions[i].x;
                    targetPosition.y = _leadSymbol.transform.position.y + _initLocalPositions[i].y;
                    _targetPositions[i] = targetPosition;
                }
            }

            // 캐릭터들의 이동
            for (int i = 0; i < _enemyCharacterList.Count; i++)
            {
                if(_enemyCharacterList[i] == null) continue;

                allDestroyed = false;
                if ((_targetPositions[i] - _enemyCharacterList[i].transform.position).magnitude > 0.3f)
                {
                    Vector3 moveDirection = (_targetPositions[i] - _enemyCharacterList[i].transform.position).normalized;
                    _enemyCharacterList[i].Move(moveDirection);
                }
            }
        }

        if (allDestroyed) Destroy(gameObject);
    }
}
