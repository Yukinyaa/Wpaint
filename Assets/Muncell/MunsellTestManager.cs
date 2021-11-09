using DG.Tweening;
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

    [SerializeField] private Text _txtNotice;

    float _cellWidth;

    RectTransform RT => transform as RectTransform;


    /// <summary>
    /// score / maxScore ���� �˷���
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
                if (cell.actualIndex > prev) // ���� ���̶� �������� ���ӵǸ� ���� +1
                    ++score;
            prev = cell.actualIndex;
        }
    }
    /// <summary>
    /// �� �������� �� �巡�� ��� ���
    /// </summary>
    public void LockCells()
    {
        foreach (var cell in _cells)
            cell.gameFinished = true;
    }

    /// <summary>
    /// �׽�Ʈ ������ϱ�
    /// </summary>
    public void RegenerateTest()
    {
        // ��� child ����
        if (_cells != null)
            foreach (var img in _cells)
                if(img != null)
                    DestroyImmediate(img.gameObject, false);

        // ��� child ���� 2
        while (transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }

        _cells = new List<MunsellCell>();

        // �� �ʺ� ����
        _cellWidth = (1 + cellMargin) / cellCount - cellMargin;

        // �� ����
        for (int i = 0; i < cellCount; i++)
        {
            MunsellCell cell = Instantiate(munselImagePF, this.transform).GetComponent<MunsellCell>();
            _cells.Add(cell);
            cell.itemImage.color = Color.Lerp(startColor, endColor, i / (cellCount - 1f));
            cell.actualIndex = i;
            cell.manager = this;
            cell.Init();
        }

        // ���� (�̹������� �θ� �°� �� ������)
        OrderCells();

        // ���ø�
        _cells.ShuffleExceptFirstAndLast();
        
        // ������, �̹����� �θ� ���� �Ȱ�
        ReOrderCells();


        // ù��°, ������ ���� �巡�� �ȵǰ� �׳� ��������
        _cells.First().gameObject.AddComponent<Munsell_FixedCell>().Init(ShowNotice);
        _cells.First().enabled = false;
        _cells.Last().gameObject.AddComponent<Munsell_FixedCell>().Init(ShowNotice);
        _cells.Last().enabled = false;
    }

    /// <summary>
    /// cell�� ���콺 ��ġ�� ���� �ű�
    /// </summary>
    public void UpdateIndex(MunsellCell cell, Vector3 mousePosition)
    {
        var cellRT = cell.transform as RectTransform;

        Vector3[] v = new Vector3[4];
        cellRT.GetWorldCorners(v);//It starts bottom left and rotates to top left, then top right, and finally bottom right.


        // �� ���ʿ� ������ �������� ��ĭ
        if (Camera.main.WorldToScreenPoint(v[0]).x > mousePosition.x)
        {
            int idx = _cells.IndexOf(cell);
            if (idx <= 1) return;


            var tmp = _cells[idx];
            _cells[idx] = _cells[idx - 1];
            _cells[idx - 1] = cell;
        }
        // �����ʿ� ������ ���������� ��ĭ
        else if (Camera.main.WorldToScreenPoint(v[2]).x < mousePosition.x)
        {
            int idx = _cells.IndexOf(cell);
            if (idx >= cellCount - 2) return;


            var tmp = _cells[idx];
            _cells[idx] = _cells[idx + 1];
            _cells[idx + 1] = cell;
        }

        // �������� ����Ʈ�� ������Ʈ�����ϱ� ������Ʈ�� ��������
        ReOrderCells();
    }

    /// <summary>
    /// ���� _cells ������ �°� �δµ� �ڽ��� �ȵ���
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
    /// ���� _cells ������ �°� �ΰ� �ڽĵ� ���� �Űܵ�
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

    //�׽�Ʈ ���� ��, ���� ��� ���� ����
    public void OnTestEnd() {
        StartCoroutine(EndRoutine());

        IEnumerator EndRoutine() {
            for(int i = 0; i < _cells.Count; i++) {
                _cells[i].PlayAnim();
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    private Coroutine _noticeRoutine;
    public void ShowNotice() {
        Color targetColor1 = _txtNotice.color;
        Color targetColor2 = targetColor1;
        targetColor1.a = 0;
        targetColor2.a = 1;

        if (_noticeRoutine != null) {
            StopCoroutine(_noticeRoutine);
        }
        _noticeRoutine = StartCoroutine(NoticeShowRoutine());

        IEnumerator NoticeShowRoutine(){
            _txtNotice.DOColor(targetColor2, 0.5f);
            yield return new WaitForSeconds(1f);
            _txtNotice.DOColor(targetColor1, 0.5f);
        }
    }
}
