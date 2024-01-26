using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]Vector3 _fixedPos;
    private void FixedUpdate()
    {
        if (Managers.GetManager<GameManager>().Player == null) return;

        Vector3 playerPosition = Managers.GetManager<GameManager>().Player.transform.position;
        playerPosition.z = -10f;
        float value = Remap((playerPosition - transform.position).magnitude, 0, 10, 0f, 0.5f);

        transform.position = Vector3.Lerp(transform.position, playerPosition + _fixedPos, value);

    }

    public static float Remap(float value, float inputMin, float inputMax, float outputMin, float outputMax)
    {
        value =  Mathf.Clamp(value, inputMin, inputMax);
        return outputMin + (value - inputMin) * (outputMax - outputMin) / (inputMax - inputMin);
    }
}
