using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.EventSystems;

using static StaticFunc;
using Unity.Mathematics;
using System;
using UnityEngine.Profiling;

[RequireComponent(typeof(RawImage))]
public class PalleteMixer : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IPointerExitHandler, IPointerDownHandler
{
    [SerializeField] float _superSampleRatio = 2;
    RawImage _img;
    

    public float paintBlobSize = 300;
    public float paintBlobAmount = 10;
    public float paintSpreadSpeed = 0.1f;
    public int PaintBlobSize => (int)(paintBlobSize * _superSampleRatio);
    public float PaintBlobAmount => paintBlobAmount;
    public int PaintBlobOutlinePixelCount => PaintBlobSize * 4 - 4;
    public float AlphaMultiplier = 1;

    RectTransform RT => transform as RectTransform;

    int2  _sizeInPixel;
    Vector2 _sizeInWorld;
    Vector3 _bottomLeft, _topRight;

    float[,] _colorWeight;
    float[,] _colorWeightBuffer;
    MColor[,] _colors;
    MColor[,] _colorsBuffer;
    Color[] _flattenedColors;
    Texture2D _texture2D;

    struct MColor 
    { 
        public float r, g, b, a;
        public MColor(float r, float g, float b, float a)
        {
            this.r = r;
            this.g = g;
            this.b = b;
            this.a = a;
        }
        public static implicit operator Color(MColor col)
        {
            return new Color(col.r, col.g, col.b, col.a);
        }
    }



    // Start is called before the first frame update
    public void Start()
    {
        _img = GetComponent<RawImage>();

        var sizeInPixelf = GetScreenSizeInPixel(_img.GetComponent<RectTransform>()) * _superSampleRatio;
        _sizeInPixel = new int2((int)sizeInPixelf.x, (int)sizeInPixelf.y);

        _texture2D = new Texture2D(_sizeInPixel.x, _sizeInPixel.y);
        _colors = new MColor[_sizeInPixel.x, _sizeInPixel.y];
        _colorWeight = new float[_sizeInPixel.x, _sizeInPixel.y];

        _colorsBuffer = new MColor[_sizeInPixel.x, _sizeInPixel.y];
        _colorWeightBuffer = new float[(int)_sizeInPixel.x, (int)_sizeInPixel.y];

        _flattenedColors = new Color[_sizeInPixel.x * _sizeInPixel.y];

        Vector3[] corners = new Vector3[4];
        RT.GetWorldCorners(corners);

        RectTransformUtility.ScreenPointToWorldPointInRectangle(RT, RectTransformUtility.WorldToScreenPoint(Camera.main, corners[0]), Camera.main, out _bottomLeft);
        RectTransformUtility.ScreenPointToWorldPointInRectangle(RT, RectTransformUtility.WorldToScreenPoint(Camera.main, corners[2]), Camera.main, out _topRight);

        Debug.Log($"Dimension:{ _sizeInPixel }");
        _sizeInWorld = _topRight - _bottomLeft;
        
        //for (int x = 0; x < _sizeInPixel.x; x++)
        //{
        //    for (int y = 0; y < _sizeInPixel.y; y++)
        //    {
        //        _texture2D.SetPixel(x, y, new MColor(1, 1, 1, 0));
        //        _colorWeight[x, y] = 0;
        //        _texture2DBuffer.SetPixel(x, y, new MColor(1, 1, 1, 0));
        //        _colorWeightBuffer[x, y] = 0;
        //    }
        //}
    }

    

    public void LateUpdate()
    {
        Profiler.BeginSample("Spread");
        for (int y = 0; y < _sizeInPixel.y; y++)
        {
            for (int x = 0; x < _sizeInPixel.x; x++)
            {
                _colorsBuffer[x, y] = _colors[x, y];
                _colorWeightBuffer[x, y] = 0;
                float w;

                if (_colorWeight[x, y] > 2)
                {
                    _colorWeightBuffer[x, y] = _colorWeight[x, y] * (1f - paintSpreadSpeed * Time.deltaTime);
                }
                else
                {
                    _colorWeightBuffer[x, y] = _colorWeight[x, y];
                }

                for (int i = 0;i < 4;++i)
                {
                    int2 p;
                    switch (i)
                    {
                        default:
                        case 0:
                            p = new int2(x, y + 1);
                            break;
                        case 1:
                            p = new int2(x, y - 1);
                            break;
                        case 2:
                            p = new int2(x + 1, y);
                            break;
                        case 3:
                            p = new int2(x - 1, y);
                            break;
                    }
                    if (IsPointInT2DRange(p) == false)
                        continue;
                    if (_colorWeightBuffer[p.x, p.y] > 2)
                        MixColorToBuffer(_colorsBuffer[p.x, p.y], _colorWeightBuffer[p.x, p.y] * paintSpreadSpeed * Time.deltaTime, x, y);
                    else
                        MixColorToBuffer(_colorsBuffer[p.x, p.y], _colorWeightBuffer[p.x, p.y] * (1f - paintSpreadSpeed * Time.deltaTime), x, y);
                }
                if ((w = _colorWeightBuffer[x, y]) < AlphaMultiplier)
                _colors[x, y].a = w / AlphaMultiplier;
                _flattenedColors[x + y * _sizeInPixel.x] = _colorsBuffer[x, y];
            }
        }
        Profiler.EndSample();



        //commit
        Swap(ref _colorsBuffer, ref _colors);
        Swap(ref _colorWeightBuffer, ref _colorWeight);

        Profiler.BeginSample("Apply");
        _texture2D.SetPixels(_flattenedColors);
        _texture2D.Apply();
        _img.texture = _texture2D;
        Profiler.EndSample();

    }

