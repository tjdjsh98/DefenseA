using JetBrains.Annotations;
using MoreMountains.Feedbacks;
using MoreMountains.FeedbacksForThirdParty;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour, IWeaponUsable
{
    Animator _animator;
    Character _character;
    public Character Character { set { Debug.LogWarning("Player 초기화 불가"); } get { return _character;} }
    Rigidbody2D _rigidbody;
    Character _ridingCharacter;
    GirlAbility _girlAbility = new GirlAbility();
    public GirlAbility GirlAbility => _girlAbility;

    [SerializeField] bool _debug;

    [SerializeField] Define.Range _meleeAttackRange;


    [SerializeField] GameObject _frontArm;
    [SerializeField] GameObject _backArmRoot;
    [SerializeField] GameObject _backHand;
    [SerializeField] GameObject _backArmIK;
    [SerializeField] GameObject _head;
    [SerializeField] GameObject _body;

    Vector3 _backArmInitPosition;
    public int ReduceReloadTime { set; get; } = 0;

    [SerializeField] float _initHeadAngle;
    [SerializeField] float _initBodyAngle;
    [SerializeField]float _initFrontArmAngle;

    WeaponSwaper _weaponSwaper;
    public WeaponSwaper WeaponSwaper => _weaponSwaper;

    CameraController _cameraController;

    float _rebound;

    [SerializeField]float _reboundRecoverPower = 20f;
    public float ReboundRecoverPower => IncreasedReboundRecoverPercent > 0 ? _reboundRecoverPower * (1 + IncreasedReboundRecoverPercent / 100) : _reboundRecoverPower / (1 - IncreasedReboundRecoverPercent / 100);
    bool _isRiding;
    // 슬라이딩
    float _slidingTime;
    float _invincibilityDuration = 0.5f;

    // 추가적인 능력
    public int IncreasedPenerstratingPower { set; get; } = 0;
    [field:SerializeField]public float IncreasedReboundRecoverPercent { set; get; }

   
    [SerializeField] bool _bounce;
    [SerializeField] float _power;

    [SerializeField] PositionLimit _eyeClose;
    float _eyeCloseElasepdTime;
    float _eyeCloseTime = 4;

    bool _isSliding;
    bool _isRun;
    bool _isFire;
    public bool IsFire => _isFire;

    float _runToFireCoolTime = 0.2f;
    float _runToFireElaspedTime = 0f;


    IInteractable _interactableOjbect;
    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _rigidbody = GetComponent<Rigidbody2D>();
        _character = GetComponent<Character>();
        _weaponSwaper = GetComponent<WeaponSwaper>();
        _weaponSwaper.Init();

        _backArmInitPosition = _backArmRoot.transform.localPosition;
        for (int i = 0; i < 3; i++)
        {
            if (_weaponSwaper.GetWeapon(i) != null)
            {
                _weaponSwaper.SelectWeapon(i);
                break;
            }
        }
        _cameraController = Camera.main.GetComponent<CameraController>();
        _girlAbility.Init(this);

        _initHeadAngle = _head.transform.rotation.eulerAngles.z;
        _initBodyAngle = _body.transform.rotation.eulerAngles.z;
        _initFrontArmAngle = _frontArm.transform.rotation.eulerAngles.z;

        _character.CharacterDeadHandler += OnCharacterDead;

        Managers.GetManager<InputManager>().MouseButtonDownHandler += UseWeapon;
        Managers.GetManager<InputManager>().MouseButtonHoldHandler += AutoUseWeapon;
        Managers.GetManager<InputManager>().MouseButtonUpHandler += UnuseWeapon;
        Managers.GetManager<InputManager>().ReloadKeyDownHandler += OnReloadKeyDown;
        Managers.GetManager<InputManager>().ReloadKeyHoldHandler += OnReloadKeyHold;
        Managers.GetManager<InputManager>().ReloadKeyUpHandler += OnReloadKeyUp;
        HandleMove();

        Managers.GetManager<InputManager>().Num1KeyDownHandler += () => _weaponSwaper.SelectWeapon(0);
        Managers.GetManager<InputManager>().Num2KeyDownHandler += () => _weaponSwaper.SelectWeapon(1);
        Managers.GetManager<InputManager>().Num3KeyDownHandler += () => _weaponSwaper.SelectWeapon(2);
        Managers.GetManager<InputManager>().JumpKeyDownHandler += OnJumpKeyDown;

        _isFire = false;

    }
    private void OnDrawGizmos()
    {
        if (!_debug) return;

        Util.DrawRangeOnGizmos(gameObject, _meleeAttackRange, Color.red);
    }

    private void OnApplicationPause(bool pause)
    {
        if(pause)
            _isFire = false;
    }

    private void OnCharacterDead()
    {
        _character.SetAnimatorTrigger("Dead");
        Managers.GetManager<GameManager>().GameEnding();
    }

    
    private void OnJumpKeyDown()
    {
        if(_isSliding) return;

        _character.Jump();
    }

    private void OnReloadKeyDown()
    {
        if (_weaponSwaper.CurrentWeapon == null) return;

      
         _weaponSwaper.CurrentWeapon.Reload(this);
    }

    private void OnReloadKeyHold()
    {
        if (_weaponSwaper.CurrentWeapon == null) return;

        _weaponSwaper.CurrentWeapon.ReloadHold();
    }
    private void OnReloadKeyUp()
    {
        if (_weaponSwaper.CurrentWeapon == null) return;

        _weaponSwaper.CurrentWeapon.ReloadUp();
    }
    public void Update()
    {
        if(Managers.GetManager<GameManager>().IsPlayTimeline) return;
        if (_character == null || _character.IsDead) return;

        if (_bounce)
        {
            _rigidbody.AddForce(Vector2.up * _power,ForceMode2D.Impulse);
            _bounce = false;
        }

        _girlAbility.AbilityUpdate();

        HandleSliding();
        TurnBody();
        RotateArm();
        RotateBody();
        CheckInteractable();

        //// 웨일 테스트
        //if (Input.GetKeyDown(KeyCode.T))
        //{
        //    Vector3? top = Managers.GetManager<GameManager>().GetGroundTop(transform.position);
        //    if (top.HasValue)
        //    {
        //        Effect effect = Managers.GetManager<ResourceManager>().Instantiate<Effect>((int)Define.EffectName.BlackWhale);
        //        effect.SetAttackProperty(_character, 10, 50, 0.2f, Define.CharacterType.Enemy);
        //        effect.SetMultiflySize(3);
        //        effect.Play(top.Value);
        //    }
        //}

        // 눈 깜빡임
        _eyeCloseElasepdTime += Time.deltaTime;
        if(_eyeCloseElasepdTime > _eyeCloseTime)
        {
            _eyeCloseElasepdTime = 0;
            _eyeCloseTime = UnityEngine.Random.Range(3f,4f);
            _eyeClose.PlayRound(3
                );
        }
        // 달리는 중에는 방향에 따라 이동, 걷는 중에는 총방향에 따라서
        if (!_isRun)
        {
            _character.IsTurnBodyAlongVelocity= false;
        }
        // 달리기 -> 총 전환 시 약간의 텀
        _runToFireElaspedTime += Time.deltaTime;
        _isRun = false;


    }

    void CheckInteractable()
    {
        if (Managers.GetManager<UIManager>().IsOpen) return;
        IInteractable closeOne = null;
        float distance = 1000;
        Util.RangeCastAll2D(gameObject, _character.GetSize(), -1, (hit) =>
        {
            if (hit.collider == null) return false;

            IInteractable interactable = hit.collider.GetComponent<IInteractable>();

            if(interactable != null)
            {
                if(closeOne == null || (hit.collider.transform.position - transform.position).magnitude < distance)
                {
                    distance = (hit.collider.transform.position - transform.position).magnitude;
                    closeOne= interactable;
                }
            }
            return false;
        });

        if(closeOne != _interactableOjbect)
        {
            if(_interactableOjbect != null)
                _interactableOjbect.HideBubble();
            _interactableOjbect = closeOne;
            if(_interactableOjbect != null)
                _interactableOjbect.ShowBubble();
        }

        if(Input.GetKeyDown(KeyCode.E))
        {
            if (_interactableOjbect != null)
                _interactableOjbect.Interact();
        }
    }

    void HandleSliding()
    {
        if (!_isSliding && _character.IsContactGround)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift))
            {
                bool isRight = _rigidbody.velocity.x > 0;
                _character.TurnBody(isRight ? Vector3.right : Vector3.left);
                _isSliding = true;
                _character.IsInvincibility = true;
                _character.IsEnableTurn = false;
                _character.IsEnableMove = false;
                _character.AddForce(Vector2.right * 120 * (isRight ? 1 : -1));
                _character.SetAnimatorBool("Sliding", _isSliding);
                _slidingTime = 0;
            }
        }
        else
        {
            _slidingTime += Time.deltaTime;
            if (_invincibilityDuration < _slidingTime)
            {
                _character.IsInvincibility = false;
            }

            if (Mathf.Abs(_rigidbody.velocity.x) < 0.1f)
            {
                _isSliding = false;
                _character.IsInvincibility = false;
                _character.IsEnableMove = true;
                _character.IsEnableTurn = true;
                _character.SetAnimatorBool("Sliding", _isSliding);
            }
        }

    }
    

    public void MeleeAttack()
    {
        Util.RangeCastAll2D(gameObject, _meleeAttackRange, Define.CharacterMask,
            (hit) =>
            {
                Character characrer = hit.collider.GetComponent<Character>();
                if (characrer && characrer.CharacterType == Define.CharacterType.Enemy)
                {
                    _character.Attack(characrer, _character.AttackPower, 100, characrer.transform.position - transform.position, hit.point, 0.2f);
                }
                return true;
            });
    }
  
    public void Rebound(float angle)
    {
        _rebound = angle;

        if (_rebound > 45)
            _rebound = 45;
        
    }

    private void TurnBody()
    {
        if (!_isRun && !_isSliding)
        {
            if (Managers.GetManager<InputManager>().MouseWorldPosition.x < transform.position.x)
                _character.TurnBody(Vector2.left);
            else
                _character.TurnBody(Vector2.right);
        }
    }

    private void RotateBody()
    {
        if(_isRun || _isSliding) return;

        float bodyAngle = 0;
        float headAngle = 0;

        Vector3 distance = Vector3.zero;
        Vector3 mousePos = Managers.GetManager<InputManager>().MouseWorldPosition;
        mousePos.z = 0;

        distance = mousePos - _frontArm.transform.position;
        float angle = _frontArm.transform.rotation.eulerAngles.z - (transform.lossyScale.x > 0? 1 : -1) *_initFrontArmAngle;

        if (angle > 180) angle = -360 + angle;
        if (angle < -180) angle = 360 + angle;

        bodyAngle = angle / 4;
        headAngle = angle / 2;
        _body.transform.rotation = Quaternion.Euler(0, 0, (transform.lossyScale.x > 0 ? bodyAngle + _initBodyAngle : bodyAngle - _initBodyAngle));
        _head.transform.rotation = Quaternion.Euler(0, 0, (transform.lossyScale.x > 0 ? headAngle + _initHeadAngle : headAngle - _initHeadAngle));
    }

    private void RotateArm()
    {
        if (Time.timeScale == 0) return;
        if(_isRun) return;

        Vector3 mousePos = Managers.GetManager<InputManager>().MouseWorldPosition;
     
        if (_rebound > 0)
        {
            if ((_rebound - ReboundRecoverPower * Time.deltaTime) < 0)
            {
                _rebound = 0;
            }
            else
            {
                _rebound -= ReboundRecoverPower  * Time.deltaTime;
            }
        }
        if(_rebound < 0)
        {
            if ((_rebound + ReboundRecoverPower  * Time.deltaTime) > 0)
            {
                _rebound = 0;
            }
            else
            {
                _rebound += ReboundRecoverPower * Time.deltaTime;
            }
        }
      

        Vector3 distance = Vector3.zero;
        mousePos.z = 0;
        distance = mousePos - _frontArm.transform.position;
        float angle = Mathf.Atan2(distance.y, Mathf.Abs(distance.x)) * Mathf.Rad2Deg + _initFrontArmAngle * (transform.lossyScale.x > 0 ? 1 : 1);
        float weaponAngle = 0;

        if (_weaponSwaper.CurrentWeapon)
        {
            angle = 0;
            float armAngle = _frontArm.transform.eulerAngles.z * (transform.lossyScale.x > 0 ? 1 : -1);
            weaponAngle = _weaponSwaper.CurrentWeapon.FirePosition.transform.eulerAngles.z * (transform.lossyScale.x > 0 ? 1 : -1);

            Vector3 firePointToTarget = mousePos - _weaponSwaper.CurrentWeapon.FirePosition.transform.position;
            float firePointToTargetAngle = Mathf.Atan2(firePointToTarget.y, Mathf.Abs(firePointToTarget.x)) * Mathf.Rad2Deg;

            firePointToTargetAngle += _rebound;

            weaponAngle += 90;
            firePointToTargetAngle += 90;

            weaponAngle %= 360;
            firePointToTargetAngle %= 360;

            if (weaponAngle < -180) weaponAngle = weaponAngle + 360;

            if (firePointToTargetAngle > 180) firePointToTargetAngle = 179f;

            if (Mathf.Abs(Mathf.Abs(firePointToTargetAngle) - Mathf.Abs(weaponAngle) )> 0.1f)
            {
                if (weaponAngle < firePointToTargetAngle)
                {
                    if (firePointToTarget.magnitude > 5)
                    {
                        armAngle += (firePointToTargetAngle - weaponAngle);
                    }
                    else
                    {

                        armAngle += (firePointToTargetAngle - weaponAngle) * Time.deltaTime * 10f;
                    }
                }
                else
                {
                    if (firePointToTarget.magnitude > 5)
                        armAngle -= (weaponAngle - firePointToTargetAngle);
                    else
                        armAngle -= (weaponAngle - firePointToTargetAngle) * Time.deltaTime*10f;
                }
            }
            angle = armAngle ;

        }

        _frontArm.transform.rotation = Quaternion.Euler(0, 0, (transform.lossyScale.x > 0 ? angle  : -angle ));
        if (_weaponSwaper.CurrentWeapon && _weaponSwaper.CurrentWeapon.HandlePosition)
        {
            if (_weaponSwaper.WeaponIndex == 1)
            {
                _backArmRoot.transform.localPosition = new Vector3(0.4009054f, 0.08444482f, 0);
            }
            else
            {
                _backArmRoot.transform.localPosition = _backArmInitPosition ;
            }
            _backArmIK.transform.position = _weaponSwaper.CurrentWeapon.HandlePosition.transform.position;
        }
        float screenWidth = Screen.width;
        Vector3 mousePosition = Input.mousePosition;

        if (mousePosition.x > screenWidth / 4 * 3)
        {
            _cameraController.ExpandsionView(Vector3.right);
        }
        else if (mousePosition.x < screenWidth / 4)
        {
            _cameraController.ExpandsionView(Vector3.left);
        }
    }

    private void HandleMove()
    {
        if (_isRiding) return;

        Managers.GetManager<InputManager>().RightArrowPressedHandler += OnRightArrowPressed;
        Managers.GetManager<InputManager>().LeftArrowPressedHandler += OnLeftArrowPressed;
        Managers.GetManager<InputManager>().UpArrowPressedHandler += OnUpArrowPressed;
        Managers.GetManager<InputManager>().DownArrowPressedHandler += OnDownArrowPressed;

    }

    void OnRightArrowPressed()
    {
        if (_character && _character.IsDead) return;
        if (_character.IsAttack)
        {
            _isRun = false;
            return;
        }


        float run = 1f;
        _character.IsTurnBodyAlongVelocity= true;
        _isRun= true;
        if (_isFire || Input.GetKey(KeyCode.LeftControl))
        {
            run = 0.4f;
            _isRun= false;
            _character.IsTurnBodyAlongVelocity= false;
        }
        else
        {
            _runToFireElaspedTime = 0;
        }
        _character.Move(Vector2.right * run);
    }
    void OnLeftArrowPressed()
    {
        if (_character && _character.IsDead) return;
        if (_character.IsAttack)
        {
            _isRun = false;
            return;
        }

        float run = 1f;
        _character.IsTurnBodyAlongVelocity = true;
        _isRun = true;
        if (_isFire || Input.GetKey(KeyCode.LeftControl))
        {
            run = 0.4f;
            _isRun = false;
            _character.IsTurnBodyAlongVelocity = false;
        }
        else
        {
            _runToFireElaspedTime = 0;
        }
        _character.Move(Vector2.left * run);
    }
    void OnUpArrowPressed()
    {
        if (_character && _character.IsDead) return;
        if (!_character.IsEnableFly) return;
        if (_character.IsAttack)
        {
            _isRun = false;
            return;
        }

        float run = 1f;
        _character.IsTurnBodyAlongVelocity = true;
        _isRun = true;
        if (_isFire || Input.GetKey(KeyCode.LeftControl))
        {
            run = 0.4f;
            _isRun = false;
            _character.IsTurnBodyAlongVelocity = false;
        }
        else
        {
            _runToFireElaspedTime = 0;
        }
        _character.Move(Vector2.up * run);
    }
    void OnDownArrowPressed()
    {
        if (_character && _character.IsDead) return;
        if (!_character.IsEnableFly) return;

        if (_character.IsAttack)
        {
            _isRun = false;
            return;
        }

        float run = 1f;
        _character.IsTurnBodyAlongVelocity = true;
        _isRun = true;
        if (_isFire || Input.GetKey(KeyCode.LeftControl))
        {
            run = 0.4f;
            _isRun = false;
            _character.IsTurnBodyAlongVelocity = false;
        }
        else
        {
            _runToFireElaspedTime = 0;
        }
        _character.Move(Vector2.down * run);
    }

    void UseWeapon()
    {
        if (Time.timeScale == 0) return;
        if (_character && _character.IsDead) return;

        _isFire= true;

        _character.TurnBody(Managers.GetManager<InputManager>().MouseWorldPosition - transform.position);
    
        if (_weaponSwaper.CurrentWeapon != null  && !_weaponSwaper.CurrentWeapon.IsAuto)
            _weaponSwaper.CurrentWeapon.Fire(this);
       
    }
    void AutoUseWeapon()
    {
        if (Time.timeScale == 0) return;
        if (_character && _character.IsDead) return;

        _isFire = true;


        _character.TurnBody(Managers.GetManager<InputManager>().MouseWorldPosition - transform.position);



        if (_weaponSwaper.CurrentWeapon != null && _weaponSwaper.CurrentWeapon.IsAuto)
            _weaponSwaper.CurrentWeapon.Fire(this);
    }
    void UnuseWeapon()
    {
        _isFire = false;
    }
    bool CheckIsCloseEnemy()
    {
        Define.Range range = new Define.Range()
        {
            center = new Vector3(0, 0.5f, 0),
            size = new Vector3(1, 1, 0),
            figureType = Define.FigureType.Box
        };

        List<RaycastHit2D> hits = Util.RangeCastAll2D(gameObject, range, Define.CharacterMask, (hit)=>
        {
            Character character = hit.collider.GetComponent<Character>();
            if (character == null) return false;

            return character.CharacterType == Define.CharacterType.Enemy;
        });

        if (hits.Count<= 0) return false;

        return true;
    }

    
    public void ResetRebound()
    {
        _rebound = 0;
        RotateArm();
    }

    public string SaveData()
    {
        string data = "";


        return data;
    }

    public void PlayRevive()
    {
        _character.SetAnimatorTrigger("Revive");
    }
    public float GetIncreasedAttackSpeedPercentage()
    {
        float percentage = 0;
        percentage += GirlAbility.GetIncreasedAttackSpeedPercentage();

        return percentage;
    }
    public float GetIncreasedAttackPowerPercentage()
    {
        float percentage = 0;
        percentage += GirlAbility.GetIncreasedAttackPowerPercentage();

        return percentage;
    }

    public float GetIncreasedReloadSpeedPercentage()
    {
        float percentage = 0;
        percentage += GirlAbility.GetIncreasedReloadSpeedPercentage();

        return percentage;
    }
}
