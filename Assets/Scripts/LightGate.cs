using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightGate : MonoBehaviour
{
    [SerializeField] bool _debug;
    [SerializeField] Define.Range _gateRange;
    [SerializeField] float _goalLightIntensity;

    Light2D _globalLight;


    private void Awake()
    {
        
        _globalLight = GameObject.Find("GlobalLight").GetComponent<Light2D>();
    }
    private void OnDrawGizmos()
    {
        if (!_debug) return;

        Util.DrawRangeOnGizmos(gameObject, _gateRange, Color.green);
    }

    private void Update()
    {
        if (_globalLight.intensity != _goalLightIntensity)
        {
            if(_goalLightIntensity < _globalLight.intensity)
            {
                if(_globalLight.intensity - Time.deltaTime/5 < _goalLightIntensity)
                {
                    _globalLight.intensity = _goalLightIntensity;
                }
                else
                {
                    _globalLight.intensity -= Time.deltaTime/5;
                }
            }
            else
            {
                if (_globalLight.intensity + Time.deltaTime / 5 > _goalLightIntensity)
                {
                    _globalLight.intensity = _goalLightIntensity;
                }
                else
                {
                    _globalLight.intensity += Time.deltaTime / 5;
                }
            }
        }
    }
}
