using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossMove : MonoBehaviour, IMoveAI
{
    Character _character;
    Player _player;
    [SerializeField] float _semiMajorAxis = 40f;
    [SerializeField] float _semiMinorAxis = 10f;

    Vector3 _nextMove;
    float _moveElaspedTime = 0;

    private void Awake()
    {
        _character = GetComponent<Character>();
        _player = Managers.GetManager<GameManager>().Player;

    }
    
    public void MoveAI()
    {
        if (_player == null ) return;

        _moveElaspedTime += Time.deltaTime * _character.Speed;

        if ((_nextMove - transform.position).magnitude < _character.Speed * Time.deltaTime)
        {
            _character.transform.position = _nextMove;
            
            float x = _player.transform.position.x + _semiMajorAxis * Mathf.Cos(_moveElaspedTime * Mathf.Deg2Rad);
            float y = _player.transform.position.y + 20 + _semiMinorAxis * Mathf.Sin(_moveElaspedTime * Mathf.Deg2Rad);

            _nextMove = new Vector3(x, y);
        }
        else 
        {
            transform.position += (_nextMove - transform.position).normalized * _character.Speed * Time.deltaTime;
        }
    }
}
