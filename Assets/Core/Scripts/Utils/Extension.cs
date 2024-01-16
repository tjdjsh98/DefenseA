using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
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
}
