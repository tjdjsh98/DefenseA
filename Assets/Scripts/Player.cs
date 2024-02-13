using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class Player : MonoBehaviour
{
    Character _character;
    public Character Character => _character;
    Rigidbody2D _rigidbody;
    Character _ridingCharacter;

    [SerializeField] GameObject _frontArm;
    [SerializeField] GameObject _weaponPoint;
    [SerializeField] GameObject _head;
    [SerializeField] GameObject _body;

    public int PenerstratingPower { set; get; } = 0;
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
    public float ReboundControlPower => _reboundControlPower;

    [SerializeField]float _reboundRecoverPower = 10f;
    float _reboundRecoverTime = 0;
    bool _isRiding;

    Animator _animator;

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
        Managers.GetManager<InputManager>().MouseButtonDown += UseWeapon;
        Managers.GetManager<InputManager>().MouseButtonHold += AutoUseWeapon;
        Managers.GetManager<InputManager>().MouseButtonUp += ShowAim;
        Managers.GetManager<InputManager>().ReloadKeyDown += OnReloadKeyDown;
        HandleMove();
        Managers.GetManager<UIManager>().GetUI<UIInGame>().SetPlayerCharacter(this);

        Managers.GetManager<InputManager>().Num1KeyDown += () => _weaponSwaper.SelectWeapon(0);
        Managers.GetManager<InputManager>().Num2KeyDown += () => _weaponSwaper.SelectWeapon(1);
        Managers.GetManager<InputManager>().Num3KeyDown += () => _weaponSwaper.SelectWeapon(2);
        Managers.GetManager<InputManager>().JumpKeyDown += _character.Jump;

    }

    private void OnReloadKeyDown()
    {
        if (_weaponSwaper.CurrentWeapon == null) return;

        _weaponSwaper.CurrentWeapon.Reload();
    }

    public void Update()
    {
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
        _rebound += angle * (1-(_reboundControlPower / (100 + _reboundControlPower)));
        
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
        Vector3 mousePos = Managers.GetManager<InputManager>().MouseWorldPosition;
        bool isPressed = false;

        if (Input.GetMouseButton(0))
        {
            isPressed = true;
            _reboundRecoverTime = 0;
        }
        _reboundRecoverTime += Time.deltaTime*3;
        if (_rebound > 0)
        {
            if ((_rebound - _reboundRecoverPower * _reboundRecoverTime * Time.deltaTime) < 0)
            {
                _rebound = 0;
            }
            else
            {
                _rebound -= _reboundRecoverPower * _reboundRecoverTime * Time.deltaTime;
            }
        }
        if(!isPressed && _rebound < 0)
        {
            if ((_rebound + _reboundRecoverPower * _reboundRecoverTime * Time.deltaTime) > 0)
            {
                _rebound = 0;
            }
            else
            {
                _rebound += _reboundRecoverPower * _reboundRecoverTime * Time.deltaTime;
            }
        }
        if (isPressed)
        {
            _rebound += Managers.GetManager<InputManager>().MouseDelta.y;
        }

        Vector3 distance = Vector3.zero;
        mousePos.z = 0;


        distance = mousePos - _frontArm.transform.position;
        float angle = Mathf.Atan2(distance.y, Mathf.Abs(distance.x)) * Mathf.Rad2Deg;

        if (angle > 180) angle = -360 + angle;
        if (angle < -180) angle = 360 + angle;
        angle += _rebound;

        if (angle >= 90) angle = 89;
        if (angle <= -90) angle = -89;
        _frontArm.transform.rotation = Quaternion.Euler(0, 0, (transform.lossyScale.x > 0 ? angle + _initFrontArmAngle : -angle - _initFrontArmAngle));

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
        Managers.GetManager<InputManager>().RightArrowPressed += OnRightArrowPressed;
        Managers.GetManager<InputManager>().LeftArrowPressed += OnLeftArrowPressed;

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
                GameObject[] gos = Util.BoxcastAll2D(gameObject, new Define.Range { center = new Vector3(0, 0), size = Vector3.one });

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
                            _ridingCharacter.IsEnableMove = true;
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
                _ridingCharacter.IsEnableMove = false;
                transform.SetParent(transform.parent.parent);
                _rigidbody.isKinematic = false;
                _character.AnimatorSetBool("Sit", false);
            }
        }
    }
}
