using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    [SerializeField]Vector3 _fixedPos;
    [SerializeField]GameObject _baseEnvironment;
    [SerializeField] Vector3 _baseEnvironmenInitLocalPosition;

    Vector3 _initCameraPosition;

    Vector3 _mouseView;
    Vector3 _mouseDirection;
    [SerializeField]float _expandsionLimit = 1.0f;

    SpriteRenderer _screenSpriteRenderer;

    Coroutine _shockWaveCoroutine;
    Coroutine _reverseCoroutine;


    static int _ringPostionID = Shader.PropertyToID("_RingPosition");
    static int _waveDistacneFromCenterID = Shader.PropertyToID("_WaveDistanceFromCenter");
    static int _cameraSizeID = Shader.PropertyToID("_CameraSize");
    static int _reverseID = Shader.PropertyToID("_Reverse");

    private void Awake()
    {
        _screenSpriteRenderer = transform.Find("ScreenEffect").GetComponent<SpriteRenderer>();
        _initCameraPosition = transform.position;
        if(_baseEnvironment)
            _baseEnvironmenInitLocalPosition = _baseEnvironment.transform.localPosition;
        _screenSpriteRenderer.material.SetFloat(_cameraSizeID, Camera.main.orthographicSize);
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
        if (Managers.GetManager<GameManager>().Player == null) return;

        Vector3 playerPosition = Managers.GetManager<GameManager>().Player.transform.position;
        playerPosition.z = -10f;
        float value = Util.Remap((playerPosition - transform.position).magnitude, 0, 10, 0f, 0.5f);

        Vector3 destination = playerPosition + _fixedPos + _mouseView;

        if (destination.x < 0 || destination.x > Managers.GetManager<GameManager>().MapSize ) return;

        transform.position = Vector3.Lerp(transform.position, playerPosition + _fixedPos + _mouseView, 0.1f);

        HandleBaseEnvironent();
    }

    void HandleBaseEnvironent()
    {
        if (_baseEnvironment == null) return;

        float mapSize = Managers.GetManager<GameManager>().MapSize;
        float distance = transform.position.x - _initCameraPosition.x;

        Vector3 position = _baseEnvironmenInitLocalPosition;

        position.x -= Mathf.Abs(_baseEnvironmenInitLocalPosition.x)*(distance / mapSize)*2;
        position.x -= _mouseView.x * (distance / mapSize)*2;
        _baseEnvironment.transform.localPosition = position;
        
    }
  

    public void ExpandsionView(Vector3 pos)
    {
        _mouseDirection = pos;
    }

    void ScreenEffect()
    {
      

    }

    public void ShockWave(Vector3 position, float speed)
    {
        if (_shockWaveCoroutine != null)
            StopCoroutine(_shockWaveCoroutine);
        _shockWaveCoroutine = StartCoroutine(CorShockWave(position, speed));
    }

    public void StopShockwave()
    {

        if (_shockWaveCoroutine != null)
            StopCoroutine(_shockWaveCoroutine);
        _screenSpriteRenderer.material.SetFloat(_waveDistacneFromCenterID, -5.0f);

    }
    void Reverse(float time)
    {
        if (_reverseCoroutine != null)
            StopCoroutine(_reverseCoroutine);
        _reverseCoroutine = StartCoroutine(CorReverse(time));
    }
    IEnumerator CorShockWave(Vector3 position,float speed)
    {
        float time = 0;

        Vector3 screenPosition = Camera.main.WorldToViewportPoint(position);
        screenPosition.z = 0;
        _screenSpriteRenderer.material.SetVector(_ringPostionID, screenPosition);
        while(time < 2.5f)
        {
            time += Time.deltaTime;
            _screenSpriteRenderer.material.SetFloat(_waveDistacneFromCenterID, time*speed);
            yield return null;
        }

        _screenSpriteRenderer.material.SetFloat(_waveDistacneFromCenterID, -5.0f);
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
}
