using System.Collections;
using System.Collections.Generic;
using UnityEditor.Rendering;
using UnityEngine;

public class Blast : MonoBehaviour
{
    [SerializeField] int _circleCount;
    [SerializeField] float _maxRadius;
    [SerializeField] float _speed ;

    LineRenderer _lineRenderer;

    private void Awake()
    {
        _lineRenderer =GetComponent<LineRenderer>();
        _lineRenderer.positionCount = _circleCount + 1;
    }


    public void Update()
    {
        
        if(Input.GetKeyDown(KeyCode.P))
        {
            StartBlast();
        }
    }

    void StartBlast()
    {
        StartCoroutine(CorBlast());
    }

    IEnumerator CorBlast()
    {
        float currentRadius = 0;

        while(currentRadius <= _maxRadius )
        {
            currentRadius += Time.deltaTime * _speed;

            DrawCircle(currentRadius);

            yield return null;
        }
    }

    void DrawCircle(float radius)
    {
        float angleBetweenPoints = 360f / _circleCount;
        
        for(int i =0; i < _circleCount; i++)
        {
            Vector3 point = new Vector3(Mathf.Cos(angleBetweenPoints * i * Mathf.Deg2Rad), Mathf.Sin(angleBetweenPoints * i * Mathf.Deg2Rad),0) * radius;
            _lineRenderer.SetPosition(i, point);
        }

        _lineRenderer.SetPosition(_circleCount, new Vector3(Mathf.Cos(0), Mathf.Sin(0), 0)* radius);
    }
}
