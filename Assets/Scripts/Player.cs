using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    Animator _animator;

    Character _character;
    public Character Character => _character;
    Rigidbody2D _rigidbody;
    Character _ridingCharacter;

    [SerializeField] GameObject _frontArm;
    [SerializeField] GameObject _backArmIK;
    [SerializeField] GameObject _head;
    [SerializeField] GameObject _body;

    public int ReduceReloadTime { set; get; } = 0;

    float _initHeadAngle;
    float _initBodyAngle;
    float _initFrontArmAngle;

    WeaponSwaper _weaponSwaper;
    public WeaponSwaper WeaponSwaper => _weaponSwaper;

    [SerializeField] float _rightPadding;
    [SerializeField] float _upPadding;

    CameraController _cameraController;

    float _rebound;
    float _reboundControlPower = 50f;
    public float ReboundControlPower => IncreasedReboundControlPowerPercent > 0 ? _reboundControlPower * (1 + IncreasedReboundControlPowerPercent / 100) : _reboundControlPower / (1 - IncreasedReboundControlPowerPercent / 100);

    [SerializeField]float _reboundRecoverPower = 10f;
    public float ReboundRecoverPower => IncreasedReboundRecoverPercent > 0 ? _reboundRecoverPower * (1 + IncreasedReboundRecoverPercent / 100) : _reboundRecoverPower / (1 - IncreasedReboundRecoverPercent / 100);
    float _reboundRecoverTime = 0;
    bool _isRiding;

    // 추가적인 능력
    public int IncreasedPenerstratingPower { set; get; } = 0;
    public float DecreasedFireDelayPercent { set; get; }
    public float IncreasedAttackSpeedPercentage { set; get; }
    public float IncreasedReloadSpeedPercent { set; get; }
    public float IncreasedReboundControlPowerPercent { set; get; }
    [field:SerializeField]public float IncreasedReboundRecoverPercent { set; get; }

    float _plentyOfBulletsIncreasedAttackPointPercentage;
    float _maxElectric = 10;
    public float MaxElectric => _maxElectric;
    public float CurrentElectric { set;get; }
    

    public float IncreaseAttackPowerPercentage => _plentyOfBulletsIncreasedAttackPointPercentage;
    // 능력 언락부분
    [field: SerializeField] public bool IsUnlockBlackSphere { get; set; } = false;
    public bool IsHaveRemoveReboundAMoment { set; get; }
    public bool IsUnlockFastReload { set; get; }
    public bool IsUnlockExtraAmmo { set; get; }
    public bool IsUnlockAutoReload { set; get; } = true;
    public bool IsUnlockLastShot { set; get; }
    [field: SerializeField] public bool IsUnlockPlentyOfBullets { set; get; }
    [field:SerializeField]public bool IsUnlockElectric { set; get; }
    [field:SerializeField]public bool IsUnlockLastStruggling { set; get; }

    // 자동장전
    List<float> _autoReloadElaspedTimeList = new List<float>();

    [SerializeField] bool _bounce;
    [SerializeField] float _power;

    [SerializeField] PositionLimit _eyeClose;
    float _eyeCloseElasepdTime;
    float _eyeCloseTime = 4;

    bool _isRun;
    bool _isFire;

    float _runToFireCoolTime = 0.2f;
    float _runToFireElaspedTime = 0f;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _rigidbody = GetComponent<Rigidbody2D>();
        _character = GetComponent<Character>();
        _weaponSwaper = GetComponent<WeaponSwaper>();
        _cameraController = Camera.main.GetComponent<CameraController>();

        _character.AttackHandler += OnAttack;

        _initHeadAngle = _head.transform.rotation.eulerAngles.z;
        _initBodyAngle = _body.transform.rotation.eulerAngles.z;
        _initFrontArmAngle = _frontArm.transform.rotation.eulerAngles.z;

        _character.CharacterDead += OnCharacterDead;

        Managers.GetManager<GameManager>().Player = this;
        Managers.GetManager<InputManager>().MouseButtonDownHandler += UseWeapon;
        Managers.GetManager<InputManager>().MouseButtonHoldHandler += AutoUseWeapon;
        Managers.GetManager<InputManager>().MouseButtonUpHandler += ShowAim;
        Managers.GetManager<InputManager>().ReloadKeyDownHandler += OnReloadKeyDown;
        Managers.GetManager<InputManager>().ReloadKeyHoldHandler += OnReloadKeyHold;
        Managers.GetManager<InputManager>().ReloadKeyUpHandler += OnReloadKeyUp;
        HandleMove();

        Managers.GetManager<InputManager>().Num1KeyDownHandler += () => _weaponSwaper.SelectWeapon(0);
        Managers.GetManager<InputManager>().Num2KeyDownHandler += () => _weaponSwaper.SelectWeapon(1);
        Managers.GetManager<InputManager>().Num3KeyDownHandler += () => _weaponSwaper.SelectWeapon(2);
        Managers.GetManager<InputManager>().JumpKeyDownHandler += _character.Jump;

    }

    private void OnCharacterDead()
    {
        SceneManager.LoadScene("MainMenu");
    }

    private void OnReloadKeyDown()
    {
        if (_weaponSwaper.CurrentWeapon == null) return;


        if (IsUnlockFastReload)
        {
            _weaponSwaper.CurrentWeapon.FastReload();
        }
        _weaponSwaper.CurrentWeapon.Reload();
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
       
        if (_bounce)
        {
            _rigidbody.AddForce(Vector2.up * _power,ForceMode2D.Impulse);
            _bounce = false;
        }

        TurnBody();
        RotateArm();
        RotateBody();
        Riding();
        Roll();
        AutoReload();
        PlentyOfBullets();
        Electric();
        _eyeCloseElasepdTime += Time.deltaTime;
        if(_eyeCloseElasepdTime > _eyeCloseTime)
        {
            _eyeCloseElasepdTime = 0;
            _eyeCloseTime = UnityEngine.Random.Range(6f,7f);
            _eyeClose.PlayRound(3
                );
        }
        if (!_isRun)
        {
            _character.IsTurnBodyAlongVelocity= false;
        }
        _runToFireElaspedTime += Time.deltaTime;
        _isRun = false;
        _isFire = false;
    }

    public float GetIncreasedDamagePercentage()
    {
        Character wall = Managers.GetManager<GameManager>().Wall;
        Character creature = Managers.GetManager<GameManager>().Creature;

        float percentage = 0;
        percentage += _plentyOfBulletsIncreasedAttackPointPercentage;

        if (IsUnlockLastStruggling && (wall == null || wall.IsDead) && (creature == null || creature.IsDead))
            percentage += 100f;
             
        return percentage;
    }

    public float GetIncreasedAttackSpeedPercentage()
    {
        Character wall = Managers.GetManager<GameManager>().Wall;
        Character creature = Managers.GetManager<GameManager>().Creature;

        float percentage = 0;

        percentage += IncreasedAttackSpeedPercentage;

        if (IsUnlockLastStruggling && (wall == null || wall.IsDead) && (creature == null || creature.IsDead))
            percentage += 100f;

        return percentage;
    }

    private void Electric()
    {
        if (!IsUnlockElectric) return;

        if(_maxElectric > CurrentElectric)
            CurrentElectric += Time.deltaTime * 0.1f;
        else
            CurrentElectric = _maxElectric;
    }

    void PlentyOfBullets()
    {
        if (!IsUnlockPlentyOfBullets) return;

        int value = _weaponSwaper.CurrentWeapon.MaxAmmo / 10;
        _plentyOfBulletsIncreasedAttackPointPercentage = value * 10f;
    }

    void OnAttack(Character target, int damage)
    {
        if (IsUnlockBlackSphere)
        {
            // 20%의 확률로
            if (Random.Range(0, 5) == 0)
            {
                GameObject go = Managers.GetManager<ResourceManager>().Instantiate("Prefabs/BlackSphere");
                go.transform.position = target.transform.position;
                BlackSphere blackSphere = go.GetComponent<BlackSphere>();
                blackSphere?.Init(_character);
            }
        }

    }
    private void AutoReload()
    {
        if (!IsUnlockAutoReload) return;

        for(int i = 0;i < _weaponSwaper.GetWeaponCount(); i++)
        {
            if (_autoReloadElaspedTimeList.Count <= i)
                _autoReloadElaspedTimeList.Add(0);
            if (_weaponSwaper.WeaponIndex == i)
            {
                _autoReloadElaspedTimeList[i] = 0;
                continue;
            }

            Weapon weapon = _weaponSwaper.GetWeapon(i);
            if (weapon== null) continue;

            if (weapon.CurrentAmmo < weapon.MaxAmmo)
            {
                if (_autoReloadElaspedTimeList[i] > weapon.ReloadDelay)
                {
                    weapon.CompleteReload();
                    _autoReloadElaspedTimeList[i] = 0;
                }
                else
                {
                    _autoReloadElaspedTimeList[i] += Time.deltaTime;
                }
            }
        }
    }

    public void SetReboundControlPower(float power)
    {
        _reboundControlPower = power;
    }

    void Roll()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (!_character.IsStun)
            {
                _character.AnimatorSetTrigger("Roll");
            }
        }
    }
    public void Rebound(float angle)
    {
        _rebound = angle;

        if (_rebound > 45)
            _rebound = 45;
        
    }

    private void TurnBody()
    {
        if (!_isRun)
        {
            if (Managers.GetManager<InputManager>().MouseWorldPosition.x < transform.position.x)
                _character.TurnBody(Vector2.left);
            else
                _character.TurnBody(Vector2.right);
        }
    }

    private void RotateBody()
    {
        if(_isRun) return;

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
        bool isPressed = false;

        if (Input.GetMouseButton(0))
        {
            isPressed = true;
            _reboundRecoverTime = 0.5f;
        }
        _reboundRecoverTime += Time.deltaTime*3;
        if (_rebound > 0)
        {
            if ((_rebound - ReboundRecoverPower * _reboundRecoverTime * Time.deltaTime) < 0)
            {
                _rebound = 0;
            }
            else
            {
                _rebound -= ReboundRecoverPower * _reboundRecoverTime * Time.deltaTime;
            }
        }
        if(!isPressed && _rebound < 0)
        {
            if ((_rebound + ReboundRecoverPower * _reboundRecoverTime * Time.deltaTime) > 0)
            {
                _rebound = 0;
            }
            else
            {
                _rebound += ReboundRecoverPower * _reboundRecoverTime * Time.deltaTime;
            }
        }
        if (isPressed)
        {
            _rebound += Managers.GetManager<InputManager>().MouseDelta.y;
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
                        armAngle += (firePointToTargetAngle - weaponAngle);
                    else
                        armAngle += (firePointToTargetAngle - weaponAngle) * Time.deltaTime*10f;
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

        if(_weaponSwaper.CurrentWeapon && _weaponSwaper.CurrentWeapon.HandlePosition)
            _backArmIK.transform.position = _weaponSwaper.CurrentWeapon.HandlePosition.transform.position;
        float screenWidth = Screen.
            width;
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

    }

    void OnRightArrowPressed()
    {
        float run = 0.4f;
        if (!_isFire && Input.GetKey(KeyCode.LeftShift))
        {
            run = 1f;
            _character.IsTurnBodyAlongVelocity= true;
            _isRun= true;
            _runToFireElaspedTime = 0;
        }
        _character.Move(Vector2.right * run);
    }
    void OnLeftArrowPressed()
    {
        float run = 0.4f;
        if (!_isFire && Input.GetKey(KeyCode.LeftShift))
        {
            run = 1f;
            _character.IsTurnBodyAlongVelocity= true;
            _isRun = true;
            _runToFireElaspedTime = 0;
        }
        _character.Move(Vector2.left * run);
    }

    void UseWeapon()
    {
        if (Time.timeScale == 0) return;

        _isFire= true;
        if (_runToFireElaspedTime < _runToFireCoolTime) return;

        Managers.GetManager<InputManager>().AimTarget = true;
        if (_weaponSwaper.CurrentWeapon != null  && !_weaponSwaper.CurrentWeapon.IsAuto)
            _weaponSwaper.CurrentWeapon.Fire(_character);
       
    }
    void AutoUseWeapon()
    {
        if (Time.timeScale == 0) return;

        _isFire= true;
        if (_runToFireElaspedTime < _runToFireCoolTime) return;

        if (_weaponSwaper.CurrentWeapon != null && _weaponSwaper.CurrentWeapon.IsAuto)
            _weaponSwaper.CurrentWeapon.Fire(_character);

    }
    void ShowAim()
    {

        Managers.GetManager<InputManager>().AimTarget = false;
    }

    void Riding()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (_isRiding == false)
            {
                GameObject[] gos = Util.RangeCastAll2D(gameObject, new Define.Range { center = new Vector3(0, 0), size = Vector3.one });

                if (gos.Length > 0)
                {
                    foreach (var go in gos)
                    {
                        if (go.gameObject.name.Equals("Dog"))
                        {
                            transform.SetParent(go.transform);
                            transform.position = go.GetComponent<WallAI>().SitPosition.transform.position;
                            _isRiding = true;
                            _character.AnimatorSetBool("Sit", true);
                            _ridingCharacter = go.GetComponent<Character>();
                            _character.IsEnableMove = false;
                            _rigidbody.isKinematic = true;
                        }
                    }
                }
            }
            else
            {
                _isRiding = false;
                _character.IsEnableMove = true;
                transform.SetParent(transform.parent.parent);
                _rigidbody.isKinematic = false;
                _character.AnimatorSetBool("Sit", false);
            }
        }
    }

    public void ResetRebound()
    {
        _rebound = 0;
        RotateArm();
    }
}
