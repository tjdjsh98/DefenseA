using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UIElements;

public class GaugeBar : MonoBehaviour
{
    LineRenderer _frontLine;
    LineRenderer FrontLine { get { if (_frontLine == null) _frontLine = transform.Find("Front").GetComponent<LineRenderer>(); return _frontLine; } }
    LineRenderer _backLine;
    LineRenderer BackLine { get { if (_backLine == null) _backLine = transform.Find("Back").GetComponent<LineRenderer>(); return _backLine; } }

    GameObject _pointStart;
    GameObject PointStart{ get { if (_pointStart == null)
            {
                _pointStart = transform.Find("PointStart").gameObject;
                _pointStart.gameObject.SetActive(false);

            }
        return _pointStart;
        }
    }

    GameObject _pointEnd;
    GameObject PointEnd{ get { if (_pointEnd == null)
            {
                _pointEnd = transform.Find("PointEnd").gameObject;
                _pointEnd.gameObject.SetActive(false);
            }
            return _pointEnd; 
        } 
    }

    public void SetRatio(int value, int max)
    {
        float ratio = 0;
        if (max == 0)
            ratio = 0;
        else
            ratio = (value > max ? max : value) / (float)max;

        FrontLine.SetPosition(1, new Vector3(BackLine.GetPosition(1).x * ratio, 0, 0));
    }
    public void SetRatio(float value, float max)
    {
        float ratio = 0;
        if (max == 0)
            ratio = 0;
        else
            ratio = (value > max ? max : value) / (float)max;

        FrontLine.SetPosition(1, new Vector3(BackLine.GetPosition(1).x * ratio, 0, 0));

    }
    
    public void DisablePoint()
    {
        PointStart.gameObject.SetActive(false);
        PointEnd.gameObject.SetActive(false);
    }
    public void Point(float startRatio, float endRatio)
    {
        PointStart.gameObject.SetActive(true);
        PointEnd.gameObject.SetActive(true);

        float max = _backLine.GetPosition(1).x;

        PointStart.transform.localPosition = new Vector3(-max / 2 + max*startRatio, 0, 0);
        PointEnd.transform.localPosition = new Vector3(-max / 2 + max*endRatio, 0, 0);
    }
}
