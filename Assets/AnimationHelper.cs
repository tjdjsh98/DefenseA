using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationHelper : MonoBehaviour
{

    GameObject _model;
    private void Awake()
    {
        _model = transform.Find("Model").gameObject;
    }
    public void RootMotion()
    {
        Vector3 temp = transform.position;
        transform.position = _model.transform.position;
        _model.transform.position = temp;
    }
}
