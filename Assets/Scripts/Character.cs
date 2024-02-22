using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TextCore.Text;

public class Character : MonoBehaviour
{
    Rigidbody2D _rigidBody;
    BoxCollider2D _boxCollider;
    CapsuleCollider2D _capsuleCollider;

    [Header("캐릭터 능력치")]
    [SerializeField] Define.CharacterType _characterType;
    public Define.CharacterType CharacterType=>_characterType;
    [SerializeField] int _maxHp;
    public int MaxHp => _maxHp;
    [SerializeField] int _hp;
    public int Hp { set => _hp = value; get => _hp; }
    
    [SerializeField] int _maxMental = 100;
    public int MaxMental => _maxMental;
    public int Mental { set; get; }

    // 증가되는 능력치
    public float IncreasedRecoverHpPower { set; get; }
    public float IncreasedDamagePercentage { set; get; }

    float _recoverHpTime;
    float _recoverHpAmount;

    [SerializeField] float _speed;
    [SerializeField] float _jumpPower = 10;
    [SerializeField] bool _isEnableFly;
    [SerializeField] bool _isEnableRevive;
    public bool IsEnableFly => _isEnableFly;

    [field:SerializeField]public bool IsEnableMove { set; get; } = true;
    // 스턴 후 전 움직임 가능상태로 변경
    [field:SerializeField]public bool IsEnableTurn { set; get; } = true;


    float _stunEleasped;
    float _stunTime;

    [SerializeField] Define.Range _groundCheckRange;

    // 캐릭터 행동상태
    [SerializeField] bool _isTurnBodyAlongVelocity = true;
    public bool IsStun {private set; get; }
    public bool IsAttack {set; get; }
    public bool IsRoll { set; get; }
    bool _isMove = false;
    public bool IsDead { set; get; }

    bool _isContactGround = false;
    private bool _isJump;

    float _groundAccelePower = 60;
    float _airAccelePower = 60;
    float _groundBreakPower = 60;
    float _airBreakPower = 20;

    public Action AttackHandler;
    public Action FinishAttackHandler;
    public Action CharacterDead;

    [SerializeField] GaugeBar _hpBar;

    public List<Define.Passive> CharacterPassive = new List<Define.Passive>();


    Animator _animator;

    Vector3 _prePosition;
    Vector3 _mySpeed;
    public Vector3 MySpeed => _mySpeed;

    public Action<Character, int, float, Vector3, float> CharacterDamaged { set; get; }

