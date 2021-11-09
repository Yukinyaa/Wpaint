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

    public Material material { get { return ri.material; } }
    
    void Start() {
        OnMaterialChanged();
    }
    public void SetMat(Material material) {
        ri.material = null;
        ri.material = material;
        OnMaterialChanged();
    }
    private void OnMaterialChanged() {
        var rt = GetComponent<RectTransform>();
        Vector2 size = GetScreenSizeInPixel(rt);

        canvas.Initalize((int)size.x, (int)size.y);
        ri.material.mainTexture = canvas.GetTexture();
    }

    // Update is called once per frame
    public void ResetCanvas()
    {
        canvas.ResetCanvas();
    }
}
