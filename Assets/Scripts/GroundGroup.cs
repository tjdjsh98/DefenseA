using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundGroup : MonoBehaviour
{
    SpriteRenderer[] _grounds;


    private void Awake()
    {
        _grounds = GetComponentsInChildren<SpriteRenderer>();
    }
    public void SetColor(Color color)
    {
        foreach (var ground in _grounds)
        {
            ground.color = color;
        }
    }
    public void SetAlpha(float alpha)
    {
        foreach(var ground in _grounds)
        {
            Color color= ground.color;
            color.a = alpha;
            ground.color = color;
        }
    }
}
