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


    // Start is called before the first frame update
    void Start()
    {
        RegenerateTest();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void GetScore(out int score, out int maxScore)
    {
        maxScore = cellCount - 1;
        int prev = 999;
        score = 0;
        foreach (var cell in _cells)
        {
            if(cell.IsDragging == false)
                if (cell.actualIndex > prev)
                    ++score;
            prev = cell.actualIndex;
        }
    }


    public void RegenerateTest()
    {
        if (_cells != null)
            foreach (var img in _cells)
                if(img != null)
                    DestroyImmediate(img.gameObject, false);
        
        while(transform.childCount > 0)
        {
            DestroyImmediate(transform.GetChild(0).gameObject);
        }

        _cells = new List<MunsellCell>();

        _cellWidth = (1 + cellMargin) / cellCount - cellMargin;

        for (int i = 0; i < cellCount; i++)
        {
            MunsellCell cell = Instantiate(munselImagePF, this.transform).GetComponent<MunsellCell>();
            _cells.Add(cell);
            cell.itemImage.color = Color.Lerp(startColor, endColor, i / (cellCount - 1f));
            cell.actualIndex = i;
            cell.manager = this;
        }
        _cells.ShuffleExceptFirstAndLast();
        _cells.First().enabled = false;
        _cells.Last().enabled = false;
        OrderCells();
    }

    public void UpdateIndex(MunsellCell cell, Vector3 mousePosition)
    {
        var cellRT = cell.transform as RectTransform;

        Vector3[] v = new Vector3[4];
        cellRT.GetWorldCorners(v);//It starts bottom left and rotates to top left, then top right, and finally bottom right.


        if (Camera.main.WorldToScreenPoint(v[0]).x > mousePosition.x)
        {
            int idx = _cells.IndexOf(cell);
            if (idx <= 1) return;


            var tmp = _cells[idx];
            _cells[idx] = _cells[idx - 1];
            _cells[idx - 1] = cell;
        }
        else if (Camera.main.WorldToScreenPoint(v[2]).x < mousePosition.x)
        {
            int idx = _cells.IndexOf(cell);
            if (idx >= cellCount - 2) return;


            var tmp = _cells[idx];
            _cells[idx] = _cells[idx + 1];
            _cells[idx + 1] = cell;
        }

        ReOrderCells();
    }

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
}
