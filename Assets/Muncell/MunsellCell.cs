using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using UnityEngine.EventSystems;

public class MunsellCell: MonoBehaviour, IDragHandler, IEndDragHandler
{
    [SerializeField] private Text _refText;
    [SerializeField] private Animator _refAnimator;
    public Image itemImage; // 실제로 보이는 이미지.
    public int actualIndex; // 제대로 정렬된거 기준
    public bool IsDragging { get; private set; } = false; // 드래그 확인용, 드래그중이면 점수로 안쳐줌
    public bool gameFinished = false; // 겜 끝나면 드래그 잠그기

    [HideInInspector]
    public MunsellTestManager manager;

    public void Init() {
        _refText.text = (actualIndex + 1).ToString();
    }


    public override string ToString() // 디버그용 
    {
        return $"{actualIndex}:{itemImage.color}"; 
    }

    private void Update()
    {
        if (!IsDragging) // 드래그중이 아니라면, child를 끌고옴
        {
            Vector3 delta = this.transform.position - itemImage.transform.position;
            if (delta.sqrMagnitude < 0.01f)
                itemImage.transform.position = this.transform.position;
            else
                itemImage.transform.position += Time.deltaTime * delta * 10;
        }
        
    }

    public void OnDrag(PointerEventData data) // 차일드만 마우스 따라가기
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
    

    public void PlayAnim() {
        _refAnimator.SetTrigger("Open");
    }
}