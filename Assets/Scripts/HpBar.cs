using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HpBar : MonoBehaviour
{
    LineRenderer _frontLine;
    LineRenderer _backLine;

    private void Awake()
    {
        _frontLine = transform.Find("Front").GetComponent<LineRenderer>();
        _backLine = transform.Find("Back").GetComponent<LineRenderer>();
    }
    public void SetRatio(int value, int max)
    {
        float ratio = 0;
        if (max == 0)
            ratio = 0;
        else
            ratio = (value > max ? max : value) / (float)max;

        _frontLine.SetPosition(1, new Vector3(_backLine.GetPosition(1).x * ratio, 0, 0));

    }
    public void SetRatio(float value, float max)
    {
        float ratio = 0;
        if (max == 0)
            ratio = 0;
        else
            ratio = (value > max ? max : value) / (float)max;

        _frontLine.SetPosition(1, new Vector3(_backLine.GetPosition(1).x * ratio, 0, 0));

    }
}
