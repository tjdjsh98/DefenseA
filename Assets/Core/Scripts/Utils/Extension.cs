using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

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
    public static T GetRandom<T>(this IEnumerable<T> list)
    {
        if (list.Count() == 0)
        {
            Debug.LogWarning("리스트의 갯수가 부족하여 랜덤 값을 가져올 수 없습니다.");
            return default(T);
        }

        int random = Random.Range(0, list.Count());
        return list.ElementAt(random);

    }
    public static List<T> GetRandom<T>(this List<T> list, int count)
    {
        if (list.Count == 0 || list.Count < count) return default(List<T>);

        List<T> values = list.ToList();
        List<T> result = new List<T>();

        for(int i =0; i < count; i++)
        {
            int random = Random.Range(0, values.Count);
            result.Add(values[random]);
            values.RemoveAt(random);
        }

        return result;
        
    }

   
    public static Vector3 GetRandom(this Vector3 vector,float min, float max)
    {
        Vector3 result = vector;
        result.x += Random.Range(min, max);
        result.y += Random.Range(min, max);

        return result;
    }
}
