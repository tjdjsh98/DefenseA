using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSwaper : MonoBehaviour
{
    Weapon _currentWeapon;

    [SerializeField] Weapon _mainWeapon;
    [SerializeField] Weapon _subWeapon;
    [SerializeField] Weapon _meleeWeapon;
    public Weapon CurrentWeapon=> _currentWeapon;
    public Action<Weapon> WeaponSwaped;

    int _weaponIndex;
    public int WeaponIndex => _weaponIndex;

    private void Awake()
    {
        SelectWeapon(0);

        Managers.GetManager<InputManager>().Num1KeyDown += () => SelectWeapon(0);
        Managers.GetManager<InputManager>().Num2KeyDown += () => SelectWeapon(1);
        Managers.GetManager<InputManager>().Num3KeyDown += () => SelectWeapon(2);
    }

    public void SelectWeapon(int index)
    {
        if (index < 0 || index >= 3) return;

        if (index == 0)
            _currentWeapon = _mainWeapon;
        else if (index == 1)
            _currentWeapon = _subWeapon;
        else if (index == 2)
            _currentWeapon = _meleeWeapon;

        _mainWeapon.gameObject.SetActive(_currentWeapon == _mainWeapon);
        _subWeapon.gameObject.SetActive(_currentWeapon == _subWeapon);
        _meleeWeapon.gameObject.SetActive(_currentWeapon == _meleeWeapon);

        WeaponSwaped?.Invoke(_currentWeapon);
    }

}
