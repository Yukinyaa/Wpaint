using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static StaticFunc;

[RequireComponent(typeof(RawImage))]
public class DrawingCanvasDisplay : MonoBehaviour
{
    public DrawingCanvas canvas;
    RawImage ri => this.GetComponent<RawImage>();
    void Start()
    {
        var rt = GetComponent<RectTransform>();
        Vector2 size = GetScreenSize(rt);

        canvas.Initalize((int)size.x, (int)size.y);
        ri.texture = canvas.GetTexture();
    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
