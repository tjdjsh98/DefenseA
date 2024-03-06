using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    [SerializeField]Vector3 _fixedPos;

    [SerializeField] List<BackgroundLayer> _backgroundLayerList;

    [SerializeField]

    float _cameraWidth;

    Vector3 _initCameraPosition;

    Vector3 _mouseView;
    Vector3 _mouseDirection;
    [SerializeField]float _expandsionLimit = 1.0f;

    SpriteRenderer _screenSpriteRenderer;

    Coroutine _shockWaveCoroutine;
    Coroutine _shockWave2Coroutine;
    Coroutine _reverseCoroutine;


    static int _ringPostionID = Shader.PropertyToID("_RingPosition");
    static int _waveDistacneFromCenterID = Shader.PropertyToID("_WaveDistanceFromCenter");
    static int _waveDistacneFromCenter2ID = Shader.PropertyToID("_WaveDistanceFromCenter2");
    static int _cameraSizeID = Shader.PropertyToID("_CameraSize");
    static int _reverseID = Shader.PropertyToID("_Reverse");

    private void Awake()
    {
        _screenSpriteRenderer = transform.Find("ScreenEffect").GetComponent<SpriteRenderer>();
        _initCameraPosition = transform.position;
        _screenSpriteRenderer.material.SetFloat(_cameraSizeID, Camera.main.orthographicSize);
        _cameraWidth = GetCameraWidth();
    }
    private void Update()
    {
        ScreenEffect(); 
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
            _mouseView.x = Mathf.Lerp(_mouseView.x,0, 0.01f);
        }

        _mouseDirection = Vector3.zero;
    }

    private void FixedUpdate()
    {
        HandleBackgroundLayers();
        if (Managers.GetManager<GameManager>().Player == null) return;  

        Vector3 playerPosition = Managers.GetManager<GameManager>().Player.transform.position;
        playerPosition.z = -10f;
        float value = Util.Remap((playerPosition - transform.position).magnitude, 0, 10, 0f, 0.5f);

        Vector3 destination = playerPosition + _fixedPos + _mouseView;

        if (destination.x < 0 || destination.x > Managers.GetManager<GameManager>().MapSize ) return;

        transform.position = Vector3.Lerp(transform.position, playerPosition + _fixedPos + _mouseView, 0.1f);

    }

    void HandleBackgroundLayers()
    {
        float mapSize = Managers.GetManager<GameManager>().MapSize;
        float distance = transform.position.x;

        distance = Mathf.Clamp(distance, 0, mapSize);

        foreach(var layer in _backgroundLayerList)
        {
            Vector3 position = Vector3.zero;
            position.x = Mathf.Lerp(layer.width / 2 - _cameraWidth / 2 , mapSize + _cameraWidth / 2 - layer.width / 2 , (distance / mapSize));
            layer.layer.transform.position = position;
        }
    }
  

    public void ExpandsionView(Vector3 pos)
    {
        _mouseDirection = pos;
    }

    void ScreenEffect()
    {
      

    }

    public void ShockWave(Vector3 position, float speed,int num =0)
    {
        if(num == 0 && _shockWaveCoroutine != null) 
            StopCoroutine(_shockWaveCoroutine);

        if (num == 1 &&_shockWave2Coroutine != null)
            StopCoroutine(_shockWave2Coroutine);

        if(num == 0)
            _shockWaveCoroutine = StartCoroutine(CorShockWave(position, speed,num));
        if(num == 1)
            _shockWave2Coroutine = StartCoroutine(CorShockWave(position, speed,num));
    }

    public void StopShockwave(int num =0)
    {
        if (num == 0 && _shockWaveCoroutine != null)
            StopCoroutine(_shockWaveCoroutine);

        if (num == 1 && _shockWave2Coroutine != null)
            StopCoroutine(_shockWave2Coroutine);
        _screenSpriteRenderer.material.SetFloat(num == 0 ?_waveDistacneFromCenterID : _waveDistacneFromCenter2ID, -5.0f);

    }
    void Reverse(float time)
    {
        if (_reverseCoroutine != null)
            StopCoroutine(_reverseCoroutine);
        _reverseCoroutine = StartCoroutine(CorReverse(time));
    }
    IEnumerator CorShockWave(Vector3 position,float speed, int num = 0)
    {
        float time = 0;

        Vector3 screenPosition = Camera.main.WorldToViewportPoint(position);
        screenPosition.z = 0;
        _screenSpriteRenderer.material.SetVector(_ringPostionID, screenPosition);
        while(time < 2.5f)
        {
            time += Time.deltaTime;
            _screenSpriteRenderer.material.SetFloat(num == 0 ?_waveDistacneFromCenterID : _waveDistacneFromCenter2ID, time*speed);
            yield return null;
        }

        if(num == 0)
            _screenSpriteRenderer.material.SetFloat(_waveDistacneFromCenterID, -5.0f);
        else
            _screenSpriteRenderer.material.SetFloat(_waveDistacneFromCenter2ID, -5.0f);
    }

    IEnumerator CorReverse(float time)
    {
        float current = 0;

        _screenSpriteRenderer.material.SetInt(_reverseID, 1);
        while (current < time)
        {
            current += Time.deltaTime;

            yield return null;
        }
        _screenSpriteRenderer.material.SetInt(_reverseID, 0);
    }

    public float GetCameraWidth()
    {
        float screenHeightInUnits = Camera.main.orthographicSize * 2;

        return screenHeightInUnits * Screen.width / Screen.height;
    }
}

[System.Serializable]
public class BackgroundLayer
{
    public GameObject layer;
    public float width;
}
