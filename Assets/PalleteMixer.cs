using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using static StaticFunc;

[RequireComponent(typeof(RawImage))]
public class PalleteMixer : MonoBehaviour
{
    struct ColorBlob
    {
        public Color c;
        public Vector2 place;
    }
    List<ColorBlob> colorPlacements = new List<ColorBlob>();
    RawImage img;
    Texture2D texture2D;

    public enum MixMode { Substractive, Additive, Mean }
    [SerializeField] MixMode mixMode;

    RectTransform rt => transform as RectTransform;
    float colorBlobSize = 300;

    Vector2 sizeInPixel, sizeInWorld;
    Vector3 bottomLeft, topRight;

    // Start is called before the first frame update
    void Start()
    {
        img = GetComponent<RawImage>();

        sizeInPixel = GetScreenSizeInPixel(img.GetComponent<RectTransform>());

        texture2D = new Texture2D((int)sizeInPixel.x, (int)sizeInPixel.y);


        Vector3[] corners = new Vector3[4];
        rt.GetWorldCorners(corners);

        RectTransformUtility.ScreenPointToWorldPointInRectangle(rt, RectTransformUtility.WorldToScreenPoint(Camera.main, corners[0]), Camera.main, out bottomLeft);
        RectTransformUtility.ScreenPointToWorldPointInRectangle(rt, RectTransformUtility.WorldToScreenPoint(Camera.main, corners[2]), Camera.main, out topRight);
        sizeInWorld = topRight - bottomLeft;

        for (int x = 0; x < texture2D.width; x++)
        {
            for (int y = 0; y < texture2D.height; y++)
            {
                texture2D.SetPixel(x, y, new Color(1, 1, 1, 0));
            }
        }
        texture2D.Apply();
        img.texture = texture2D;

        Debug.Log(sizeInPixel);
        Debug.Log(sizeInWorld);


    }

    public void AddColor(Vector2 mousePos, Color color)
    {
        RectTransformUtility.ScreenPointToWorldPointInRectangle(rt, mousePos, Camera.main, out Vector3 localPos);
        
        Vector2 pixelPos = localPos - bottomLeft;
        pixelPos.x *= sizeInPixel.x / sizeInWorld.x;
        pixelPos.y *= sizeInPixel.y / sizeInWorld.y;


        colorPlacements.Add(new ColorBlob { c = color, place = pixelPos });

        for (int x = 0; x < texture2D.width; x++)
        {
            for (int y = 0; y < texture2D.height; y++)
            {
                Vector2 range = (Vector2)pixelPos - new Vector2(x, y);
                if (range.sqrMagnitude < colorBlobSize && range.magnitude < colorBlobSize)
                {
                    UpdatePixel(x, y);
                }
            }
        }
        texture2D.Apply();
        img.texture = texture2D;
    }

    public Color PickColor(Vector2 mousePos)
    {
        RectTransformUtility.ScreenPointToWorldPointInRectangle(rt, mousePos, Camera.main, out Vector3 localPos);

        Vector2 pixelPos = localPos - bottomLeft;
        pixelPos.x *= sizeInPixel.x / sizeInWorld.x;
        pixelPos.y *= sizeInPixel.y / sizeInWorld.y;

        return texture2D.GetPixel((int)pixelPos.x, (int)pixelPos.y);
    }

    void UpdatePixel(int x, int y)
    {
        float r, g, b;
        int cnt = 0;
        r = g = b = 0;
        Vector2 pos = new Vector2(x, y);

        foreach (var colorBlob in colorPlacements)
        {
            Vector2 range = colorBlob.place - new Vector2(x, y);
            
            if (range.sqrMagnitude < colorBlobSize && range.magnitude < colorBlobSize)
            {
                cnt += 1;
                
                r += 1 - colorBlob.c.r;
                g += 1 - colorBlob.c.g;
                b += 1 - colorBlob.c.b;
            }
        }


        switch (mixMode)
        {
            case MixMode.Substractive:
                Color col_s = new Color(1 - r, 1 - g, 1 - b);
                texture2D.SetPixel(x, y, col_s);
                break;
            case MixMode.Additive:
                Color col_a = new Color(cnt - r, cnt - g, cnt - b);
                texture2D.SetPixel(x, y, col_a);
                break;
            case MixMode.Mean:
                Color col_m = new Color(1 - r / cnt, 1 - g / cnt, 1 - b / cnt);
                texture2D.SetPixel(x, y, col_m);
                break;
        }
        
    }
}
