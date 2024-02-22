using MoreMountains.FeedbacksForThirdParty;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderAttack : MonoBehaviour
{
    Character _character;
    Rigidbody2D _rigidbody;
    LineRenderer _lineRenderer;
    
    GameObject _targetingPlayer;
    Player _player;

    Vector3 _fireDirection = new Vector3(0.7f, 0.3f);
    [SerializeField] float _jumpPower = 50;

    bool _isJumpAttack = false;
    List<GameObject> _attackList = new List<GameObject>();
    Define.Range _attackRange;
    void Awake()
    {
        _character = GetComponent<Character>();
        _rigidbody = GetComponent<Rigidbody2D>();
        _lineRenderer = GetComponentInChildren<LineRenderer>();
        _attackRange = _character.GetSize();
    }


    void Update()
    {
        if(_isJumpAttack )
        {
            GameObject[] gameObjects = Util.RangeCastAll2D(gameObject, _attackRange, LayerMask.GetMask("Character"));

            foreach(var gameObject in gameObjects)
            {
                if (_attackList.Contains(gameObject)) continue;
                _attackList.Add(gameObject);

                Character character = gameObject.GetComponent<Character>();
                if (character && character.CharacterType == Define.CharacterType.Player)
                {
                    character.Damage(_character,1,10,_rigidbody.velocity);
                }
            }
        }

        if (_character.IsAttack) return;

        if (_player == null)
            _player = Managers.GetManager<GameManager>().Player;

        if (_player == null) return;

        if ((_player.transform.position - transform.position).magnitude < 10) return;


        _fireDirection.x = transform.lossyScale.x > 0 ? Mathf.Abs(_fireDirection.x): -Mathf.Abs(_fireDirection.x);
        _targetingPlayer = PredictTrajectory(transform.position, _fireDirection * _jumpPower);
        if(_targetingPlayer== null) return;

        _character.IsAttack = true;

        _character.Jump(_fireDirection, _jumpPower);
        _isJumpAttack= true;
        _attackList.Clear();
        Invoke("After", 2);
    }

    void After()
    {
        _character.IsAttack =false;
        _isJumpAttack = false;
    }
    GameObject PredictTrajectory(Vector3 startPos, Vector3 vel)
    {
        int step = 180;
        float deltaTime = Time.fixedDeltaTime;
        Vector3 gravity = Physics.gravity * _rigidbody.gravityScale ;

        Vector3 position = startPos;
        Vector3 velocity = (Vector3)_rigidbody.velocity + vel/ _rigidbody.mass;

        _lineRenderer.positionCount = step+1;
            _lineRenderer.SetPosition(0, position);
        for (int i = 0; i < step; i++)
        {
            position += velocity * deltaTime + 0.5f * gravity * deltaTime * deltaTime;
            velocity += gravity * deltaTime;
            _lineRenderer.SetPosition(i+1, position);


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

    void JumpAttack()
    {

    }
}
