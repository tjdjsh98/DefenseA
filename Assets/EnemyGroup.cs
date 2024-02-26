using MoreMountains.Feedbacks;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

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

    float _layerDistance = 1;

    bool _init = false;
    private void Awake()
    {
        _enemyList = GetComponentsInChildren<EnemyAI>().ToList();

        foreach(var enemy in _enemyList)
        {
          
            _initLocalPositions.Add(Vector3.zero);

            enemy.IsAutoMove = false;
            _enemyCharacterList.Add(enemy.GetComponent<Character>());
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
            Vector3 playerPosition = Managers.GetManager<GameManager>().Daughter.GetCenter();
            if (!_init)
            {
                _velocity = (playerPosition - transform.position).normalized;
                _init = true;
            }

            _leadDirection = (playerPosition - _leadSymbol.transform.position).normalized;

            _velocity += (_leadDirection * _accel) * Time.deltaTime;
            _velocity.x = Mathf.Clamp(_velocity.x, -_speed,_speed);
            _velocity.y = Mathf.Clamp(_velocity.y, -_speed,_speed);
            
            _leadSymbol.transform.position += _velocity * Time.deltaTime;





            // 각 몬스터들의 위치 설정
            for (int i = 0; i < _enemyCharacterList.Count; i++)
            {
                if (_enemyCharacterList[i] == null) continue;

                Vector3 targetPosition = Vector3.zero;
                float angle = 360 / _enemyCharacterList.Count * i;
                Vector3 localPosition = new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad) * _layerDistance, Mathf.Sin(angle * Mathf.Deg2Rad) * _layerDistance, 0);

                targetPosition.x = _leadSymbol.transform.position.x + localPosition.x;
                targetPosition.y = _leadSymbol.transform.position.y + localPosition.y;
                _targetPositions[i] = targetPosition;
            }


            int count = 0;
            // 캐릭터들의 이동
            Vector3 crownCenter = Vector3.zero;
            for (int i = 0; i < _enemyCharacterList.Count; i++)
            {
                if(_enemyCharacterList[i] == null) continue;

                allDestroyed = false;
                Vector3 moveDirection = Vector3.zero;

                if ((_targetPositions[i] - _enemyCharacterList[i].transform.position).magnitude > 1)
                    moveDirection = (_targetPositions[i] - _enemyCharacterList[i].transform.position).normalized;
                else
                    moveDirection = _targetPositions[i] - _enemyCharacterList[i].transform.position;

                _enemyCharacterList[i].Move(moveDirection);
                crownCenter.x += _enemyCharacterList[i].transform.position.x;
                crownCenter.y += _enemyCharacterList[i].transform.position.y;
                count++;
            }

            crownCenter.x /= count;
            crownCenter.y /= count;

            _layerDistance = Mathf.Clamp((2 - (crownCenter - _leadSymbol.transform.position).magnitude),0,3) + 1;
        }

        if (allDestroyed) Destroy(gameObject);
    }
}
