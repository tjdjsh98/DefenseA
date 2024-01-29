using System;
using System.Collections;
using System.Collections.Generic;
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
    private void Awake()
    {
        _character= GetComponent<Character>();
        _weaponSwaper = GetComponent<WeaponSwaper>();
        _cameraController = Camera.main.GetComponent<CameraController>();

        Managers.GetManager<GameManager>().SetPlayer(this);
        Managers.GetManager<InputManager>().MouseButtonHold += UseWeapon;
        Managers.GetManager<InputManager>().Num1KeyDown += UseWeapon;
        HandleMove();
        Managers.GetManager<UIManager>().GetUI<UIInGame>().SetPlayerCharacter(this);

        _initCameraPosition = Camera.main.transform.position;
    }

    public void Update()
    {
        RotateArm();
        HandleMove();
    }

    private void RotateArm()
    {
        Vector3 distance = Managers.GetManager<InputManager>().MouseWorldPosition - _arm.transform.position;

        float angle = Mathf.Atan2(distance.y, distance.x) * Mathf.Rad2Deg;

        if (transform.localScale.x > 0)
        {
            if ((angle >= 0 && angle <= 70) || (angle >= -70 && angle <= 0))
            {
                _arm.transform.rotation = Quaternion.Euler(0, 0, angle);

            }
        }
        else
        {
            angle += 180f;
            if ((angle >= 0 && angle <= 70) || (angle >= 270 && angle <= 340))
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
        _weaponSwaper.CurrentWeapon.Fire(_character);
    }
}
