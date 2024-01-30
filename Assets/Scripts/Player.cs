using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Player : MonoBehaviour
{
    Character _character;
    public Character Character=> _character;

    [SerializeField] GameObject _arm;

    WeaponSwaper _weaponSwaper;
    public WeaponSwaper WeaponSwaper => _weaponSwaper;

    [SerializeField] float _rightPadding;
    [SerializeField] float _upPadding;

    Vector3 _initCameraPosition;

    CameraController _cameraController;

    float _outAngle;
    float _outAngleControlPower = 100f;

    bool _isRiding;

    Rigidbody2D _rigidbody;
    private void Awake()
    {
        _rigidbody= GetComponent<Rigidbody2D>();
        _character= GetComponent<Character>();
        _weaponSwaper = GetComponent<WeaponSwaper>();
        _cameraController = Camera.main.GetComponent<CameraController>();

        Managers.GetManager<GameManager>().SetPlayer(this);
        Managers.GetManager<InputManager>().MouseButtonHold += UseWeapon;
        Managers.GetManager<InputManager>().Num1KeyDown += UseWeapon;
        HandleMove();
        Managers.GetManager<UIManager>().GetUI<UIInGame>().SetPlayerCharacter(this);

        _initCameraPosition = Camera.main.transform.position;

        Managers.GetManager<InputManager>().Num1KeyDown += () => _weaponSwaper.SelectWeapon(0);
        Managers.GetManager<InputManager>().Num2KeyDown += () => _weaponSwaper.SelectWeapon(1);
        Managers.GetManager<InputManager>().Num3KeyDown += () => _weaponSwaper.SelectWeapon(2);
    }

    public void Update()
    {
        TurnBody();
        RotateArm();
        HandleMove();
        Riding();
    }

    public void OutAngle(float angle)
    {
        _outAngle+= angle;
        if (_outAngle > 45)
            _outAngle = 45;
    }

    private void TurnBody()
    {
        Vector3 scale =  transform.localScale;
        if (Input.mousePosition.x < Screen.width / 2)
            scale.x = -Math.Abs(scale.x);
        else
            scale.x = Math.Abs(scale.x);

        transform.localScale = scale;   
    }

    private void RotateArm()
    {
        if (!Input.GetMouseButton(0))
        {
            if (_outAngle > 0)
            {
                _outAngle -= _outAngleControlPower * Time.deltaTime;
                if (_outAngle < 0)
                    _outAngle = 0;
            }
        }
        else
        {
            if (_outAngle > 0)
            {
                _outAngle -= _outAngleControlPower/2 * Time.deltaTime;
                if (_outAngle < 0)
                    _outAngle = 0;
            }
        }

        Vector3 distance = Managers.GetManager<InputManager>().MouseWorldPosition - _arm.transform.position;

        float angle = Mathf.Atan2(distance.y, distance.x) * Mathf.Rad2Deg;
        if (transform.localScale.x > 0)
        {
            if ((angle >= 0 && angle <= 70) || (angle >= -70 && angle <= 0))
            {
                angle += _outAngle;
                _arm.transform.rotation = Quaternion.Euler(0, 0, angle);

            }
        }
        else
        {
            angle += 180f;
            if ((angle >= 0 && angle <= 70))
            {
                angle -= _outAngle;
            }
            if ((angle >= 270 && angle <= 360))
            {
                angle -= _outAngle;
            }
            _arm.transform.rotation = Quaternion.Euler(0, 0, angle);

        }

        float screenWidth = Screen.width;
        Vector3 mousePosition = Input.mousePosition;

        if(mousePosition.x > screenWidth/4*3)
        {
            _cameraController.ExpandsionView(Vector3.right);
        }
        else if(mousePosition.x < screenWidth / 4)
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
        if(_weaponSwaper.CurrentWeapon != null)
            _weaponSwaper.CurrentWeapon.Fire(_character);
    }

    void Riding()
    {
        if(Input.GetKeyDown(KeyCode.E))
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
                            transform.localPosition = Vector3.up;
                            _isRiding = true;
                            _rigidbody.isKinematic = true;
                        }
                    }
                }
            }
            else
            {
                _isRiding = false;
                transform.SetParent(transform.parent.parent);
                _rigidbody.isKinematic = false;
            }
        }
    }
}
