using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[ExecuteInEditMode]
public class PositionLimit : MonoBehaviour
{
    [SerializeField] bool _debug;

    [SerializeField] Vector3 _minLimitLocalPosition;
    [SerializeField] Vector3 _maxLimitLocalPosition;

    [SerializeField][Range(0,1)] float _value;

    public void Update()
    {
        if (_debug || Application.isPlaying)
        {
            Vector3 localPosition = transform.localPosition;

            localPosition.x = Mathf.Clamp(localPosition.x, _minLimitLocalPosition.x, _maxLimitLocalPosition.x);
            localPosition.y = Mathf.Clamp(localPosition.y, _minLimitLocalPosition.y, _maxLimitLocalPosition.y);

            localPosition = _minLimitLocalPosition + (_maxLimitLocalPosition - _minLimitLocalPosition) * _value;

            transform.localPosition = localPosition;
        }
    }

    public void PlayRound(float speed = 1)
    {
        StartCoroutine(CorRound(speed));
    }


    IEnumerator CorRound(float speed,bool forward = true)
    {
        bool hit = false;
        _value = forward ? 1 : 0;
        while (true)
        {
            if (forward)
            {
                if (!hit)
                {
                    if (_value - Time.deltaTime* speed < 0)
                    {
                        _value = 0;
                        hit = true;
                    }
                    else
                    {
                        _value -= Time.deltaTime* speed;
                    }
                }
                else
                {
                    if (_value + Time.deltaTime * speed > 1)
                    {
                        _value = 1;
                        break;
                    }
                    else
                    {
                        _value += Time.deltaTime * speed;
                    }
                }
            }
            yield return null;
        }

    }
}
