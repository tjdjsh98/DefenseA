using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using UnityEditor.Rendering;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public static class Util
{
    public static GameObject FindOrCreate(string name)
    {
        GameObject go = null;

        go = GameObject.Find(name);
        if (go == null)
            go = new GameObject(name);

        return go;
    }

    public static RaycastHit2D RangeCast2D(GameObject go, Define.Range range, int layerMask = -1)
    {
        if (go.transform.localScale.x < 0)
            range.center.x = -range.center.x;

        RaycastHit2D hit = new RaycastHit2D();

        float originAngle = Mathf.Atan2(range.center.y, range.center.x);
        Vector3 rangePosition = Vector3.zero;
        rangePosition.x = range.center.magnitude * Mathf.Cos(range.angle * Mathf.Deg2Rad + originAngle);
        rangePosition.y = range.center.magnitude * Mathf.Sin(range.angle * Mathf.Deg2Rad + originAngle);
        // ���̾� ����ũ��� ����
        if (layerMask == -1)
        {

            if (range.figureType == Define.FigureType.Box)
            {
                hit = Physics2D.BoxCast(go.transform.position + rangePosition, range.size, range.angle, Vector2.zero, 0);
            }
            else if (range.figureType == Define.FigureType.Circle)
            {
                hit = Physics2D.CircleCast(go.transform.position + rangePosition, range.size.x, Vector2.zero);
            }
            else if (range.figureType == Define.FigureType.Raycast)
            {
                hit = Physics2D.Raycast(go.transform.position + rangePosition, range.size, range.size.magnitude);
            }
        }
        // ���̾� ����ũ ���
        else
        {
            if (range.figureType == Define.FigureType.Box)
            {
                hit = Physics2D.BoxCast(go.transform.position + rangePosition, range.size, range.angle, Vector2.zero, 0, layerMask);
            }
            else if (range.figureType == Define.FigureType.Circle)
            {
                hit = Physics2D.CircleCast(go.transform.position + rangePosition, range.size.x, Vector2.zero, 0, layerMask);
            }
            else if (range.figureType == Define.FigureType.Raycast)
            {
                hit = Physics2D.Raycast(go.transform.position + rangePosition, range.size, range.size.magnitude, layerMask);
            }
        }

        return hit;
    }
    public static List<RaycastHit2D> RangeCastAll2D(GameObject go, Define.Range range, int layerMask = -1, Func<RaycastHit2D,bool> condition = null)
    {
        if (go.transform.localScale.x < 0)
            range.center.x = -range.center.x;

        RaycastHit2D[] hits = null;

        float originAngle = Mathf.Atan2(range.center.y, range.center.x);
        Vector3 rangePosition = Vector3.zero;
        rangePosition.x = range.center.magnitude * Mathf.Cos(range.angle * Mathf.Deg2Rad + originAngle);
        rangePosition.y = range.center.magnitude * Mathf.Sin(range.angle * Mathf.Deg2Rad + originAngle);
        // ���̾� ����ũ��� ����
        if (layerMask == -1)
        {
            if (range.figureType == Define.FigureType.Box)
            {
                hits = Physics2D.BoxCastAll(go.transform.position + rangePosition, range.size, range.angle, Vector2.zero, 0);
            }
            else if (range.figureType == Define.FigureType.Circle)
            {
                hits = Physics2D.CircleCastAll(go.transform.position + rangePosition, range.size.x, Vector2.zero);
            }
            else if (range.figureType == Define.FigureType.Raycast)
            {
                hits = Physics2D.RaycastAll(go.transform.position + rangePosition, range.size);
            }
        }
        // ���̾� ����ũ ���
        else
        {
            if (range.figureType == Define.FigureType.Box)
            {
                hits = Physics2D.BoxCastAll(go.transform.position + rangePosition, range.size, range.angle, Vector2.zero, 0, layerMask);
            }
            else if (range.figureType == Define.FigureType.Circle)
            {
                hits = Physics2D.CircleCastAll(go.transform.position + rangePosition, range.size.x, Vector2.zero, 0, layerMask);
            }
            else if (range.figureType == Define.FigureType.Raycast)
            {

                hits = Physics2D.RaycastAll(go.transform.position + rangePosition, range.size, range.size.magnitude, layerMask);
            }
        }
        if (hits == null) return null;

        List<RaycastHit2D> list = new List<RaycastHit2D>();
        for (int i = 0; i < hits.Length; i++)
        {
            if (condition == null || condition.Invoke(hits[i]))
            {
                list.Add(hits[i]);
            }
        }

        return list;

    }

    public static float Remap(float value, float inputMin, float inputMax, float outputMin, float outputMax)
    {
        value = Mathf.Clamp(value, inputMin, inputMax);
        return outputMin + (value - inputMin) * (outputMax - outputMin) / (inputMax - inputMin);
    }
    public static void DrawRangeOnGizmos(GameObject go, Define.Range range, Color color)
    {
        Gizmos.color = color;
        if (go.transform.localScale.x < 0)
        {
            range.center.x = -range.center.x;
        }


        Matrix4x4 matrix = Matrix4x4.TRS(go.transform.position, Quaternion.Euler(0, 0, range.angle), Vector3.one);
        Gizmos.matrix = matrix;

        if (go.transform.localScale.x < 0)
            range.center.x = -range.center.x;

        if (range.figureType == Define.FigureType.Box)
            Gizmos.DrawWireCube(range.center, range.size);
        if (range.figureType == Define.FigureType.Circle)
            Gizmos.DrawWireSphere(range.center, range.size.x);
        if (range.figureType == Define.FigureType.Raycast)
            Gizmos.DrawRay(range.center, range.size);

        Gizmos.matrix = Matrix4x4.identity;
    }

    public static float PreviewPercentage(float orginValue, float currentPercentage, float increasedPercentage)
    {
        float percentage = currentPercentage + increasedPercentage;

        return percentage > 0 ? orginValue * (1 + percentage / 100) : orginValue / (1 - percentage / 100);
    }
    public static int PreviewPercentage(int orginValue, float currentPercentage, float increasedPercentage)
    {
        float percentage = currentPercentage + increasedPercentage;

        return percentage > 0 ? Mathf.RoundToInt(orginValue * (1 + percentage / 100)) : Mathf.RoundToInt(orginValue / (1 - percentage / 100));
    }
    

    public static Vector3? GetGroundPosition(Vector3 position)
    {
        RaycastHit2D hit = Physics2D.Raycast(position, Vector2.down, 100, LayerMask.GetMask("Ground"));

        if(hit.collider == null) return null;

        return hit.point;
    }

    public static T DeepClone<T>(this T obj)
    {
        using (var ms = new MemoryStream())
        {
            var formatter = new BinaryFormatter();
            formatter.Serialize(ms, obj);
            ms.Position = 0;

            return (T)formatter.Deserialize(ms);
        }
    }

    public static float CalcPercentage(float percentage)
    {
        float result = 0;
        if(percentage < 0)
        {
            result = 1/(1- percentage/100f);
        }
        else
        {
            result = 1 *(1+percentage / 100f);
        }

        return result;
    }

    public static Vector2 GetFitSpriteSize(Sprite sprite, int maxSize)
    {
        Vector2 spriteSize = sprite.textureRect.size;

        if (spriteSize.x > spriteSize.y)
        {
            spriteSize.y = maxSize * spriteSize.y/spriteSize.x;
            spriteSize.x = maxSize;
        }
        else
        {
            spriteSize.x = maxSize * spriteSize.x / spriteSize.y;
            spriteSize.y = maxSize;
        }

        return spriteSize;
    }
}
