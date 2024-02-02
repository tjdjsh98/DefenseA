using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

public class Player : MonoBehaviour
{
    Character _character;
    public Character Character => _character;
    Rigidbody2D _rigidbody;
    Character _ridingCharacter;

    [SerializeField] GameObject _frontArm;
    [SerializeField] GameObject _backArm;
    [SerializeField] GameObject _weaponPoint;
    [SerializeField] GameObject _head;
    [SerializeField] GameObject _body;

    float _initHeadAngle;
    float _initBodyAngle;
    float _initFrontArmAngle;
    float _initBackArmAngle;

    WeaponSwaper _weaponSwaper;
    public WeaponSwaper WeaponSwaper => _weaponSwaper;

    [SerializeField] float _rightPadding;
    [SerializeField] float _upPadding;

    Vector3 _initCameraPosition;

    CameraController _cameraController;

    float _rebound;
    float _reboundControlPower = 100f;
    public float ReboundControlPower => _reboundControlPower;

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
        _initFrontArmAngle = _frontArm.transform.localRotation.eulerAngles.z;
        _initBackArmAngle = _backArm.transform.rotation.eulerAngles.z;

        Managers.GetManager<GameManager>().Player = this;
        Managers.GetManager<InputManager>().MouseButtonHold += UseWeapon;
        Managers.GetManager<InputManager>().ReloadKeyDown += OnReloadKeyDown;
        HandleMove();
        Managers.GetManager<UIManager>().GetUI<UIInGame>().SetPlayerCharacter(this);

        _initCameraPosition = Camera.main.transform.position;

        Managers.GetManager<InputManager>().Num1KeyDown += () => _weaponSwaper.SelectWeapon(0);
        Managers.GetManager<InputManager>().Num2KeyDown += () => _weaponSwaper.SelectWeapon(1);
        Managers.GetManager<InputManager>().Num3KeyDown += () => _weaponSwaper.SelectWeapon(2);

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
        _rebound += angle;
        if (_rebound > 45)
            _rebound = 45;
    }

    private void TurnBody()
    {
        if (Input.mousePosition.x < Screen.width / 2)
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

        distance = mousePos - _backArm.transform.position;
        float angle = _backArm.transform.rotation.eulerAngles.z - (transform.lossyScale.x > 0? 1 : -1) *_initBackArmAngle;

        if (angle > 180) angle = -360 + angle;
        if (angle < -180) angle = 360 + angle;

        bodyAngle = angle / 2;

        _body.transform.rotation = Quaternion.Euler(0, 0, (transform.lossyScale.x > 0 ? bodyAngle + _initBodyAngle : bodyAngle - _initBodyAngle));
        _head.transform.rotation = Quaternion.Euler(0, 0, (transform.lossyScale.x > 0 ? angle + _initHeadAngle : angle - _initHeadAngle));
    }

    private void RotateArm()
    {
        if (!Input.GetMouseButton(0))
        {
            if (_rebound > 0)
            {
                _rebound -= _reboundControlPower * Time.deltaTime;
                if (_rebound < 0)
                    _rebound = 0;
            }
        }
        else
        {
            if (_rebound > 0)
            {
                _rebound -= _reboundControlPower / 2 * Time.deltaTime;
                if (_rebound < 0)
                    _rebound = 0;
            }
        }

        Vector3 distance = Vector3.zero;
        Vector3 mousePos = Managers.GetManager<InputManager>().MouseWorldPosition;
        mousePos.z = 0;

        distance = mousePos - _backArm.transform.position;
        float angle = Mathf.Atan2(distance.y, Mathf.Abs(distance.x)) * Mathf.Rad2Deg;

        if (angle > 180) angle = -360 + angle;
        if (angle < -180) angle = 360 + angle;
        angle += _rebound;

        if (angle >= 90) angle = 89;

        _backArm.transform.rotation = Quaternion.Euler(0, 0, (transform.lossyScale.x > 0 ? angle + _initBackArmAngle : -angle - _initBackArmAngle));

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
        if (_weaponSwaper.CurrentWeapon != null)
            _weaponSwaper.CurrentWeapon.Fire(_character);
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
