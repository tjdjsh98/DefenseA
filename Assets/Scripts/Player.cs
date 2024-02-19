using MoreMountains.Tools;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{
    Animator _animator;

    Character _character;
    public Character Character => _character;
    Rigidbody2D _rigidbody;
    Character _ridingCharacter;

    [SerializeField] GameObject _frontArm;
    [SerializeField] GameObject _weaponPoint;
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
    public bool IsHaveRemoveReboundAMoment { set; get; }
    public bool IsHaveFastReload { set; get; }
    public bool IsHaveExtraAmmo { set; get; }
    public bool IsHaveAutoReload { set; get; }
    public int IncreasedPenerstratingPower { set; get; } = 0;
    public bool IsHaveLastShot { set; get; }
    public float DecreasedFireRatePercent { set; get; }
    public float IncreasedReloadSpeedPercent { set; get; }
    public float IncreasedReboundControlPowerPercent { set; get; }
    public float IncreasedReboundRecoverPercent { set; get; }
    public int IncreasedDamage { set; get; }




    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _rigidbody = GetComponent<Rigidbody2D>();
        _character = GetComponent<Character>();
        _weaponSwaper = GetComponent<WeaponSwaper>();
        _cameraController = Camera.main.GetComponent<CameraController>();

        _initHeadAngle = _head.transform.rotation.eulerAngles.z;
        _initBodyAngle = _body.transform.rotation.eulerAngles.z;
        _initFrontArmAngle = _frontArm.transform.rotation.eulerAngles.z;

        Managers.GetManager<GameManager>().Player = this;
        Managers.GetManager<InputManager>().MouseButtonDownHandler += UseWeapon;
        Managers.GetManager<InputManager>().MouseButtonHoldHandler += AutoUseWeapon;
        Managers.GetManager<InputManager>().MouseButtonUpHandler += ShowAim;
        Managers.GetManager<InputManager>().ReloadKeyDownHandler += OnReloadKeyDown;
        HandleMove();

        Managers.GetManager<InputManager>().Num1KeyDownHandler += () => _weaponSwaper.SelectWeapon(0);
        Managers.GetManager<InputManager>().Num2KeyDownHandler += () => _weaponSwaper.SelectWeapon(1);
        Managers.GetManager<InputManager>().Num3KeyDownHandler += () => _weaponSwaper.SelectWeapon(2);
        Managers.GetManager<InputManager>().JumpKeyDownHandler += _character.Jump;

    }

    private void OnReloadKeyDown()
    {
        if (_weaponSwaper.CurrentWeapon == null) return;


        if (IsHaveFastReload)
        {
            _weaponSwaper.CurrentWeapon.FastReload();
        }
        _weaponSwaper.CurrentWeapon.Reload();
    }

    public void Update()
    {
        if(Managers.GetManager<GameManager>().IsPlayTimeline) return;
       
        TurnBody();
        HandleMove();
        RotateArm();
        RotateBody();
        Riding();
        Roll();
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
        _rebound += angle * (1-(ReboundControlPower / (100 + ReboundControlPower)));

        if (_rebound > 45)
            _rebound = 45;
        
    }

    private void TurnBody()
    {
        if (Managers.GetManager<InputManager>().MouseWorldPosition.x < transform.position.x)
            _character.TurnBody(Vector2.left);
        else
            _character.TurnBody(Vector2.right);
    }

    private void RotateBody()
    {
        float bodyAngle = 0;

        Vector3 distance = Vector3.zero;
        Vector3 mousePos = Managers.GetManager<InputManager>().MouseWorldPosition;
        mousePos.z = 0;

        distance = mousePos - _frontArm.transform.position;
        float angle = _frontArm.transform.rotation.eulerAngles.z - (transform.lossyScale.x > 0? 1 : -1) *_initFrontArmAngle;

        if (angle > 180) angle = -360 + angle;
        if (angle < -180) angle = 360 + angle;

        bodyAngle = angle / 2;

        _body.transform.rotation = Quaternion.Euler(0, 0, (transform.lossyScale.x > 0 ? bodyAngle + _initBodyAngle : bodyAngle - _initBodyAngle));
        _head.transform.rotation = Quaternion.Euler(0, 0, (transform.lossyScale.x > 0 ? angle + _initHeadAngle : angle - _initHeadAngle));
    }

    private void RotateArm()
    {
        if (Time.timeScale == 0) return;

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

    }

    void OnRightArrowPressed()
    {
        _character.Move(Vector2.right);
    }
    void OnLeftArrowPressed()
    {
        _character.Move(Vector2.left);
    }

    void UseWeapon()
    {
        if (Time.timeScale == 0) return;

        Managers.GetManager<InputManager>().AimTarget = true;
        if (_weaponSwaper.CurrentWeapon != null && _weaponSwaper.CurrentWeapon.CurrentAmmo > 0 && !_weaponSwaper.CurrentWeapon.IsAuto)
            _weaponSwaper.CurrentWeapon.Fire(_character);
        if (_weaponSwaper.CurrentWeapon != null && _weaponSwaper.CurrentWeapon.CurrentAmmo <= 0 && !_weaponSwaper.CurrentWeapon.IsReload)
        {
            _weaponSwaper.CurrentWeapon.Reload();
        }
    }
    void AutoUseWeapon()
    {
        if (Time.timeScale == 0) return;

        if (_weaponSwaper.CurrentWeapon != null && _weaponSwaper.CurrentWeapon.CurrentAmmo > 0 && _weaponSwaper.CurrentWeapon.IsAuto)
            _weaponSwaper.CurrentWeapon.Fire(_character);

        if (_weaponSwaper.CurrentWeapon != null && _weaponSwaper.CurrentWeapon.CurrentAmmo <= 0 && !_weaponSwaper.CurrentWeapon.IsReload)
        {
            _weaponSwaper.CurrentWeapon.Reload();
        }

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
                            transform.position = go.GetComponent<DogAI>().SitPosition.transform.position;
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
}
