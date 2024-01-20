using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : ManagerBase
{
    Camera _mainCamera;
    public Vector3 MouseWorldPosition { get { return _mainCamera.ScreenToWorldPoint(Input.mousePosition); } }

    public Action MouseButtonDown;
    public Action MouseButtonHold;
    public Action Num1KeyDown;
    public Action Num2KeyDown;
    public Action Num3KeyDown;

    public override void Init()
    {
        _mainCamera= Camera.main;
    }

    public override void ManagerUpdate()
    {
        if (Input.GetMouseButtonDown(0))
            MouseButtonDown?.Invoke();
        if (Input.GetMouseButton(0))
            MouseButtonHold?.Invoke();
        if (Input.GetKeyDown(KeyCode.Alpha1))
            Num1KeyDown?.Invoke();
        if(Input.GetKeyDown(KeyCode.Alpha2))
            Num2KeyDown?.Invoke();
        if (Input.GetKeyDown(KeyCode.Alpha3))
            Num3KeyDown?.Invoke();
    }
}
