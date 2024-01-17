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

    private void Awake()
    {
        _character= GetComponent<Character>();
        _weaponSwaper = GetComponent<WeaponSwaper>();

        Managers.GetManager<GameManager>().SetPlayer(this);
        Managers.GetManager<InputManager>().MouseButtonDown += UseWeapon;
        Managers.GetManager<UIManager>().GetUI<UIInGame>().SetPlayerCharacter(this);
    }

    public void Update()
    {
        RotateArm();
    }

    private void RotateArm()
    {
        Vector3 distance = Managers.GetManager<InputManager>().MouseWorldPosition - _arm.transform.position;

        float angle = Mathf.Atan2(distance.y, distance.x) * Mathf.Rad2Deg;

        _arm.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void UseWeapon()
    {
        _weaponSwaper.CurrentWeapon.Fire(_character);
    }
}
