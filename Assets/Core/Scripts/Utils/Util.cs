using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Util
{
    public static GameObject FindOrCreate(string name)
    {
        GameObject go = null;

        go = GameObject.Find(name);
        if(go == null)
            go = new GameObject(name);

        return go;
    }

    public static GameObject[] BoxcastAll2D(GameObject go, Define.Range range)
    {
        if(go.transform.localScale.x < 0)
            range.center.x = -range.center.x;

        RaycastHit2D[] hits = Physics2D.BoxCastAll(go.transform.position + range.center,range.size, 0, Vector2.zero, 0);

        GameObject[] result = new GameObject[hits.Length];

        for(int i =0 ; i < hits.Length;i++) 
        {
            result[i] = hits[i].collider.gameObject;
        }

        return result;
    }
    public static float Remap(float value, float inputMin, float inputMax, float outputMin, float outputMax)
    {
        value = Mathf.Clamp(value, inputMin, inputMax);
        return outputMin + (value - inputMin) * (outputMax - outputMin) / (inputMax - inputMin);
    }
}
