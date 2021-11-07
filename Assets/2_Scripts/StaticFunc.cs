using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public static class StaticFunc
{
    public static void ReleaseTemp(this RenderTexture self) { RenderTexture.ReleaseTemporary(self); }
    public static Vector2 GetScreenSizeInPixel(RectTransform rt)
    {
        Vector3[] v = new Vector3[4];
        rt.GetWorldCorners(v);//It starts bottom left and rotates to top left, then top right, and finally bottom right.

        Vector2 size = Camera.main.WorldToScreenPoint(v[2]) - Camera.main.WorldToScreenPoint(v[0]);
        //Vector2 size = (v[2]) - (v[0]);
        return size;
    }
    public static int SqrMagnitude(this int2 t)
    {
        return Math.Abs(t.x) + Math.Abs(t.y);
    }
    public static int Max(this int2 t)
    {
        return Math.Max(Math.Abs(t.x), Math.Abs(t.y));
    }
    public static float Magnitude(this int2 t)
    {
        return Mathf.Sqrt(t.x * t.x + t.y * t.y);
    }

    public static void Swap<T>(ref T a, ref T b)
    {
        T tmp = a;
        a = b;
        b = tmp;
    }

    public static bool AABBPoint(int2 minSqr, int2 maxSqr, int2 point)
    {
        if (point.x < minSqr.x || maxSqr.x < point.x) return false;
        if (point.y < minSqr.y || maxSqr.y < point.y) return false;
        return true;
    }

    public static void ShuffleExceptFirstAndLast<T>(this List<T> list)
    {
        System.Random rnd = new System.Random();
        for (int i = 2; i < list.Count - 1; i++)
        {
            int j = rnd.Next(i - 2) + 1;
            T tmp = list[j];
            list[j] = list[i];
            list[i] = tmp;
        }
    }

    public static IList<T> Shuffle<T>(this IList<T> list, int size)
    {
        System.Random rnd = new System.Random();
        var res = new T[size];

        res[0] = list[0];
        for (int i = 1; i < size; i++)
        {
            int j = rnd.Next(i);
            res[i] = res[j];
            res[j] = list[i];
        }
        return res;
    }

    public static T GetRandom<T>(this IList<T> list)
    {
        System.Random rnd = new System.Random();
        
        return list[rnd.Next(list.Count - 1)];
    }


    public static IList<T> Shuffle<T>(this IList<T> list)
    { return list.Shuffle(list.Count); }


}