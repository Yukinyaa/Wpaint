using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.EventSystems;


[ExecuteInEditMode]
public class ColorSlot : MonoBehaviour, IDragHandler, IEndDragHandler, IPointerClickHandler, IPointerExitHandler, IPointerEnterHandler
{
    [SerializeField]
    Image backgroundImage;
    [SerializeField]
    Image itemImage;
    [SerializeField]
    Color color;
    
    RectTransform rt;


    public void Start()
    {
        rt = GetComponent<RectTransform>();
        if (itemImage != null)
            itemImage.color = color;
    }
    public void Update()
    {
        if(itemImage != null)
            itemImage.color = color;
    }


    public void OnDrag(PointerEventData data)
    {
        itemImage.transform.SetParent(itemImage.canvasRenderer.transform);
        itemImage.transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData data)
    {
        var raycastResult = RaycastMouse();
        var target = raycastResult.Select(a => a.gameObject.GetComponent<PalleteMixer>()).Where(a => a != null).FirstOrDefault();


        if (target != null)
        {
            target.AddColor(data.position, color);
        }

        itemImage.transform.SetParent(this.transform);
        itemImage.transform.localPosition = Vector3.zero;
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

    public void OnPointerClick(PointerEventData data)
    {

    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        //parent.ShowItemText(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //parent.HideItemText(this);
    }
}