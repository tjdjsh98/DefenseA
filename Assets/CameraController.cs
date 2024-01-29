using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]Vector3 _fixedPos;

    Vector3 _mouseView;
    Vector3 _mouseDirection;
    [SerializeField]float _expandsionLimit = 1.0f;

    private void Update()
    {
        if(_mouseDirection.x >=1)
        {
            _mouseView.x = Mathf.Lerp(_mouseView.x, _expandsionLimit, 0.01f);
        }
        else if(_mouseDirection.x <= -1)
        {
            _mouseView.x = Mathf.Lerp(_mouseView.x, -_expandsionLimit, 0.01f);
        }
        else
        {
            _mouseView = Vector3.zero;
        }

        _mouseDirection = Vector3.zero;
    }

    private void FixedUpdate()
    {
        if (Managers.GetManager<GameManager>().Player == null) return;

        Vector3 playerPosition = Managers.GetManager<GameManager>().Player.transform.position;
        playerPosition.z = -10f;
        float value = Remap((playerPosition - transform.position).magnitude, 0, 10, 0f, 0.5f);

        transform.position = Vector3.Lerp(transform.position, playerPosition + _fixedPos + _mouseView, 0.1f);

    }

    public static float Remap(float value, float inputMin, float inputMax, float outputMin, float outputMax)
    {
        value =  Mathf.Clamp(value, inputMin, inputMax);
        return outputMin + (value - inputMin) * (outputMax - outputMin) / (inputMax - inputMin);
    }

    public void ExpandsionView(Vector3 pos)
    {
        _mouseDirection = pos;
    }
}
