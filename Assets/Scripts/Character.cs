using MoreMountains.Feedbacks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class Character : MonoBehaviour,IHp
{
    Rigidbody2D _rigidBody;
    BoxCollider2D _boxCollider;
    CapsuleCollider2D _capsuleCollider;
    CircleCollider2D _circleCollider;
    Animator _animator;

    #region 캐릭터능력치
    [Header("캐릭터 능력치")]
    [SerializeField] Define.CharacterType _characterType;
    public Define.CharacterType CharacterType=>_characterType;
    [SerializeField] int _maxHp;        
    public int MaxHp => _maxHp;
    [SerializeField] int _hp;
    public int Hp { set => _hp = Math.Clamp(value,0,_maxHp); get => _hp; }

    // 증가되는 능력치
    float _increasedHpRegeneration = 0;
    public float IncreasedHpRegeneration
    {
        set
        {
            _increasedHpRegeneration = value;
            _increasedHpRegeneration = (float)Math.Round(_increasedHpRegeneration, 2);

        }
        get
        {
            return _increasedHpRegeneration;
        }
    }
    public float IncreasedDamageReducePercentage { set; get; }

    float _recoverHpTime;
    float _recoverHpAmount;
    [SerializeField] float _speed;
    public float Speed => _speed;
    [SerializeField] float _standing;
    public float Standing => _standing;

    #endregion
    [field:SerializeField]public int AttackPower { set; get; }
    [SerializeField] float _jumpPower = 10;
    [field:SerializeField]public  bool IsNotInstantlyDie = false;
    [SerializeField] bool _isEnableRevive;
    [SerializeField] bool _isEnableFly = false;
    [SerializeField] bool _isHaveDamageInvincibility = false;
    public bool IsEnableFly => _isEnableFly;
    [field:SerializeField]public bool IsEnableMove { set; get; } = true;
    // 스턴 후 전 움직임 가능상태로 변경
    [field:SerializeField]public bool IsEnableTurn { set; get; } = true;


    float _stunEleasped;
    float _stunTime;

    [SerializeField] Define.Range _groundCheckRange;

    // 캐릭터 행동상태
    [field: SerializeField] public bool IsTurnBodyAlongVelocity { set; get; } = true;
    public bool IsStun {private set; get; }
    public bool IsAttack {set; get; }
    public bool IsRoll { set; get; }
    public bool IsDead { set; get; }
    bool _isKnockBack;
    bool _isContactGround = false;
    public bool IsIgnoreBreak { set; get; }
    [field:SerializeField]public bool IsSuperArmer { get; set; }

    public bool IsFaceRight => transform.lossyScale.x > 0;
    [field: SerializeField] public bool IsInvincibility;
    bool _isDamagedInvincibility;

    public bool IsContactGround => _isContactGround;
    private bool _isJump;

    float _groundAccelePower = 30;
    float _airAccelePower = 60;
    float _groundBreakPower = 40;
    float _airBreakPower = 10;

    Vector3 _moveDirection;

    // 핸들러
    public Action CharacterDeadHandler;
    public Action<Character, int, float, Vector3, Vector3, float> AttackHandler; // 공격한 적, 데미지
    public Action<Character, int, float, Vector3, Vector3, float> AddtionalAttackHandler; // 공격한 적, 데미지
    public Action<Character, int, float, Vector3,Vector3, float> DamagedHandler { set; get; }
    public Action<Character, int, float, Vector3,Vector3, float> AddtionalDamagedHandler { set; get; }

    [SerializeField] GaugeBar _hpBar;


    Vector3 _prePosition;
    Vector3 _mySpeed;
    public Vector3 MySpeed => _mySpeed;


    public Action<Vector2> BodyTurnHandler;

    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _boxCollider = GetComponent<BoxCollider2D>(); 
        _capsuleCollider = GetComponent<CapsuleCollider2D>();
        _circleCollider = GetComponent<CircleCollider2D>();

        _hp = _maxHp;
        if (_animator)
            _animator.logWarnings = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawWireCube(transform.position + _groundCheckRange.center, _groundCheckRange.size);
    }

    private void Update()
    {
        if (Hp > 0)
        {
            IsDead = false;
        }
        if (IsDead) return;
        if(Managers.GetManager<GameManager>().IsPlayTimeline) return;

        _recoverHpAmount += Time.deltaTime * IncreasedHpRegeneration;
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

        // 넉백 
        if (IsStun)
        {
            _stunEleasped += Time.deltaTime;
            if (_stunEleasped > _stunTime && _rigidBody.velocity.magnitude < 0.1f)
            {
                IsStun = false;
                _stunEleasped = 0;
            }
        }
        CheckGround();
        HandleMove();
    }
    private void FixedUpdate()
    {
        Vector3 currentPosition = transform.position;
        _mySpeed = (currentPosition - _prePosition) / Time.fixedDeltaTime;
        _prePosition = currentPosition;
    }

    public void SetSpeed(float speed)
    {
        _speed =speed;
    }

    public void Move(Vector2 direction)
    {

        if (IsStun) return;

        // 진행 방향에 맞게 몸을 회전
        if (IsTurnBodyAlongVelocity)
        {
            TurnBody(direction);
        }

        // 움직임 제어
        if (IsEnableMove)
        {
            _moveDirection = direction;
        }
    }

    void HandleMove()
    {
        // 진행 방향에 맞게 몸을 회전
        if (IsTurnBodyAlongVelocity)
        {
            TurnBody(_moveDirection);
        }

        if (IsIgnoreBreak)
        {
            _moveDirection = Vector3.zero;
            return;
        }

        // 가속
        if (_moveDirection != Vector3.zero)
        {
            _moveDirection.x = Mathf.Clamp(_moveDirection.x, -1, 1);
            _moveDirection.y = Mathf.Clamp(_moveDirection.y, -1, 1);
            float maxXSpeed = _speed * _moveDirection.x;
            float maxYSpeed = _speed * _moveDirection.y;

            Vector2 currentSpeed = _rigidBody.velocity;

            if (IsEnableFly || _isContactGround)
            {
                if (_moveDirection.x != 0)
                {
                    if ((_moveDirection.x > 0 && currentSpeed.x + ((_moveDirection.x > 0 ? 1 : -1) * (_isContactGround ? _groundAccelePower : _airAccelePower) * Time.deltaTime) > maxXSpeed) ||
                        (_moveDirection.x < 0 && currentSpeed.x + ((_moveDirection.x > 0 ? 1 : -1) * (_isContactGround ? _groundAccelePower : _airAccelePower) * Time.deltaTime) < maxXSpeed))
                    {
                        currentSpeed.x = maxXSpeed;
                    }
                    else
                        currentSpeed.x += (_moveDirection.x > 0 ? 1 : -1) * (_isContactGround ? _groundAccelePower : _airAccelePower) * Time.deltaTime;
                }
                
            }
            if (IsEnableFly)
            {
                if (_moveDirection.y != 0)
                {
                    if (Mathf.Abs(currentSpeed.y + (_moveDirection.y > 0 ? 1 : -1) * _airAccelePower * Time.deltaTime) > Mathf.Abs(maxYSpeed))
                    {
                        currentSpeed.y = maxYSpeed;
                    }
                    else
                    {
                        currentSpeed.y += (_moveDirection.y > 0 ? 1 : -1) * _airAccelePower * Time.deltaTime;
                    }
                }
            }
            _rigidBody.velocity = currentSpeed;
        }
        else
        {
            Vector2 currentSpeed = _rigidBody.velocity;
            if (IsEnableFly)
            {
                if (currentSpeed.x > 0 && currentSpeed.x - Time.deltaTime * _airBreakPower < 0)
                    currentSpeed.x = 0;
                else if (currentSpeed.x < 0 && currentSpeed.x + Time.deltaTime * _airBreakPower > 0)
                    currentSpeed.x = 0;
                else
                    currentSpeed.x += Time.deltaTime * _airBreakPower * (currentSpeed.x > 0 ? -1 : 1);

                if (currentSpeed.y > 0 && currentSpeed.y - Time.deltaTime * _airBreakPower < 0)
                    currentSpeed.y = 0;
                else if (currentSpeed.y < 0 && currentSpeed.x + Time.deltaTime * _airBreakPower > 0)
                    currentSpeed.y = 0;
                else
                    currentSpeed.y += Time.deltaTime * _airBreakPower * (currentSpeed.y > 0 ? -1 : 1);
            }
            else
            {
                if (currentSpeed.x != 0)
                {
                    if (_isContactGround)
                    {
                        if (currentSpeed.x > 0 && (currentSpeed.x - Time.deltaTime * _groundBreakPower) < 0)
                            currentSpeed.x = 0;
                        else if (currentSpeed.x < 0 && (currentSpeed.x + Time.deltaTime * _groundBreakPower) > 0)
                            currentSpeed.x = 0;
                        else
                            currentSpeed.x += Time.deltaTime * _groundBreakPower * (currentSpeed.x > 0 ? -1 : 1);

                     
                    }
                    else
                    {
                        if (currentSpeed.y > 0 && currentSpeed.y - Time.deltaTime * _airBreakPower < 0)
                            currentSpeed.y = 0;
                        else if (currentSpeed.y < 0 && currentSpeed.y + Time.deltaTime * _airBreakPower > 0)
                            currentSpeed.y = 0;
                        else
                            currentSpeed.y += Time.deltaTime * _airBreakPower * (currentSpeed.y > 0 ? -1 : 1);
                    }
                }
            }
            _rigidBody.velocity = currentSpeed;
        }

        _moveDirection = Vector3.zero;
    }
    public void AddForce(Vector2 forceDirection)
    {
        _moveDirection = Vector2.zero;
        _rigidBody.AddForce(forceDirection, ForceMode2D.Impulse);
    }

    void ControlAnimation()
    {
        if (!_animator || _animator.runtimeAnimatorController == null) return;
        if(!IsStun)
        {
            float speed = _rigidBody.velocity.x * transform.lossyScale.x / Math.Abs(transform.lossyScale.x);
            if (speed > _speed * 0.9f)
            {
                
                _animator?.SetFloat("WalkBlend", 1);

            }
            else
            {
                _animator?.SetFloat("WalkBlend", Mathf.Clamp(speed/ _speed, -0.4f, 1));
            }
        }

        _animator?.SetFloat("YSpeed", _rigidBody.velocity.y);

    }

    public void SetHp(int hp)
    {
        _maxHp = hp;
        _hp = hp;
    }
    public void MulHp(float mul)
    {
        _maxHp = Mathf.RoundToInt(_maxHp * mul);
        _hp = _maxHp;
    }

    public void SetMaxHp(int maxHp) 
    {
        _maxHp = maxHp;
    }
    public void AddMaxHp(int value)
    {
        _maxHp += value; 
        if(!IsDead)
            _hp += value;
    }


    IEnumerator CorDamageInvincibility()
    {
        _isDamagedInvincibility = true;

        yield return new WaitForSeconds(1f);

        _isDamagedInvincibility = false;
    }
    // 최종적으로 가한 데미지를 반환합니다.
    public int Damage(IHp attacker, int damage, float power, Vector3 direction, Vector3 damagePoint, float stunTime = 0f)
    {
        if (IsInvincibility || _isDamagedInvincibility) return 0;

        Character attackerCharacter = attacker as Character;
        direction = direction.normalized;
        damage = IncreasedDamageReducePercentage > 0 ? Mathf.RoundToInt(damage / (1 + IncreasedDamageReducePercentage / 100)) :
            Mathf.RoundToInt(damage * (1 - IncreasedDamageReducePercentage / 100));

        if(damage != 0)
            Managers.GetManager<TextManager>().ShowText(damagePoint, damage.ToString(), 10, Color.red);


        _hp -= damage;

        float knockBack = power;
        knockBack *= (1 - _standing / 100f);
        Vector3 knockBackPower = direction * knockBack;
        _rigidBody.AddForce(direction * knockBack, ForceMode2D.Impulse);
        if (!IsSuperArmer && stunTime > 0)
        {
         
            IsStun = true;
            _stunEleasped = 0;
            _stunTime = stunTime;
        }
        if (_isHaveDamageInvincibility) 
            StartCoroutine(CorDamageInvincibility());

        DamagedHandler?.Invoke(attackerCharacter, damage, power, direction,damagePoint, stunTime);

        if (_hp <= 0)
        {
            Dead();
          
        }

   

        return damage;
    }

    // 추가적으로 받는 공격 효과를 처리합니다.
    public int AddtionalDamage(IHp attacker, int damage, float power, Vector3 direction, Vector3 damagePoint, float stunTime = 0f)
    {
        if (IsInvincibility) return 0;

        Character attackerCharacter = attacker as Character;
        direction = direction.normalized;
        damage = IncreasedDamageReducePercentage > 0 ? Mathf.RoundToInt(damage / (1 + IncreasedDamageReducePercentage / 100)) :
            Mathf.RoundToInt(damage * (1 - IncreasedDamageReducePercentage / 100));

        if (damage != 0)
            Managers.GetManager<TextManager>().ShowText(damagePoint, damage.ToString(), 10, Color.red);


        _hp -= damage;

        float knockBack = power;
        knockBack *= (1 - _standing / 100f);
        Vector3 knockBackPower = direction * knockBack;
        _rigidBody.AddForce(direction * knockBack, ForceMode2D.Impulse);
        if (!IsSuperArmer && stunTime > 0)
        {

            IsStun = true;
            _stunEleasped = 0;
            _stunTime = stunTime;
        }


        AddtionalDamagedHandler?.Invoke(attackerCharacter, damage, power, direction, damagePoint, stunTime);

        if (_hp <= 0)
        {
            Dead();
        }



        return damage;
    }

    public void Dead()
    {
        _rigidBody.velocity = Vector3.zero;
        IsDead = true;
        CharacterDeadHandler?.Invoke();

        if (!_isEnableRevive)
        {
            Destroy(gameObject);
        }
        else
        {
            if (_boxCollider)
                _boxCollider.enabled = false;
            if (_capsuleCollider)
                _capsuleCollider.enabled = false;
            _rigidBody.isKinematic = true;
        }
    }
    // 상대방으로 공격하는 함수
    public int Attack(IHp target, int damage, float power, Vector3 direction,Vector3 attackPoint, float stunTime = 0.1f)
    {
        if (target == null) return 0;

        int resultDamage = target.Damage(this,damage, power, direction,attackPoint, stunTime);
        AttackHandler?.Invoke(target as Character,resultDamage,power,direction,attackPoint,stunTime);

        return resultDamage;
    }

    // 공격 이후에 추가적 효과로 적을 공격한 함수
    public int AddtionalAttack(IHp target, int damage, float power, Vector3 direction, Vector3 attackPoint, float stunTime = 0.1f)
    {
        if (target == null) return 0;

        int resultDamage = target.AddtionalDamage(this, damage, power, direction, attackPoint, stunTime);
        AddtionalAttackHandler?.Invoke(target as Character, resultDamage, power, direction, attackPoint, stunTime);

        return resultDamage;
    }



    public void SetVelocity(Vector2 velcoity)
    {
        _rigidBody.velocity = velcoity;
    }
    public void Jump()
    {
        if (!_isContactGround) return;

        _rigidBody.AddForce(Vector2.up * _jumpPower,ForceMode2D.Impulse);

        _isJump = true;
    }
    public void Jump(Vector3 direction, float power)
    {
        if (!_isContactGround) return;
        
        _rigidBody.AddForce(direction*power,ForceMode2D.Impulse);

        _isContactGround = false;
        _isJump = true;
    }
    void CheckGround()
    {
        if (_groundCheckRange.size == Vector3.zero) return;

        if(!_isContactGround && _rigidBody.velocity.y > 0) return;
        
        List<RaycastHit2D> hits = Util.RangeCastAll2D(gameObject, _groundCheckRange, LayerMask.GetMask("Ground"));

        if (hits.Count> 0)
        {
            _isContactGround = true;
            _isJump = false;
            SetAnimatorBool("IsContactGround", _isContactGround);
        }
        else
        {
            _isContactGround = false;
            SetAnimatorBool("IsContactGround", _isContactGround);
        }

    }

    public void SetAnimatorBool(string name, bool value)
    {
        if (_animator)
            _animator.SetBool(name, value);
    }

    public void SetAnimatorInt(string name, int value)
    {
        if(_animator)
            _animator.SetInteger(name, value);
    }
    public void SetAnimatorTrigger(string name)
    {
        if (_animator)
            _animator.SetTrigger(name);
    }
    public void SetAnimationSpeed(float speed)
    {
        if (_animator)
            _animator.speed = speed;
    }

    public void SetAnimationWeight(string layerName, float weight)
    {
        int index = _animator.GetLayerIndex(layerName);
        _animator.SetLayerWeight(index, weight);
    }
    public void TurnBody(Vector2 direction)
    {
        if (!IsEnableTurn) return;
        Vector3 scale = transform.localScale;
        if (transform.localScale.x < 0 && direction.x > 0)
        {
            scale.x = Mathf.Abs(scale.x);
            transform.localScale = scale;

            BodyTurnHandler?.Invoke(direction);
        }
        else if (transform.localScale.x > 0 && direction.x < 0)
        {
            scale.x = -Mathf.Abs(scale.x);
            transform.localScale = scale;
            BodyTurnHandler?.Invoke(direction);
        }
    }

    public void ChangeEnableFly(bool isFly)
    {
        _rigidBody.gravityScale = isFly?0: 3;
        _isEnableFly = isFly;
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

    public Vector3 GetTop()
    {
        Vector3 center = Vector3.zero;

        center += transform.position;

        if (_capsuleCollider != null)
        {
            center.x += _capsuleCollider.offset.x;
            center.y += _capsuleCollider.size.y;
        }
        if (_boxCollider != null)
        {
            center.x += _boxCollider.offset.x;
            center.y += _boxCollider.size.y;
        }

        return center;
    }




    public void SetVelocityForcibly(Vector3 velocity)
    {
        _rigidBody.velocity = velocity;
    }

    public Define.Range GetSize()
    {
        if (_boxCollider == null)
            _boxCollider = GetComponent<BoxCollider2D>();
        Define.Range size = new Define.Range()
        {
            center = _boxCollider ? _boxCollider.offset : _capsuleCollider ? _capsuleCollider.offset:_circleCollider?_circleCollider.offset : Vector3.zero,
            size = _boxCollider ? _boxCollider.size : _capsuleCollider ? _capsuleCollider.size : _circleCollider ? new Vector3(_circleCollider.radius,0,0) : Vector3.zero,
            figureType = _boxCollider ? Define.FigureType.Box : _capsuleCollider ? Define.FigureType.Circle : _circleCollider ? Define.FigureType.Circle : Define.FigureType.Box
        };

        return size;
    }

  public void SetStanding(float standing)
    {
        if (standing > 100)
            standing = 100;
        _standing = standing;
    }

    public void Revive()
    {
        _hp = _maxHp;
        IsDead = false;
        if (_boxCollider)
            _boxCollider.enabled = true;
        if (_capsuleCollider)
            _capsuleCollider.enabled = true;
        _rigidBody.isKinematic = false;
    }
}
