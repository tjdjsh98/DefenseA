using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Extension
{
    public static T GetOrAddComponent<T>(this GameObject gameObject) where T : MonoBehaviour
    {
        T component =  gameObject.GetComponent<T>();
        if(component == null)
        {
            component = gameObject.AddComponent<T>();
        }

        return component;
    }

    public static GameObject FindOrCreate(this GameObject game, string name)
    {
        GameObject gameObject = null;

        return gameObject;
    }

    public static T Last<T>(this List<T> list) 
    {
        if (list.Count == 0) return default(T);
        return list[list.Count - 1];
    }
    public static T GetRandom<T>(this List<T> list)
    {
        if (list.Count == 0) return default(T);

        return list[Random.Range(0, list.Count)];
    }
    public static Vector3 GetRandom(this Vector3 vector,float min, float max)
    {
        Vector3 result = vector;
        result.x += Random.Range(min, max);
        result.y += Random.Range(min, max);

        return result;
    }
}
