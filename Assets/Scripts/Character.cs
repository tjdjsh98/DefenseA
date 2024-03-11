using MoreMountains.Feedbacks;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Properties;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.TextCore.Text;

public class Character : MonoBehaviour
{
    Rigidbody2D _rigidBody;
    BoxCollider2D _boxCollider;
    CapsuleCollider2D _capsuleCollider;
    Animator _animator;

    #region 캐릭터능력치
    [Header("캐릭터 능력치")]
    [SerializeField] Define.CharacterType _characterType;
    public Define.CharacterType CharacterType=>_characterType;
    [SerializeField] int _maxHp;
    public int MaxHp => _maxHp;
    [SerializeField] int _hp;
    public int Hp { set => _hp = value; get => _hp; }
    
    [SerializeField] int _maxMental = 100;
    public int MaxMental => _maxMental;
    [field:SerializeField]public int Mental { set; get; }

    // 증가되는 능력치
    public float IncreasedRecoverHpPower { set; get; }
    public float IncreasedDamageReducePercentage { set; get; }

    float _recoverHpTime;
    float _recoverHpAmount;
    [SerializeField] float _speed;
    public float Speed => _speed;

    [SerializeField] bool _isGainMentalWhenKillIt;
    [SerializeField] int _gainMentalAmount;
    #endregion
    [field:SerializeField]public int AttackPower { set; get; }

    [SerializeField] float _jumpPower = 10;
    [SerializeField] bool _isEnableRevive;
    [SerializeField] bool _isEnableFly = false;
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
    bool _isMove = false;
    public bool IsDead { set; get; }
    bool _isKnockBack;
    bool _isContactGround = false;
    public bool IsContactGround => _isContactGround;
    private bool _isJump;

    float _groundAccelePower = 60;
    float _airAccelePower = 60;
    float _groundBreakPower = 60;
    float _airBreakPower = 20;


    // 핸들러
    public Action PlayAttackHandler;
    public Action FinishAttackHandler;
    public Action CharacterDead;
    public Action<Character,int> AttackHandler; // 공격한 적, 데미지
    public Action<Character, int, float, Vector3, float> CharacterDamaged { set; get; }

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
        _hp = _maxHp;
        if(_animator)
            _animator.logWarnings = false;
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
        HandleBreak();
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

