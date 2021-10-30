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
        if (point.x > minSqr.x || maxSqr.x > point.x) return false;
        if (point.y > minSqr.y || maxSqr.y > point.y) return false;
        return true;
    }


    static public IEnumerable<int2> FourD1Point(int2 p)
    {
        yield return new int2(p.x, p.y + 1);
        yield return new int2(p.x, p.y - 1);
        yield return new int2(p.x + 1, p.y);
        yield return new int2(p.x - 1, p.y);
    }
    //http://groups.csail.mit.edu/graphics/classes/6.837/F98/Lecture6/circle.html
    public static IEnumerable<int2> CircleOutlinePoints(int2 Center, int radius)
    {
        int x, y, r2;

        r2 = radius * radius;
        yield return new int2(Center.x, Center.y + radius);
        yield return new int2(Center.x, Center.y - radius);
        yield return new int2(Center.x + radius, Center.y);
        yield return new int2(Center.x - radius, Center.y);

        y = radius;
        x = 1;
        y = (int)(Mathf.Sqrt(r2 - 1) + 0.5);
        while (x < y)
        {
            yield return new int2(Center.x + x, Center.y + y);
            yield return new int2(Center.x + x, Center.y - y);
            yield return new int2(Center.x - x, Center.y + y);
            yield return new int2(Center.x - x, Center.y - y);
            yield return new int2(Center.x + y, Center.y + x);
            yield return new int2(Center.x + y, Center.y - x);
            yield return new int2(Center.x - y, Center.y + x);
            yield return new int2(Center.x - y, Center.y - x);
            x += 1;
            y = (int)(Math.Sqrt(r2 - x * x) + 0.5);
        }
        if (x == y)
        {
            yield return new int2(Center.x + x, Center.y + y);
            yield return new int2(Center.x + x, Center.y - y);
            yield return new int2(Center.x - x, Center.y + y);
            yield return new int2(Center.x - x, Center.y - y);
        }
    }
}