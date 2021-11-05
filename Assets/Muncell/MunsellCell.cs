using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.EventSystems;

public class MunsellCell: MonoBehaviour, IDragHandler, IEndDragHandler
{
    public Image itemImage;
    public int actualIndex;
    public bool IsDragging { get; private set; } = false;
    public bool gameFinished = false;

    [HideInInspector]
    public MunsellTestManager manager;

    [HideInInspector]


    public override string ToString()
    {
        return $"{actualIndex}:{itemImage.color}";
    }
    private void OnDestroy()
    {
        DestroyImmediate(itemImage.gameObject);
    }
    private void Update()
    {
        if (!IsDragging)
        {
            Vector3 delta = this.transform.position - itemImage.transform.position;
            if (delta.sqrMagnitude < 0.01f)
                itemImage.transform.position = this.transform.position;
            else
                itemImage.transform.position += Time.deltaTime * delta * 10;
        }
        
    }
    public void OnDrag(PointerEventData data)
    {
        if (gameFinished) return;
        IsDragging = true;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(itemImage.transform.parent.transform as RectTransform, Input.mousePosition, Camera.main, out Vector2 point);
        itemImage.transform.localPosition = point;
        manager.UpdateIndex(this, Input.mousePosition);
    }

    public void OnEndDrag(PointerEventData data)
    {
        IsDragging = false;
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