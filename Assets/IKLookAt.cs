using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class IKLookAt : MonoBehaviour
{
    [SerializeField] GameObject _eye;
    [SerializeField] GameObject _body;

    float _initAngle;
    float _initBodyAngle;
    private void Awake()
    {
        _initAngle = transform.localRotation.eulerAngles.z;
        _initBodyAngle = _body.transform.localRotation.eulerAngles.z;
    }

    private void Update()
    {
        Vector3 distance = Managers.GetManager<InputManager>().MouseWorldPosition - _eye.transform.position;

        float angle = Mathf.Atan2(distance.y,Mathf.Abs(distance.x)) * Mathf.Rad2Deg;

        float bodyAngle = angle;
        _body.transform.localRotation = Quaternion.Euler(0, 0, bodyAngle/2 + _initBodyAngle);

        transform.localRotation = Quaternion.Euler(0, 0, angle/2 + _initAngle);
    }
}
