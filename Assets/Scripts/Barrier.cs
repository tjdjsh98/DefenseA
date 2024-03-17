using MoreMountains.FeedbacksForThirdParty;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Barrier : MonoBehaviour
{
    Character _character;
    CircleCollider2D _circle;
    Effect _barrierEffect;

    bool _isExpansion;

    float _maxExpansionSize;
    float _currentExpansionSize;
    float _durationTime;
    float _elasepdTime;

    private void Awake()
    {
        _character = GetComponent<Character>();
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
            _barrierEffect.SetProperty("Size", _currentExpansionSize);

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
            _elasepdTime += Time.deltaTime;
            if(_character)
                _barrierEffect.transform.position = _character.transform.position;
            if(_elasepdTime >= _durationTime)
            {
                Managers.GetManager<ResourceManager>().Destroy(_barrierEffect.gameObject);
                Managers.GetManager<ResourceManager>().Destroy(gameObject);
            }
        }
    }


    public void StartExpansion(Character character, float expansionSize, float durationTime)
    {
        _character = character;
        _currentExpansionSize = 0;
        _maxExpansionSize= expansionSize;
        _isExpansion= true;
        _elasepdTime = 0;
        _durationTime = durationTime;

        _barrierEffect = Managers.GetManager<ResourceManager>().Instantiate(Managers.GetManager<DataManager>().GetData<Effect>((int)Define.EffectName.Barrier));
        _barrierEffect.SetProperty("Size", _currentExpansionSize);
        _barrierEffect.SetProperty("Duration", durationTime);
        _barrierEffect.Play(_character.transform.position);
    }

}
