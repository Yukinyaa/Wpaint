using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class MunsellTestManager : MonoBehaviour
{
    List<MunsellCell> _cells;
    [SerializeField]
    public GameObject munselImagePF;
    [SerializeField]
    public Color startColor, endColor;
    [SerializeField]
    public int cellCount;
    [SerializeField]
    public float cellMargin;

    float _cellWidth;

    RectTransform RT => transform as RectTransform;

    /// <summary>
    /// score / maxScore 점수 알려줌
    /// </summary>
    /// <param name="score"></param>
    /// <param name="maxScore"></param>
    public void GetScore(out int score, out int maxScore)
    {
        maxScore = cellCount - 1;

        int prev = 999;
        score = 0;
        foreach (var cell in _cells)
        {
            if(cell.IsDragging == false)
                if (cell.actualIndex > prev) // 이전 셀이랑 다음셀이 연속되면 점수 +1
                    ++score;
            prev = cell.actualIndex;
        }
    }
    /// <summary>
    /// 겜 끝났을때 셀 드래그 기능 잠금
    /// </summary>
    public void LockCells()
    {
        foreach (var cell in _cells)
            cell.gameFinished = true;
    }

    /// <summary>
    /// 테스트 재시작하기
    /// </summary>
    public void RegenerateTest()
    {
        // 모든 child 제거
        if (_cells != null)
            foreach (var img in _cells)
                if(img != null)
                    DestroyImmediate(img.gameObject, false);

        // 모든 child 제거 2
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }

        _cells = new List<MunsellCell>();

        // 셀 너비 설정
        _cellWidth = (1 + cellMargin) / cellCount - cellMargin;

        // 셀 생성
        for (int i = 0; i < cellCount; i++)
        {
            MunsellCell cell = Instantiate(munselImagePF, this.transform).GetComponent<MunsellCell>();
            _cells.Add(cell);
            cell.itemImage.color = Color.Lerp(startColor, endColor, i / (cellCount - 1f));
            cell.actualIndex = i;
            cell.manager = this;
            cell.Init();
        }

        // 정렬 (이미지까지 부모에 맞게 딱 가도록)
        OrderCells();

        // 셔플링
        _cells.ShuffleExceptFirstAndLast();
        
        // 재정렬, 이미지는 부모 따라 안감
        ReOrderCells();


        // 첫번째, 마지막 셀은 드래그 안되게 그냥 꺼버리기
        _cells.First().enabled = false;
        _cells.Last().enabled = false;
    }

    /// <summary>
    /// cell을 마우스 위치에 따라서 옮김
    /// </summary>
    public void UpdateIndex(MunsellCell cell, Vector3 mousePosition)
    {
        var cellRT = cell.transform as RectTransform;

        Vector3[] v = new Vector3[4];
        cellRT.GetWorldCorners(v);//It starts bottom left and rotates to top left, then top right, and finally bottom right.


        // 셀 왼쪽에 있으면 왼쪽으로 한칸
        if (Camera.main.WorldToScreenPoint(v[0]).x > mousePosition.x)
        {
            int idx = _cells.IndexOf(cell);
            if (idx <= 1) return;


            var tmp = _cells[idx];
            _cells[idx] = _cells[idx - 1];
            _cells[idx - 1] = cell;
        }
        // 오른쪽에 있으면 오른쪽으로 한칸
        else if (Camera.main.WorldToScreenPoint(v[2]).x < mousePosition.x)
        {
            int idx = _cells.IndexOf(cell);
            if (idx >= cellCount - 2) return;


            var tmp = _cells[idx];
            _cells[idx] = _cells[idx + 1];
            _cells[idx + 1] = cell;
        }

        // 위에서는 리스트만 업데이트했으니까 오브젝트도 업덱해줌
        ReOrderCells();
    }

    /// <summary>
    /// 셀을 _cells 순서에 맞게 두는데 자식은 안따라감
    /// </summary>
    public void ReOrderCells()
    {
        for (int i = 0; i < cellCount; i++)
        {
            var t = _cells[i].itemImage.transform.position;
            var cellRT = _cells[i].transform as RectTransform;
            
            cellRT.anchorMin = new Vector2((_cellWidth + cellMargin) * i, 0);
            cellRT.anchorMax = new Vector2((_cellWidth + cellMargin) * i + _cellWidth, 1);
            cellRT.anchoredPosition = Vector2.zero;
            cellRT.sizeDelta = Vector2.zero;
            _cells[i].itemImage.transform.position = t;
        }
    }

    /// <summary>
    /// 셀을 _cells 순서에 맞게 두고 자식도 같이 옮겨둠
    /// </summary>
    public void OrderCells() {
        for (int i = 0; i < cellCount; i++)
        {
            var cellRT = _cells[i].transform as RectTransform;
            cellRT.anchorMin = new Vector2((_cellWidth + cellMargin) * i, 0);
            cellRT.anchorMax = new Vector2((_cellWidth + cellMargin) * i + _cellWidth, 1);
            cellRT.anchoredPosition = Vector2.zero;
            cellRT.sizeDelta = Vector2.zero;
        }
    }

    //테스트 종료 후, 서순 까는 연출 시작
    public void OnTestEnd() {
        StartCoroutine(EndRoutine());

        IEnumerator EndRoutine() {
            for(int i = 0; i < _cells.Count; i++) {
                _cells[i].PlayAnim();
                yield return new WaitForSeconds(0.1f);
            }
        }
    }
}
