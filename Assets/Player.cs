using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    Character _character;
    public Character Character=> _character;
    WeaponSwaper _weaponSwaper;
    public WeaponSwaper WeaponSwaper => _weaponSwaper;

    private void Awake()
    {
        _character= GetComponent<Character>();
        _weaponSwaper = GetComponent<WeaponSwaper>();



        Managers.GetManager<InputManager>().MouseButtonDown += UseWeapon;
        Managers.GetManager<UIManager>().GetUI<UIInGame>().SetPlayerCharacter(this);
    }

    public void Update()
    {
        RotateWeapon();
    }

    private void RotateWeapon()
    {
        Vector3 distance = Managers.GetManager<InputManager>().MouseWorldPosition - _weaponSwaper.CurrentWeapon.transform.position;

        float angle = Mathf.Atan2(distance.y, distance.x) * Mathf.Rad2Deg;

        _weaponSwaper.CurrentWeapon.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void UseWeapon()
    {
        _weaponSwaper.CurrentWeapon.Fire(_character);
    }
}
