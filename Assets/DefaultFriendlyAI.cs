using System.Collections;
using UnityEngine;

public class DefaultFriendlyAI : MonoBehaviour
{
    [SerializeField] bool _debug;

    Character _character;
    Character Girl => Managers.GetManager<GameManager>().Girl;
    

    [SerializeField] SpriteRenderer _model;

    [SerializeField] Define.Range _detectRange;
    [SerializeField] Define.Range _attackRange;


    float _playerToCharacterDistance = 5;
    int _sequence;
    float _sequenceTime;

    GameObject _target;

    Coroutine _aiCoroutine;



    private void Awake()
    {
        _character = GetComponent<Character>();
    }
    private void OnDrawGizmosSelected()
    {
        if (!_debug) return;

        Util.DrawRangeOnGizmos(gameObject, _attackRange,Color.red);
        Util.DrawRangeOnGizmos(gameObject, _detectRange,Color.green);
    }

    private void Update()
    {
        AI();
    }

    void AI()
    {
        // 적 찾기 
        if (_sequence == 0)
        {
            if (_aiCoroutine != null)
                StopCoroutine(_aiCoroutine);

            _aiCoroutine = StartCoroutine(CorDetectEnemy());
            _sequence++;
        }
        // 플레이어 추적
        if (_sequence == 1)
        {
            _character.TurnBody(Girl.transform.position - transform.position);
            if (_playerToCharacterDistance < Mathf.Abs(Girl.transform.position.x - transform.position.x))
            {
                _character.Move((Girl.transform.position - transform.position) * (Mathf.Abs(Girl.transform.position.x - transform.position.x) - _playerToCharacterDistance));
            }

            if (_target)
            {
                if (_aiCoroutine != null)
                    StopCoroutine(_aiCoroutine);
                _sequence++;
            }
        }

        // 적이 있는 방향으로 이동
        if (_sequence == 2)
        {
            // 적이 사라지면 0번으로 이동
            if (_target == null || !_target.activeSelf)
            {
                _target = null;
                _sequence = 0;
                return;
            }
            _character.Move(_target.transform.position - transform.position);
            if (Util.RangeCastAll2D(gameObject, _attackRange, Define.CharacterMask, hit =>
            {
                Character character = hit.collider.GetComponent<Character>();
                if (character != null && !character.IsDead && character.CharacterType == Define.CharacterType.Enemy)
                {
                    _target = character.gameObject;
                    return true;
                }
                return false;
            }).Count > 0)
            {
                _character.SetVelocityForcibly(Vector3.zero);
                _character.TurnBody(_target.transform.position - transform.position);
                _character.IsAttack = true;
                _character.IsEnableTurn = false;
                _character.IsEnableMove = false;
                _sequence++;
            }
        }

        // 공격 애니메이션
        if (_sequence == 3)
        {
            _sequenceTime += Time.deltaTime;
            if (_sequenceTime > 0.5f)
            {
                _model.color = new Color(_sequenceTime / 0.5f, 1- _sequenceTime / 0.5f, 1- _sequenceTime / 0.5f, 1);
                _sequenceTime = 0;
                _sequence++;
            }
        }

        // 타격
        if (_sequence == 4)
        {
            Util.RangeCastAll2D(gameObject, _attackRange, Define.CharacterMask, hit =>
            {
                Character character = hit.collider.GetComponent<Character>();
                if (character != null && !character.IsDead && character.CharacterType == Define.CharacterType.Enemy)
                {
                    _character.Attack(character, _character.AttackPower, 30, character.transform.position - transform.position, hit.point, 0f);
                }
                return true;
            });
            _sequence++;
            _character.IsAttack = false;
            _character.IsEnableTurn = true;
            _character.IsEnableMove = true;
        }
        if (_sequence == 5)
        {

            _sequenceTime += Time.deltaTime;
            if (_sequenceTime > 3)
            {
                _model.color = new Color(1, 1, 1, 1);
                _sequenceTime = 0;
                _sequence = 0;
            }

        }
    }

    void ResetAI()
    {

    }

    IEnumerator CorDetectEnemy()
    {
        while (true)
        {
            Util.RangeCastAll2D(gameObject, _detectRange, Define.CharacterMask, hit =>
            {
                if (_target != null) return false;

                Character character = hit.collider.GetComponent<Character>();
                if (character != null && !character.IsDead&& character.CharacterType == Define.CharacterType.Enemy)
                {
                    _target = hit.collider.gameObject;
                }
                return true;
            });
            yield return new WaitForSeconds(0.2f);
        }
    }
}
