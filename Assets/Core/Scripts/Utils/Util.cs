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

    public static GameObject RangeCast2D(GameObject go, Define.Range range, int layerMask = -1)
    {
        if (go.transform.localScale.x < 0)
            range.center.x = -range.center.x;

        RaycastHit2D hit = new RaycastHit2D();

        // 레이어 마스크사용 안함
        if (layerMask == -1)
        {
            if (range.figureType == Define.FigureType.Box)
            {
                hit = Physics2D.BoxCast(go.transform.position + range.center, range.size, 0, Vector2.zero, 0);
            }
            else if (range.figureType == Define.FigureType.Circle)
            {
                hit = Physics2D.CircleCast(go.transform.position + range.center, range.size.x, Vector2.zero);
            }
            else if (range.figureType == Define.FigureType.Raycast)
            {
                hit = Physics2D.Raycast(go.transform.position + range.center, range.size, range.size.magnitude);
            }
        }
        // 레이어 마스크 사용
        else
        {
            if (range.figureType == Define.FigureType.Box)
            {
                hit = Physics2D.BoxCast(go.transform.position + range.center, range.size, 0, Vector2.zero, 0, layerMask);
            }
            else if (range.figureType == Define.FigureType.Circle)
            {
                hit = Physics2D.CircleCast(go.transform.position + range.center, range.size.x, Vector2.zero, 0, layerMask);
            }
            else if (range.figureType == Define.FigureType.Raycast)
            {
                hit = Physics2D.Raycast(go.transform.position + range.center, range.size, range.size.magnitude, layerMask);
            }
        }

        return hit.collider ? hit.collider.gameObject : null;
    }
    public static GameObject[] RangeCastAll2D(GameObject go, Define.Range range, int layerMask = -1)
    {
        if (go.transform.localScale.x < 0)
            range.center.x = -range.center.x;

        RaycastHit2D[] hits = null;

        // 레이어 마스크사용 안함
        if (layerMask == -1)
        {
            if (range.figureType == Define.FigureType.Box)
            {
                hits = Physics2D.BoxCastAll(go.transform.position + range.center, range.size, 0, Vector2.zero, 0);
            }
            else if (range.figureType == Define.FigureType.Circle)
            {
                hits = Physics2D.CircleCastAll(go.transform.position + range.center, range.size.x, Vector2.zero);
            }
            else if (range.figureType == Define.FigureType.Raycast)
            {
                hits = Physics2D.RaycastAll(go.transform.position + range.center, range.size);
            }
        }
        // 레이어 마스크 사용
        else
        {
            if (range.figureType == Define.FigureType.Box)
            {
                hits = Physics2D.BoxCastAll(go.transform.position + range.center, range.size, 0, Vector2.zero, 0, layerMask);
            }
            else if (range.figureType == Define.FigureType.Circle)
            {
                hits = Physics2D.CircleCastAll(go.transform.position + range.center, range.size.x, Vector2.zero, 0, layerMask);
            }
            else if (range.figureType == Define.FigureType.Raycast)
            {
                hits = Physics2D.RaycastAll(go.transform.position + range.center, range.size, range.size.magnitude, layerMask);
            }
        }

        if (hits == null) return null;

        GameObject[] gos = new GameObject[hits.Length];
        for (int i = 0; i < hits.Length; i++)
        {
            gos[i] = hits[i].collider.gameObject;
        }

        return gos;

    }

    public static float Remap(float value, float inputMin, float inputMax, float outputMin, float outputMax)
    {
        value = Mathf.Clamp(value, inputMin, inputMax);
        return outputMin + (value - inputMin) * (outputMax - outputMin) / (inputMax - inputMin);
    }
    public static void DrawRangeOnGizmos(GameObject go, Define.Range range, Color color)
    {
        Gizmos.color = color;

        if (range.figureType == Define.FigureType.Box)
            Gizmos.DrawWireCube(go.transform.position + range.center, range.size);
        if (range.figureType == Define.FigureType.Circle)
            Gizmos.DrawWireSphere(go.transform.position + range.center, range.size.x);
        if (range.figureType == Define.FigureType.Raycast)
            Gizmos.DrawRay(go.transform.position + range.center, range.size);
    }

    public static float PreviewPercentage(float orginValue, float currentPercentage, float increasedPercentage)
    {
        float percentage = currentPercentage + increasedPercentage;

        return percentage > 0 ? orginValue * (1 + percentage/100) : orginValue/(1-percentage / 100);
    }
    public static int PreviewPercentage(int orginValue, float currentPercentage, float increasedPercentage)
    {
        float percentage = currentPercentage + increasedPercentage;

        return percentage > 0 ? Mathf.RoundToInt(orginValue * (1 + percentage/100)) :Mathf.RoundToInt( orginValue / (1 - percentage/100));
    }
}
