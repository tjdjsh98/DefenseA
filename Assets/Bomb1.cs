using MoreMountains.Feedbacks;
using MoreMountains.FeedbacksForThirdParty;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Timeline;
using UnityEngine;

public class Bomb1 : MonoBehaviour
{
    Character _character;
    EnemyAI enemyAI;
    SpriteRenderer _spriteRenderer;


    int _sequence = 0;
    float _sequenceElapsed = 0;

    // 날아가기 까지
    Vector3 _initPosition;
    Vector3 _flyPosition;
    float _flyTime = 2;

    // 잠시 대기
    float _idleTime = 3;

    // 시간이 되면 플레이어 쪽으로 발사
    [SerializeField] Define.Range _size;
    Vector3 _fireDirection;
    float _speed;


    private void Awake()
    {
        _character = GetComponent<Character>();
        _character.AttackHandler += OnAttack;
        _spriteRenderer =  transform.Find("Model").GetComponent<SpriteRenderer>();
    }

    private void OnAttack(Character arg1, int arg2)
    {
                _sequence = 0;
        Managers.GetManager<ResourceManager>().Destroy(gameObject);
    }

    public void SetFlyPosition(Vector3 position)
    {
        _flyPosition = position;
        _sequence = 1;
    }


    private void Update()
    {
        if (_sequence == 1)
        {
            _initPosition = transform.position;
            _sequence++;
        }
        if (_sequence == 2)
        {
            _sequenceElapsed += Time.deltaTime;
            float lerp = _sequenceElapsed / _flyTime;
            if(_sequenceElapsed >= _flyTime)
            {
                _sequenceElapsed = 0;
                lerp = 1;
                _sequence++;
            }
            transform.position = Vector3.Lerp(_initPosition, _flyPosition, lerp);
        }
        if (_sequence == 3)
        {
            _sequenceElapsed += Time.deltaTime;
            float r = _sequenceElapsed/_idleTime;
            if (_spriteRenderer)
            {
                _spriteRenderer.color = new Color(r, 1, 1, 1);
            }
            if (_idleTime < _sequenceElapsed)
            {
                _sequence++;
                _sequenceElapsed = 0;
            }
        }
        if (_sequence == 4)
        {
            _fireDirection = Managers.GetManager<GameManager>().Girl.GetCenter()- transform.position;
            _sequence++;
        }
        if (_sequence == 5)
        {
            _character.SetVelocity(_fireDirection.normalized * 70f);

            bool isHit = false;
            Util.RangeCastAll2D(gameObject, _character.GetSize(), Define.CharacterMask, (hit) =>
            {
                if (isHit) return false;
                Character character = hit.collider.GetComponent<Character>();
                if (character && character.CharacterType == Define.CharacterType.Player)
                {
                    _character.Attack(character, _character.AttackPower, 40, _fireDirection.normalized, hit.point, 0f);
                }
                return false;
            });
            if (_character.IsContactGround || isHit)
            {
                _sequence = 0;
                Managers.GetManager<ResourceManager>().Destroy(gameObject);
            }
        }
    }
}
