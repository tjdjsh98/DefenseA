using MoreMountains.FeedbacksForThirdParty;
using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using UnityEngine;

public class JumpAttack : MonoBehaviour
{
    Character _character;
    Rigidbody2D _rigidbody;
    LineRenderer _lineRenderer;
    
    GameObject _targetingPlayer;
    Player _player;

    [SerializeField]Vector3 _fireDirection = new Vector3(0.7f, 0.3f);
    [SerializeField] float _originJumpPower = 50;

    float _jumpPower;

    float _checkTime = 0;

    bool _isJumpAttack = false;
    List<GameObject> _attackList = new List<GameObject>();
    Define.Range _attackRange;

    bool _isReadyJump = false;
    float _jumpReadyTime = 0;
    void Awake()
    {
        _character = GetComponent<Character>();
        _rigidbody = GetComponent<Rigidbody2D>();
        _lineRenderer = GetComponentInChildren<LineRenderer>();
        _attackRange = _character.GetSize();
    }


    void Update()
    {
        if(_isJumpAttack)
        {
            List<RaycastHit2D> hits = Util.RangeCastAll2D(gameObject, _attackRange, LayerMask.GetMask("Character"));

            foreach(var hit in hits)
            {
                if (_attackList.Contains(hit.collider.gameObject)) continue;
                _attackList.Add(hit.collider.gameObject);

                Character character = hit.collider.GetComponent<Character>();
                if (character && character.CharacterType == Define.CharacterType.Player)
                {
                    _character.Attack(character,_character.AttackPower,10,_rigidbody.velocity);
                }
            }
            if (_character.IsContactGround)
            {
                EndJumpAttack();
            }
        }
        _checkTime += Time.deltaTime;

        if (!_isReadyJump && !_isJumpAttack && _checkTime > 1)
        {
            _checkTime = 0;

            if (_character.IsAttack) return;

            if (_player == null)
                _player = Managers.GetManager<GameManager>().Player;

            if (_player == null) return;

            if ((_player.transform.position - transform.position).magnitude < 10) return;



            for (int i = 90; i >= 45; i -= 5)
            {
                float angle = i;

                if (transform.localScale.x < -0)
                    angle = 180 - angle;

                angle *= Mathf.Deg2Rad;


                for (float tempPower = _originJumpPower - 5; tempPower <= _originJumpPower + 5; tempPower+=2)
                {
                    _targetingPlayer = PredictTrajectory(transform.position, new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)).normalized * tempPower);

                    if (_targetingPlayer != null)
                    {
                        _jumpPower = tempPower;
                        _character.IsAttack = true;
                        _fireDirection = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle)).normalized;
                        _jumpReadyTime = 0;
                        _isReadyJump = true;
                        _character.IsEnableMove = false;
                        _character.AnimatorSetTrigger("Ready");
                        return;
                    }
                }
            }
        }

        if(_isReadyJump&& !_isJumpAttack)
        {
            if (_jumpReadyTime < 1)
            {
                _jumpReadyTime += Time.deltaTime;
            }
            else
            {
                _character.AnimatorSetTrigger("Jump");
                _isReadyJump = false;
                _isJumpAttack = true;
                _character.IsAttack = true;
                _character.Jump(_fireDirection, _jumpPower);
                _attackList.Clear();
            }
        }
    }

    void EndJumpAttack()
    {
        _character.IsAttack =false;
        _isJumpAttack = false;
            _character.IsEnableMove = true;
    }
    GameObject PredictTrajectory(Vector3 startPos, Vector3 vel)
    {
        int step = 180;
        float deltaTime = Time.fixedDeltaTime;
        Vector3 gravity = Physics.gravity * _rigidbody.gravityScale ;

        Vector3 position = startPos;
        Vector3 velocity =vel/ _rigidbody.mass;

        _lineRenderer.positionCount = step + 1;
        _lineRenderer.SetPosition(0, position);
        for (int i = 0; i < step; i++)
        {
            position += velocity * deltaTime + 0.5f * gravity * deltaTime * deltaTime;
            velocity += gravity * deltaTime;
            _lineRenderer.SetPosition(i + 1, position);


            RaycastHit2D[] hits = Physics2D.CircleCastAll(position, 0.01f, Vector2.zero, 0, LayerMask.GetMask("Character"));

            if (hits.Length > 0)
            {
                foreach (var hit in hits)
                {
                    if (hit.collider.gameObject.tag == "Player")
                    {
                        return hit.collider.gameObject;
                    }
                }
            }
        }
        return null;
    }

}
