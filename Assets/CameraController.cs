using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]Vector3 _fixedPos;

    Vector3 _mouseView;
    Vector3 _mouseDirection;
    [SerializeField]float _expandsionLimit = 1.0f;

    SpriteRenderer _screenSpriteRenderer;

    Coroutine _shockWaveCoroutine;
    Coroutine _reverseCoroutine;


    static int _waveDistacneFromCenterID = Shader.PropertyToID("_WaveDistanceFromCenter");
    static int _reverseID = Shader.PropertyToID("_Reverse");

    private void Awake()
    {
        _screenSpriteRenderer = transform.Find("ScreenEffect").GetComponent<SpriteRenderer>();
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
            _mouseView = Vector3.zero;
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

    }

  

    public void ExpandsionView(Vector3 pos)
    {
        _mouseDirection = pos;
    }

    void ScreenEffect()
    {
        if(Input.GetKeyDown(KeyCode.W))
        {
            ShockWave(Managers.GetManager<InputManager>().MouseWorldPosition);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            Reverse(0.2f);
        }

    }

    void ShockWave(Vector3 position)
    {
        if (_shockWaveCoroutine != null)
            StopCoroutine(_shockWaveCoroutine);
        _shockWaveCoroutine = StartCoroutine(CorShockWave(position));
    }

    void Reverse(float time)
    {
        if (_reverseCoroutine != null)
            StopCoroutine(_reverseCoroutine);
        _reverseCoroutine = StartCoroutine(CorReverse(time));
    }
    IEnumerator CorShockWave(Vector3 position)
    {
        float time = 0;

        while(time < 2.5f)
        {
            time += Time.deltaTime;
            _screenSpriteRenderer.material.SetFloat(_waveDistacneFromCenterID, time);
            yield return null;
        }

        _screenSpriteRenderer.material.SetFloat(_waveDistacneFromCenterID, -0.1f);
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