    void MixColorToBuffer(MColor c, float cw, int x, int y)
    {
        MColor oCol = _colorsBuffer[x, y];
        float oW = _colorWeightBuffer[x, y];

        MixColor(c, cw, oCol, oW, out oCol, out oW);

        _colorWeightBuffer[x, y] = oW;
        _colorsBuffer[x, y] = oCol;
    }

    public void AddColor(Vector2 mousePos, Color color_)
    {
        MColor color = new MColor(color_.r, color_.g, color_.b, color_.a);
        RectTransformUtility.ScreenPointToWorldPointInRectangle(RT, mousePos, Camera.main, out Vector3 localPos);

        int2 pixelPos = MousePosToPixelpos(mousePos);

        MColor mixedColor = new MColor();
        float mixedColorw = 0;

        int2 t2dDimension = _sizeInPixel;

        for (int x = 0; x < t2dDimension.x; x++)
        {
            for (int y = 0; y < t2dDimension.y; y++)
            {
                int2 range = pixelPos - new int2(x, y);
                float magnitude;
                if (range.Max() < PaintBlobSize && (magnitude = range.Magnitude()) < PaintBlobSize)
                {
                    MixColor(mixedColor, mixedColorw, _colors[x, y], _colorWeight[x, y], out mixedColor, out mixedColorw);
                    _colorWeight[x, y] = magnitude / PaintBlobSize * PaintBlobAmount;
                    _colors[x, y] = color;
                }
            }
        }
        foreach(int2 p in CircleOutlinePoints(pixelPos, PaintBlobSize))
        {
            if (IsPointInT2DRange(p) == false)
                continue;
            _colorWeight[p.x, p.y] = mixedColorw / PaintBlobOutlinePixelCount;
            _colors[p.x, p.y] = mixedColor;
        }
    }

    public bool IsPointInT2DRange(int2 p)
    {
        return AABBPoint(new int2(0, 0), _sizeInPixel, p);
    }


    public Color PickColor(Vector2 mousePos)
    {
        int2 pixelPos = MousePosToPixelpos(mousePos);
        return _colors[pixelPos.x, pixelPos.y];
    }

    public int2 MousePosToPixelpos(Vector2 mousePos)
    {
        RectTransformUtility.ScreenPointToWorldPointInRectangle(RT, mousePos, Camera.main, out Vector3 localPos);
        Vector2 pixelPos = localPos - _bottomLeft;
        pixelPos.x *= _sizeInPixel.x / _sizeInWorld.x;
        pixelPos.y *= _sizeInPixel.y / _sizeInWorld.y;
        
        return new int2((int)pixelPos.x, (int)pixelPos.y);
    }

    bool _isDragging = false;

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
    {
        _isDragging = true;
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData)
    {
        _isDragging = false;
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        FindObjectOfType<IndieStudio.DrawingAndColoring.Logic.GameManager>().SetToolColor(PickColor(eventData.position));
    }


    Vector2 _lastPixelPos;
    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        FindObjectOfType<IndieStudio.DrawingAndColoring.Logic.GameManager>().SetToolColor(PickColor(eventData.position));
        if (_isDragging)
        {
            Debug.Log("Draggin");
        }
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        _isDragging = false;
    }



    static void MixColor(MColor a, float aw, MColor b, float bw, out MColor outc, out float outw)
    {
        outw = aw * bw;

        outc.r = a.r * aw + b.r * bw / outw;
        outc.g = a.g * aw + b.g * bw / outw;
        outc.b = a.b * aw + b.b * bw / outw;

        outc.a = 1;
    }
}
