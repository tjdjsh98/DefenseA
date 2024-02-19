using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GaugeBar : MonoBehaviour
{
    LineRenderer _frontLine;
    LineRenderer _backLine;

    GameObject _pointStart;
    GameObject _pointEnd;

    private void Awake()
    {
        _frontLine = transform.Find("Front").GetComponent<LineRenderer>();
        _backLine = transform.Find("Back").GetComponent<LineRenderer>();
        _pointStart = transform.Find("PointStart").gameObject;
        _pointEnd = transform.Find("PointEnd").gameObject;

        _pointStart.gameObject.SetActive(false);
        _pointEnd.gameObject.SetActive(false);
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
    
    public void DisablePoint()
    {
        _pointStart.gameObject.SetActive(false);
        _pointEnd.gameObject.SetActive(false);
    }
    public void Point(float startRatio, float endRatio)
    {
        _pointStart.gameObject.SetActive(true);
        _pointEnd.gameObject.SetActive(true);

        float max = _backLine.GetPosition(1).x;

        _pointStart.transform.localPosition = new Vector3(-max / 2 + max*startRatio, 0, 0);
        _pointEnd.transform.localPosition = new Vector3(-max / 2 + max*endRatio, 0, 0);
    }
}
