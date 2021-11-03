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
    const int ClipAlphaPass = 0;
    const int BlurPass = 1;
    const int CustomSubtractivePass = 2;
    const int CustomAdditivePass = 3;
    const int AlphaClippingPass = 4;
    const int ColorAddingPass = 5;

    [SerializeField] float _superSampleRatio = 2;



    RawImage _img;


    public Shader avgBloomShader;

    public float paintBlobSize = 300;
    public float paintBlobAmount = 10;
    public float paintSpreadSpeed = 0.1f;

    public int PaintBlobSize => (int)(paintBlobSize * _superSampleRatio);
    public float PaintBlobAmount => paintBlobAmount;

    public int PaintBlobOutlinePixelCount => PaintBlobSize * 4 - 4;
    public float AlphaMultiplier = 1;

    RectTransform RT => transform as RectTransform;

    int2 _sizeInPixel;
    Vector2 _sizeInWorld;
    Vector3 _bottomLeft, _topRight;
    RenderTexture _texture;
    RenderTexture _texture_rendering;
    RenderTexture fuck;

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

    public RenderTexture GetNewRenderTexture()
    {
        RenderTexture texture;
        if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBFloat))
            texture = RenderTexture.GetTemporary(_sizeInPixel.x, _sizeInPixel.y, 0, RenderTextureFormat.ARGBFloat);
        else if (SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGB64))
            texture = RenderTexture.GetTemporary(_sizeInPixel.x, _sizeInPixel.y, 0, RenderTextureFormat.ARGBFloat);
        else
            texture = RenderTexture.GetTemporary(_sizeInPixel.x, _sizeInPixel.y, 0);

        texture.antiAliasing = 1;
        UnityEngine.RenderTexture.active = texture;
        GL.Clear(true, true, new Color(1,1,1,0));

        return texture;
    }


    // Start is called before the first frame update
    public void Start()
    {
        _img = GetComponent<RawImage>();

        var sizeInPixelf = GetScreenSizeInPixel(_img.GetComponent<RectTransform>()) * _superSampleRatio;
        _sizeInPixel = new int2((int)sizeInPixelf.x, (int)sizeInPixelf.y);

        _texture_rendering = GetNewRenderTexture();
        _texture = GetNewRenderTexture();

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


    //[SerializeField]
    Material mat;

    [SerializeField]
    RawImage DBGIMG;

    public void SetDBGImage(RenderTexture img)
    {
        fuck?.ReleaseTemp();
        fuck = GetNewRenderTexture();
        Graphics.CopyTexture(img, fuck);
        DBGIMG.texture = fuck;
    }

    public void LateUpdate()
    {
        SpriteRenderer r;
        if (mat == null)
        {
            mat = new Material(avgBloomShader);
            mat.hideFlags = HideFlags.HideAndDontSave;
        }
        bool justAdded = false;
        if (_addColor)
        {
            justAdded = true;
            RenderTexture tex_source = _texture;
            mat.SetColor("_addColor", _addColorCol);
            mat.SetVector("_addColor_pos", _addColorTo);
            Debug.Log(_addColorTo);

            mat.SetFloat("_blobSize", 0.1f);
            mat.SetFloat("_dspl_brush_size", 0.1f);


            RenderTexture tex_dest = GetNewRenderTexture();

            Graphics.Blit(tex_source, tex_dest, mat, 5);
            Debug.Log("blit");
            tex_source.ReleaseTemp();
            _addColor = false;

            _texture = tex_dest;
        }

        RenderTexture rt = UnityEngine.RenderTexture.active;

        RenderTexture alpha = GetNewRenderTexture();
        RenderTexture blur = GetNewRenderTexture();


        mat.SetTexture("_SourceTex", _texture);

        Graphics.Blit(_texture, alpha, mat, ClipAlphaPass);



        Graphics.Blit(alpha, blur, mat, BlurPass);


        RenderTexture tex_sub = GetNewRenderTexture();
        RenderTexture tex_add = GetNewRenderTexture();


        mat.SetTexture("_SourceTex", _texture );
        Graphics.Blit(alpha, tex_sub, mat, CustomSubtractivePass);

        if (justAdded)
            SetDBGImage(tex_sub);


        _texture.ReleaseTemp(); alpha.ReleaseTemp();

        mat.SetTexture("_SourceTex", tex_sub);
        Graphics.Blit(blur, tex_add, mat, 3);


        blur.ReleaseTemp(); tex_sub.ReleaseTemp();

        _texture = tex_add;
        _texture_rendering.ReleaseTemp();
        _texture_rendering = GetNewRenderTexture();

        Graphics.Blit(_texture, _texture_rendering, mat, 4);


        _img.texture = _texture_rendering;
        
    }
    bool _addColor = false;
    Vector2 _addColorTo;
    Color _addColorCol;
    public void AddColor(Vector2 mousePos, Color color_)
    {
        MColor color = new MColor(color_.r, color_.g, color_.b, color_.a);
        RectTransformUtility.ScreenPointToWorldPointInRectangle(RT, mousePos, Camera.main, out Vector3 localPos);

        int2 pixelPos = MousePosToPixelpos(mousePos);

        _addColor = true;
        _addColorTo = new Vector2(pixelPos.x /(float)_sizeInPixel.x, pixelPos.y / (float)_sizeInPixel.y);
        _addColorCol = color_;
    }

    public bool IsPointInT2DRange(int2 p)
    {
        return AABBPoint(new int2(0, 0), _sizeInPixel, p);
    }


    public Color PickColor(Vector2 mousePos)
    {
        
        int2 pixelPos = MousePosToPixelpos(mousePos);
        if (!AABBPoint(int2.zero, _sizeInPixel - new int2(1, 1), pixelPos))
            return Color.clear;


        UnityEngine.Profiling.Profiler.BeginSample(">>>tlqkf<<<");
        Texture2D texture = new Texture2D(1,1);
        
        RenderTexture.active = _texture;

        
        texture.ReadPixels(new Rect(pixelPos.x, _sizeInPixel.y - pixelPos.y, 1,1), 0, 0);
        
        Color color = texture.GetPixel(0, 0);
        //Debug.Log($"{color}  {pixelPos}");
        Destroy(texture);
        UnityEngine.Profiling.Profiler.EndSample();
        return color;
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
        _lastImgPos = Vector2.zero;
    }

    void IPointerDownHandler.OnPointerDown(PointerEventData eventData)
    {
        FindObjectOfType<IndieStudio.DrawingAndColoring.Logic.GameManager>().SetToolColor(PickColor(eventData.position));
    }


    Vector2 _lastImgPos;
    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        FindObjectOfType<IndieStudio.DrawingAndColoring.Logic.GameManager>().SetToolColor(PickColor(eventData.position));
        
        var pixelPos = MousePosToPixelpos(Input.mousePosition);
        

        if (_isDragging)
        {
            
            var tex_source = _texture;
            var ImgPos = new Vector2(pixelPos.x / (float)_sizeInPixel.x, pixelPos.y / (float)_sizeInPixel.y);
            mat.SetVector("_dspl_from", _lastImgPos);
            mat.SetVector("_dspl_to", ImgPos);
            if (_lastImgPos == Vector2.zero)
            {
                _lastImgPos = ImgPos;
                return;
            }
            

            _lastImgPos = ImgPos;

            RenderTexture tex_dest = GetNewRenderTexture();

            Graphics.Blit(tex_source, tex_dest, mat, 6);
            tex_source.ReleaseTemp();
            _addColor = false;

            _texture = tex_dest;
        }
    }

    void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
    {
        _isDragging = false;
        _lastImgPos = Vector2.zero;
    }
}