    public Action<Vector2> BodyTurn;

    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _boxCollider = GetComponent<BoxCollider2D>(); 
        _capsuleCollider = GetComponent<CapsuleCollider2D>();
        _hp = _maxHp;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawWireCube(transform.position + _groundCheckRange.center, _groundCheckRange.size);
    }

    private void Update()
    {
        if(IsDead) return;
        if(Managers.GetManager<GameManager>().IsPlayTimeline) return;

        _recoverHpAmount += Time.deltaTime * IncreasedRecoverHpPower;
        if ((_recoverHpTime += Time.deltaTime) > 1)
        {
            _recoverHpTime = 0;
            if ((int)_recoverHpAmount != 0)
            {
                if (Hp + (int)_recoverHpAmount > MaxHp)
                    Hp = MaxHp;
                else
                    Hp += (int)_recoverHpAmount;

                _recoverHpAmount -= (int)_recoverHpAmount;
            }
        }

        ControlAnimation();
        if (_hpBar)
        {
            _hpBar.SetRatio(_hp, _maxHp);
        }

        if (IsStun)
        {
            _stunEleasped += Time.deltaTime;
            if (_stunEleasped > _stunTime)
            {
                IsStun = false;
                _stunEleasped = 0;
            }
        }
        CheckGround();
        HandleBreak();
    }
    private void FixedUpdate()
    {
        Vector3 currentPosition = transform.position;
        _mySpeed = (currentPosition - _prePosition) / Time.fixedDeltaTime;
        _prePosition = currentPosition;
    }

    void HandleBreak()
    {
        // 브레이크
        if (!_isMove)
        {
            float xSpeed = _rigidBody.velocity.x;
            if (xSpeed != 0)
            {
                if (_isContactGround)
                {
                    xSpeed += Time.deltaTime * _groundBreakPower * (xSpeed > 0 ? -1 : 1);
                    if (xSpeed < Time.deltaTime * _groundBreakPower * xSpeed)
                        xSpeed = 0;
                }
                else
                {

                    xSpeed += Time.deltaTime * _airBreakPower * (xSpeed > 0 ? -1 : 1);
                    if (xSpeed < Time.deltaTime * _airBreakPower * xSpeed)
                        xSpeed = 0;
                }
            }
            float ySpeed = _rigidBody.velocity.y;
            if (_isEnableFly)
            {
                ySpeed -= Time.deltaTime * _airBreakPower;
                if (ySpeed < Time.deltaTime * _groundBreakPower * ySpeed)
                    ySpeed = 0;
            }
            _rigidBody.velocity = new Vector3(xSpeed, ySpeed);
        }

        _isMove = false;
    }

    void ControlAnimation()
    {
        if (!_animator || _animator.runtimeAnimatorController == null) return;
        if(!IsStun)
        {
            _animator.SetFloat("WalkBlend", Mathf.Clamp(Mathf.Abs(_rigidBody.velocity.x)/_speed,0,1));
        }
    }

    public void SetHp(int hp)
    {
        _maxHp = hp;
        _hp = hp;
    }
    
    public void SetMaxHp(int maxHp) 
    {
        _maxHp = maxHp;
    }
    public void AddMaxHp(int value)
    {
        _maxHp += value; 
        _hp += value;
    }


    public void Damage(Character attacker, int damage, float power, Vector3 direction, float stunTime = 0.1f)
    {
        Managers.GetManager<TextManager>().ShowText(transform.position + Vector3.up, damage.ToString(), 10, Color.red);

        damage = IncreasedDamagePercentage > 0 ? Mathf.RoundToInt(damage * (1 + IncreasedDamagePercentage / 100)) :
            Mathf.RoundToInt(damage / (1 - IncreasedDamagePercentage / 100));

        _hp -= damage;
        _rigidBody.AddForce(direction.normalized * power, ForceMode2D.Impulse);

        CharacterDamaged?.Invoke(attacker, damage, power, direction, stunTime);

        if (_hp <= 0)
        {
            CharacterDead?.Invoke();
            attacker.Mental += 5;

            if (!_isEnableRevive)
            {
                Destroy(gameObject);
            }
            else
            {
                IsDead = true;
                if(_boxCollider)
                    _boxCollider.enabled = false;
                if(_capsuleCollider)
                    _capsuleCollider.enabled = false;
                _rigidBody.isKinematic = true;
            }
        }

        IsStun = true;
        _stunEleasped = 0;
        _stunTime = stunTime;
    }

    public void Move(Vector2 direction)
    {
    
        if (IsStun) return;

        // 진행 방향에 맞게 몸을 회전
        if (_isTurnBodyAlongVelocity)
        {
            TurnBody(direction);
        }

        // 움직임 제어
        if (IsEnableMove)
        {
            direction.x = Mathf.Clamp(direction.x, -1, 1);
            direction.y = Mathf.Clamp(direction.y, -1, 1);
            float maxSpeed = Mathf.Abs( _speed * direction.x);

            Vector2 currentSpeed = _rigidBody.velocity;

            currentSpeed.x += (direction.x > 0 ? 1 : -1) *  (_isContactGround?_groundAccelePower:_airAccelePower) * Time.deltaTime;
            if (Mathf.Abs(currentSpeed.x) > maxSpeed)
                currentSpeed.x = currentSpeed.x > 0 ? maxSpeed : -maxSpeed;

            if (_isEnableFly)
            {
                currentSpeed.y += (direction.y > 0 ? 1 : -1) * _airAccelePower * Time.deltaTime;
                if (Mathf.Abs(currentSpeed.y) > maxSpeed)
                    currentSpeed.y = currentSpeed.y > 0 ? maxSpeed : -maxSpeed;
            }

            _rigidBody.velocity = currentSpeed;

            _isMove = true;
        }
    }

    public void Attack()
    {
        AttackHandler?.Invoke();
    }
    public void FinishAttack()
    {
        IsAttack= false;
        FinishAttackHandler?.Invoke();
    }

    public void Jump()
    {
        if (!_isContactGround) return;

        _rigidBody.AddForce(Vector2.up * _jumpPower,ForceMode2D.Impulse);

        _isContactGround = false;
        _isJump = true;
    }

    void CheckGround()
    {
        if (_groundCheckRange.size == Vector3.zero) return;
        if (Mathf.Approximately(_rigidBody.velocity.y,0)) return;

        GameObject[] gos = Util.RangeCastAll2D(gameObject, _groundCheckRange,LayerMask.GetMask("Ground"));

        if (gos.Length > 0)
        {
            _isContactGround = true;
            _isJump = false;
        }
        else
        {
            _isContactGround = false;
        }
    }

    public void AnimatorSetBool(string name, bool value)
    {
        _animator.SetBool(name, value);
    }
    public void AnimatorSetTrigger(string name)
    {
        _animator.SetTrigger(name);
    }
    public void SetAnimationSpeed(float speed)
    {
        _animator.speed = speed;
    }
    public void TurnBody(Vector2 direction)
    {
        if (!IsEnableTurn) return;
        Vector3 scale = transform.localScale;
        if (transform.localScale.x < 0 && direction.x > 0)
        {
            scale.x = Mathf.Abs(scale.x);
            transform.localScale = scale;

            BodyTurn?.Invoke(direction);
        }
        else if (transform.localScale.x > 0 && direction.x < 0)
        {
            scale.x = -Mathf.Abs(scale.x);
            transform.localScale = scale;
            BodyTurn?.Invoke(direction);
        }
    }

    public Vector3 GetCenter()
    {
        Vector3 center = Vector3.zero;

        center += transform.position;

        if (_capsuleCollider != null) {
            center += (Vector3)_capsuleCollider.offset;
        }
        if(_boxCollider!= null)
        {
            center += (Vector3)_boxCollider.offset;
        }

        return center;
    }

    public void SetVelocityForcibly(Vector3 velocity)
    {
        _rigidBody.velocity = velocity;
    }

    public void Revive()
    {
        _hp = _maxHp/2;
        IsDead = false;
        if (_boxCollider)
            _boxCollider.enabled = true;
        if (_capsuleCollider)
            _capsuleCollider.enabled = true;
        _rigidBody.isKinematic = false;
    }
}
