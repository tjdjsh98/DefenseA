using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSwaper : MonoBehaviour
{
    Weapon _currentWeapon;
    public Weapon CurrentWeapon=> _currentWeapon;
    [SerializeField]List<Weapon> _weaponList;
    public List<Weapon> WeaponList=>_weaponList;
    public Action<Weapon> WeaponSwaped;

    int _weaponIndex;
    public int WeaponIndex => _weaponIndex;

    private void Awake()
    {
        SelectWeapon(0);

        Managers.GetManager<InputManager>().Num1KeyDown += () => SelectWeapon(0);
        Managers.GetManager<InputManager>().Num2KeyDown += () => SelectWeapon(1);
    }

    public void SelectWeapon(int index)
    {
        if (index < 0 || index >= _weaponList.Count) return;

        _currentWeapon = _weaponList[index];
        _weaponIndex = index;

        foreach(var weapon in _weaponList)
        {
            if (weapon == _currentWeapon)
            {
                weapon.gameObject.SetActive(true);
                continue;
            }
            weapon.gameObject.SetActive(false);
        }

        WeaponSwaped?.Invoke(_currentWeapon);
    }

}
