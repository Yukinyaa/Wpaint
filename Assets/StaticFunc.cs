using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

}