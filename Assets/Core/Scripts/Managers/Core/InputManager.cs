using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : Manager
{
    Camera _mainCamera;
    public Vector3 MouseWorldPosition { get { return _mainCamera.ScreenToWorldPoint(Input.mousePosition); } }

    public Action MouseButtonDown;

    public override void Init()
    {
        _mainCamera= Camera.main;
    }

    public override void ManagerUpdate()
    {
        if (Input.GetMouseButtonDown(0))
        {
            MouseButtonDown?.Invoke();
        }

    }
}
