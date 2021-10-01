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

    float colorBlobSize = 300;
    // Start is called before the first frame update
    void Start()
    {
        img = GetComponent<RawImage>();

        Vector2 size = GetScreenSize(img.GetComponent<RectTransform>());

        texture2D = new Texture2D((int)size.x, (int)size.y);

        for (int x = 0; x < texture2D.width; x++)
        {
            for (int y = 0; y < texture2D.height; y++)
            {
                texture2D.SetPixel(x, y, new Color(1, 1, 1, 0));
            }
        }
        texture2D.Apply();
        img.texture = texture2D;
    }

    public void AddColor(Vector2 canvasPos, Color color)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(transform as RectTransform, canvasPos, null, out Vector2 localPos);
        localPos += new Vector2(texture2D.width, texture2D.height) / 2;
        Debug.Log(localPos);

        colorPlacements.Add(new ColorBlob { c = color, place = localPos });

        for (int x = 0; x < texture2D.width; x++)
        {
            for (int y = 0; y < texture2D.height; y++)
            {
                Vector2 range = localPos - new Vector2(x, y);
                if (range.sqrMagnitude < colorBlobSize && range.magnitude < colorBlobSize)
                {
                    UpdatePixel(x, y);
                }
            }
        }
        texture2D.Apply();
        img.texture = texture2D;
    }

    public Color PickColor(Vector2 worldPos)
    {
        Vector2 localPos = transform.InverseTransformPoint(worldPos);
        return texture2D.GetPixel((int)worldPos.x, (int)worldPos.y);
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

        //mean
        Color colm = new Color(1 - r / cnt, 1 - g / cnt, 1 - b / cnt);
        //Substractive
        Color cols = new Color(1 - r,1 - g, 1 - b);
        

        texture2D.SetPixel(x, y, cols);
    }
}
