using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;
using Random = UnityEngine.Random;

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

    Camera _effectCamera;
    SpriteRenderer _screenSpriteRenderer;

    Coroutine _shockWaveCoroutine;
    Coroutine _shockWave2Coroutine;
    Coroutine _reverseCoroutine;
    Coroutine _cameraShake;

    Vector3 _shakePosition;


    static int _ringPostionID = Shader.PropertyToID("_RingPosition");
    static int _waveDistacneFromCenterID = Shader.PropertyToID("_WaveDistanceFromCenter");
    static int _waveDistacneFromCenter2ID = Shader.PropertyToID("_WaveDistanceFromCenter2");
    static int _cameraSizeID = Shader.PropertyToID("_CameraSize");
    static int _reverseID = Shader.PropertyToID("_Reverse");

    private void Awake()
    {
        _effectCamera = transform.Find("EffectCamera").GetComponent<Camera>();  
        _screenSpriteRenderer = transform.Find("ScreenEffect").GetComponent<SpriteRenderer>();
        _initCameraPosition = transform.position;
        _screenSpriteRenderer.material.SetFloat(_cameraSizeID, Camera.main.orthographicSize);

        OffEffectScreen();
    }
    private void Update()
    {
        _cameraWidth = GetCameraWidth();
        _screenSpriteRenderer.transform.localScale = new Vector3(_cameraWidth,Camera.main.orthographicSize*2);

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

        if (destination.x < 0)
        {
            destination.x = 0;
        }
        if (destination.x > Managers.GetManager<GameManager>().MapSize)
        {
            destination.x = Managers.GetManager<GameManager>().MapSize;
        }
        
        transform.position = Vector3.Lerp(transform.position, destination, 0.1f) + _shakePosition;
    }

    void OnEffectScreen()
    {
        _effectCamera.gameObject.SetActive(true);
        _screenSpriteRenderer.gameObject.SetActive(true);
    }
    void OffEffectScreen()
    {
        _effectCamera.gameObject.SetActive(false);
        _screenSpriteRenderer.gameObject.SetActive(false);
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
            position.y = layer.layer.transform.position.y;
            layer.layer.transform.position = position;
        }
    }
  

    public void ExpandsionView(Vector3 pos)
    {
        _mouseDirection = pos;
    }


    public void ShockWave(Vector3 position, float speed,int num =0)
    {
        OnEffectScreen();
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
        OffEffectScreen();
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
        OnEffectScreen();
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

        OffEffectScreen();
    }

    IEnumerator CorReverse(float time)
    {
        OnEffectScreen();
        float current = 0;

        _screenSpriteRenderer.material.SetInt(_reverseID, 1);
        while (current < time)
        {
            current += Time.deltaTime;

            yield return null;
        }
        _screenSpriteRenderer.material.SetInt(_reverseID, 0);
        OffEffectScreen();
    }

    public float GetCameraWidth()
    {
        float screenHeightInUnits = Camera.main.orthographicSize * 2;

        return screenHeightInUnits * Screen.width / Screen.height;
    }

    public void ShakeCamera(float power, float time)
    {
        if(_cameraShake != null)
            StopCoroutine(_cameraShake);
        StartCoroutine(CorShakeCamera(power, time));
    }

    IEnumerator CorShakeCamera(float power, float time)
    {
        float currentTime = 0;
        while (currentTime < time)
        {
            _shakePosition = Random.insideUnitCircle * power;
            currentTime += Time.deltaTime;

            yield return null;
        }

        _shakePosition = Vector3.zero;
    }
}

[System.Serializable]
public class BackgroundLayer
{
    public GameObject layer;
    public float width;
}