    void HandleBreak()
    {
        // 브레이크
        if (!_isMove )
        {
            float xSpeed = _rigidBody.velocity.x;
            if (xSpeed != 0)
            {
                if (_isContactGround)
                {
                    if (xSpeed > 0 && xSpeed - Time.deltaTime * _groundBreakPower < 0)
                        xSpeed = 0;
                    else if (xSpeed <0 &&  xSpeed + Time.deltaTime * _groundBreakPower > 0)
                        xSpeed = 0;
                    else 
                        xSpeed += Time.deltaTime * _groundBreakPower * (xSpeed > 0 ? -1 : 1);
                }
            }
            float ySpeed = _rigidBody.velocity.y;
            if (IsEnableFly)
            {
                if (xSpeed > 0 && xSpeed - Time.deltaTime * _airBreakPower < 0)
                    xSpeed = 0;
                else if (xSpeed < 0 && xSpeed + Time.deltaTime * _airBreakPower > 0)
                    xSpeed = 0;
                else
                    xSpeed += Time.deltaTime * _airBreakPower * (xSpeed > 0 ? -1 : 1);

                if (ySpeed > 0 && ySpeed - Time.deltaTime * _airBreakPower < 0)
                    ySpeed = 0;
                else if (ySpeed < 0 && xSpeed + Time.deltaTime * _airBreakPower > 0)
                    ySpeed = 0;
                else
                    ySpeed += Time.deltaTime * _airBreakPower * (ySpeed > 0 ? -1 : 1);
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
        _hp += value;
    }


    // 최종적으로 가한 데미지를 반환합니다.
    public int Damage(Character attacker, int damage, float power, Vector3 direction, float stunTime = 0.1f)
    {

        damage = IncreasedDamageReducePercentage > 0 ? Mathf.RoundToInt(damage / (1 + IncreasedDamageReducePercentage / 100)) :
            Mathf.RoundToInt(damage * (1 - IncreasedDamageReducePercentage / 100));

        if(damage != 0)
            Managers.GetManager<TextManager>().ShowText(transform.position + Vector3.up, damage.ToString(), 10, Color.red);


        _hp -= damage;
        _rigidBody.AddForce(direction.normalized * power, ForceMode2D.Impulse);

        CharacterDamaged?.Invoke(attacker, damage, power, direction, stunTime);

        if (_hp <= 0)
        {
            CharacterDead?.Invoke();
            if (_isGainMentalWhenKillIt)
            {
                attacker.Mental += _gainMentalAmount;
            }

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

        return damage;
    }

    public int Attack(Character target, int damage, float power, Vector3 direction, float stunTime = 0.1f)
    {
        if (target == null) return 0;

        int resultDamage = target.Damage(this,damage, power, direction, stunTime);
        AttackHandler?.Invoke(target,resultDamage);

        return resultDamage;
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
            direction.x = Mathf.Clamp(direction.x, -1, 1);
            direction.y = Mathf.Clamp(direction.y, -1, 1);
            float maxXSpeed = Mathf.Abs( _speed * direction.x);
            float maxYSpeed = Mathf.Abs( _speed * direction.y);

            Vector2 currentSpeed = _rigidBody.velocity;

            if (IsEnableFly || _isContactGround)
            {
                if (Mathf.Abs(currentSpeed.x + (direction.x > 0 ? 1 : -1) * (_isContactGround ? _groundAccelePower : _airAccelePower) * Time.deltaTime) > maxXSpeed)
                    currentSpeed.x = currentSpeed.x > 0 ? maxXSpeed : -maxXSpeed;
                else
                    currentSpeed.x += (direction.x > 0 ? 1 : -1) * (_isContactGround ? _groundAccelePower : _airAccelePower) * Time.deltaTime;
            }
            if (IsEnableFly)
            {
                if (Mathf.Abs(currentSpeed.y + (direction.y > 0 ? 1 : -1) * _airAccelePower * Time.deltaTime) > maxYSpeed)
                {
                    currentSpeed.y = currentSpeed.y > 0 ? maxYSpeed : -maxYSpeed;
                }
                else
                {
                    currentSpeed.y += (direction.y > 0 ? 1 : -1) * _airAccelePower * Time.deltaTime;
                }
            }
            _rigidBody.velocity = currentSpeed;
            _isMove = true;
        }
    }

    public void PlayAttack()
    {
        PlayAttackHandler?.Invoke();
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

        _isJump = true;
    }
    public void Jump(Vector3 direction, float power)
    {
        _rigidBody.AddForce(direction * power, ForceMode2D.Impulse);

        _isContactGround = false;
        _isJump = true;
    }
    void CheckGround()
    {
        if (_groundCheckRange.size == Vector3.zero) return;

        if(!_isContactGround && _rigidBody.velocity.y > 0) return;
        
        GameObject[] gos = Util.RangeCastAll2D(gameObject, _groundCheckRange, LayerMask.GetMask("Ground"));

        if (gos.Length > 0)
        {
            _isContactGround = true;
            _isJump = false;
            AnimatorSetBool("IsContactGround", _isContactGround);
        }
        else
        {
            _isContactGround = false;
            AnimatorSetBool("IsContactGround", _isContactGround);
        }

    }

    public void AnimatorSetBool(string name, bool value)
    {
        if(_animator)
            _animator.SetBool(name, value);
    }
    public void AnimatorSetTrigger(string name)
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

    public void SetVelocityForcibly(Vector3 velocity)
    {
        _rigidBody.velocity = velocity;
    }

    public Define.Range GetSize()
    {
        Define.Range size = new Define.Range()
        {
            center = _boxCollider ? _boxCollider.offset : _capsuleCollider ? _capsuleCollider.offset : Vector3.zero,
            size = _boxCollider ? _boxCollider.size : _capsuleCollider ? _capsuleCollider.size : Vector3.zero,
            figureType = _boxCollider ? Define.FigureType.Box : _capsuleCollider ? Define.FigureType.Circle : Define.FigureType.Box
        };

        return size;
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
