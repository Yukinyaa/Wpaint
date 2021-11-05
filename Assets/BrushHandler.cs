using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BrushHandler : MonoBehaviour
{
    [SerializeField]
    DrawingCanvasDisplay drawingCanvasDisplay;

    [SerializeField]
    Canvas drawCanvas;

    [SerializeField]
    Image brushImage;
    TrailRenderer tr;

    bool isDrawing;
    Color currentColor;




    public void Start()
    {
        tr = brushImage.GetComponent<TrailRenderer>();
    }

    void Update()
    {
        var mousepos = Input.mousePosition;

        if (Input.GetMouseButtonDown(0))
        {

            var raycastResult = RaycastMouse();
            var palleteMixer = GetFirstTypeFromArray<PalleteMixer>(raycastResult);

            if (palleteMixer != null)
            {
                currentColor = palleteMixer.PickColor(mousepos);
                brushImage.color = currentColor;

                tr.material.color = currentColor;
                Gradient gradient = new Gradient()
                {
                    alphaKeys = new GradientAlphaKey[] { new GradientAlphaKey(1, 0), new GradientAlphaKey(1, 1) },
                    colorKeys = new GradientColorKey[] { new GradientColorKey(currentColor, 0), new GradientColorKey(currentColor, 1) },
                    mode = GradientMode.Fixed
                };

                tr.colorGradient = gradient;
            }
            else if (raycastResult.Exists(a => a.gameObject.GetComponent<ColorSlot>() != null)) { }
            else
            {
                tr.enabled = true;
                isDrawing = true;
            }

        }
        else if (Input.GetMouseButtonUp(0))
        {
            tr.enabled = false;
            isDrawing = false;
        }

        RectTransformUtility.ScreenPointToWorldPointInRectangle(drawingCanvasDisplay.transform as RectTransform, mousepos, Camera.main, out Vector3 worldPoint);
        Vector3 localPoint = drawingCanvasDisplay.transform.InverseTransformPoint(worldPoint);
        if (isDrawing)
        {
            brushImage.transform.SetParent(drawCanvas.transform, true);
            brushImage.transform.position = drawCanvas.transform.TransformPoint(localPoint);
        }
        else
        {
            brushImage.transform.SetParent(drawingCanvasDisplay.transform, true);
            brushImage.transform.localPosition = localPoint;
        }





        static T GetFirstTypeFromArray<T>(List<RaycastResult> raycastResult)
        {
            return raycastResult.Select(a => a.gameObject.GetComponent<T>()).Where(a => a != null).FirstOrDefault();
        }

    }

    //from: https://answers.unity.com/questions/1009987/detect-canvas-object-under-mouse-because-only-some.html?_ga=2.85083865.1727764624.1593864048-1629740874.1572570134
    public List<RaycastResult> RaycastMouse()
    {
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            pointerId = -1,
        };

        pointerData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        //Debug.Log(results.Count);

        return results;
    }




}