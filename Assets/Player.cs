using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    Character _character;
    [SerializeField] Weapon _weapon;

    private void Awake()
    {
        Managers.GetManager<InputManager>().MouseButtonDown += UseWeapon;
        _character= GetComponent<Character>();
    }

    public void Update()
    {
        RotateWeapon();
    }

    private void RotateWeapon()
    {
        Vector3 distance = Managers.GetManager<InputManager>().MouseWorldPosition - _weapon.transform.position;

        float angle = Mathf.Atan2(distance.y, distance.x) * Mathf.Rad2Deg;
         
        _weapon.transform.rotation = Quaternion.Euler(0, 0, angle);
    }

    void UseWeapon()
    {
        _weapon.Fire(_character);
    }
}
