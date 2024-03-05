using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barrier : MonoBehaviour
{
    Character _character;
    CircleCollider2D _circle;
    GameObject _model;

    bool _isExpansion;

    float _maxExpansionSize;
    float _currentExpansionSize;


    private void Awake()
    {
        _character = GetComponent<Character>();
        _model = transform.Find("Model").gameObject;
        _circle = GetComponent<CircleCollider2D>();
    }

    private void Update()
    {
        if (_isExpansion)
        {

            if (_currentExpansionSize < _maxExpansionSize)
                _currentExpansionSize += Time.deltaTime*2 * _maxExpansionSize;
            else
                _currentExpansionSize = _maxExpansionSize;
            _circle.radius= _currentExpansionSize/2;
            _model.transform.localScale = new Vector3(_currentExpansionSize, _currentExpansionSize, 1);

            Define.Range range = new Define.Range() {
                center = Vector3.zero,
                size = new Vector3(_currentExpansionSize / 2, 0),
                figureType= Define.FigureType.Circle
            };

            GameObject[] gameObjects = Util.RangeCastAll2D(gameObject, range, LayerMask.GetMask("Character"));

            foreach (GameObject gameObject in gameObjects)
            {
                Character character = gameObject.GetComponent<Character>();
                if (character != null && character.CharacterType == Define.CharacterType.Enemy)
                {
                    Vector3 direction = character.transform.position - transform.position;
                    direction = direction.normalized;
                    character.transform.position += Time.deltaTime * 2 * _maxExpansionSize * direction;
                }
            }
        }
    }


    public void StartExpansion(float expansionSize)
    {
        _maxExpansionSize= expansionSize;
        _currentExpansionSize = 0;
        _isExpansion= true;
    }

}
