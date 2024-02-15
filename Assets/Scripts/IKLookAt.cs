using MoreMountains.Tools;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class IKLookAt : MonoBehaviour
{
    [SerializeField] Player _player;
    [SerializeField] GameObject _eye;
    [SerializeField] GameObject _body;

    float _initAngle;
    float _initBodyAngle;
    private void Awake()
    {
        _player = GetComponentInParent<Player>();
        _initAngle = transform.rotation.eulerAngles.z;
        _initBodyAngle = _body.transform.rotation.eulerAngles.z;
    }

    private void Update()
    {
        Vector3 distance = Managers.GetManager<InputManager>().MouseWorldPosition - _eye.transform.position;

        float angle = 0;
        float bodyAngle = 0;
        if (_player != null && _player.WeaponSwaper.CurrentWeapon != null)
        {
            angle = _player.WeaponSwaper.CurrentWeapon.transform.rotation.eulerAngles.z;
            angle = transform.lossyScale.x > 0 ? angle : -angle;
        }
        else
        {
            angle = Mathf.Atan2(distance.y, Mathf.Abs(distance.x)) * Mathf.Rad2Deg;
        }
        if (angle > 180) angle = -360 + angle;
        if (angle < -180) angle = 360 + angle;

        bodyAngle = angle/2;

        _body.transform.rotation = Quaternion.Euler(0, 0, (transform.lossyScale.x > 0 ? bodyAngle + _initBodyAngle : -bodyAngle - _initBodyAngle));
        transform.rotation = Quaternion.Euler(0, 0, (transform.lossyScale.x > 0 ? angle + _initAngle : -angle - _initAngle));
    }
}
