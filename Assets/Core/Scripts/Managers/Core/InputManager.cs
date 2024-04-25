using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class InputManager : ManagerBase
{
    GraphicRaycaster _graphicRaycaster;
    GraphicRaycaster GraphicRaycaster { set { _graphicRaycaster = value; } get { if (_graphicRaycaster == null) GetRaycaster(); return _graphicRaycaster; } }

    Camera _mainCamera;
    Camera MainCamera { get { if (_mainCamera == null) _mainCamera = Camera.main; return _mainCamera; }  } 
    public Vector3 MouseWorldPosition { get { return MainCamera.ScreenToWorldPoint(Input.mousePosition); } }

    public Action<List<GameObject>> UIMouseDownHandler;
    public Action<List<GameObject>> UIMouseDragHandler;
    public Action<List<GameObject>> UIMouseHoverHandler;
    public Action<List<GameObject>> UIMouseUpHandler;
    List<GameObject> _uiRaycastGameObjects = new List<GameObject>();
    bool _isFinishRaycastUI = false;

    public Action MouseButtonDownHandler;
    public Action MouseButton1DownHandler;
    public Action MouseButtonHoldHandler;
    public Action MouseButtonUpHandler;
    List<GameObject> _mouseRaycastGameobjects = new List<GameObject>();


    public Action Num1KeyDownHandler;
    public Action Num2KeyDownHandler;
    public Action Num3KeyDownHandler;
    public Action ReloadKeyDownHandler;
    public Action ReloadKeyHoldHandler;
    public Action ReloadKeyUpHandler;
    public Action JumpKeyDownHandler;
    public Action InteractKeyDownHandler;

    public Action Skill1KeyDownHandler;
    public Action Skill2KeyDownHandler;
    public Action Skill3KeyDownHandler;
    public Action Skill4KeyDownHandler;

    public Action RightArrowPressedHandler;
    public Action LeftArrowPressedHandler;
    public Action UpArrowPressedHandler;
    public Action DownArrowPressedHandler;

    public override void Init()
    {
        
    }

    public override void ManagerUpdate()
    {
       
        if(UIMouseHoverHandler != null)
        {
            RaycastUI();
            UIMouseHoverHandler.Invoke(_uiRaycastGameObjects);
        }

        if (Input.GetMouseButtonDown(0))
        {
            if(UIMouseDownHandler!= null)
            {
                RaycastUI();
                UIMouseDownHandler.Invoke(_uiRaycastGameObjects);
            }
            MouseButtonDownHandler?.Invoke();
        }
   
        if (Input.GetMouseButton(0))
        {

            MouseButtonHoldHandler?.Invoke();
        }
        if (Input.GetMouseButtonUp(0))
        {

            MouseButtonUpHandler?.Invoke();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
            Num1KeyDownHandler?.Invoke();
        if(Input.GetKeyDown(KeyCode.Alpha2))
            Num2KeyDownHandler?.Invoke();
        if (Input.GetKeyDown(KeyCode.Alpha3))
            Num3KeyDownHandler?.Invoke();

        if (Input.GetKey(KeyCode.W))
            UpArrowPressedHandler?.Invoke();
        if (Input.GetKey(KeyCode.S))
            DownArrowPressedHandler?.Invoke();
        if (Input.GetKey(KeyCode.D))
            RightArrowPressedHandler?.Invoke();
        if (Input.GetKey(KeyCode.A))
            LeftArrowPressedHandler?.Invoke();

        if (Input.GetKeyDown(KeyCode.R) || Input.GetMouseButtonDown(1))
            ReloadKeyDownHandler?.Invoke();
        if (Input.GetKey(KeyCode.R) || Input.GetMouseButton(1))
            ReloadKeyHoldHandler?.Invoke();
        if (Input.GetKeyUp(KeyCode.R) || Input.GetMouseButtonUp(1))
            ReloadKeyUpHandler?.Invoke();

        if (Input.GetKeyDown(KeyCode.Space))
            JumpKeyDownHandler?.Invoke();
        if(Input.GetKeyDown(KeyCode.E))
            InteractKeyDownHandler?.Invoke();

        if (Input.GetKeyDown(KeyCode.Z))
            Skill1KeyDownHandler?.Invoke();
        if (Input.GetKeyDown(KeyCode.X))
            Skill2KeyDownHandler?.Invoke();
        if (Input.GetKeyDown(KeyCode.C))
            Skill3KeyDownHandler?.Invoke();
        if (Input.GetKeyDown(KeyCode.V))
            Skill4KeyDownHandler?.Invoke();

        _isFinishRaycastUI = false;
    }

    void GetRaycaster()
    {
        if (_graphicRaycaster != null || GameObject.Find("Canvas") == null) return;

        _graphicRaycaster = GameObject.Find("Canvas").GetComponent<GraphicRaycaster>();
    }
    public void RaycastUI()
    {
        if (_isFinishRaycastUI) return;

        var ped = new PointerEventData(null);

        _uiRaycastGameObjects.Clear();

        ped.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        GraphicRaycaster.Raycast(ped, results);

        if (results.Count <= 0) return;

        foreach (var item in results)
        {
            _uiRaycastGameObjects.Add(item.gameObject);
        }
        _isFinishRaycastUI = true;
    }
    public bool CheckIsOverlapUI()
    {
        var ped = new PointerEventData(null);

        _uiRaycastGameObjects.Clear();

        ped.position = Input.mousePosition;
        List<RaycastResult> results = new List<RaycastResult>();
        GraphicRaycaster.Raycast(ped, results);

        if (results.Count > 0) return true;

        return false;
    }
    public void Raycast()
    {
        RaycastHit2D[] hits = Physics2D.RaycastAll(MouseWorldPosition, Vector2.zero, 0);

        _mouseRaycastGameobjects.Clear();

        if (hits.Length <= 0) return;
        foreach (var hit in hits)
        {
            if (hit.collider == null) continue;
            _mouseRaycastGameobjects.Add(hit.collider.gameObject);
        }
    }

    public override void ManagerDestroy()
    {
    }
}
