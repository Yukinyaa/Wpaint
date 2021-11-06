using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(MunsellTestManager))]
public class MunsellGameManager : MonoBehaviour
{
    MunsellTestManager _testManager;
    [SerializeField] Text _timetext;
    [SerializeField] Text _scoretext;
    [SerializeField] List<Button> _retartBtn;

    float _remainingTime, _finishedTime;
    bool _isFinished;
    enum Difficulty { Easy, Normal, Hard }

    static Color ParseHTMLString(string str) { ColorUtility.TryParseHtmlString(str, out Color col); return col; }

    static List<Tuple<Color, Color>> colors;


    
    void Start()
    {
        colors = new List<Tuple<Color, Color>> {// ㄹㅇ 먼셀테스트 색상임
            new Tuple<Color, Color>(ParseHTMLString("#B2766F"), ParseHTMLString("#9D8E48")),
            new Tuple<Color, Color>(ParseHTMLString("#97914B"), ParseHTMLString("#529687")),
            new Tuple<Color, Color>(ParseHTMLString("#4E9689"), ParseHTMLString("#7B84A3")),
            new Tuple<Color, Color>(ParseHTMLString("#8484A3"), ParseHTMLString("#B37673"))
        };
        _testManager = GetComponent<MunsellTestManager>();
        
        
        _retartBtn[0].onClick.AddListener(() => Restart(Difficulty.Easy));
        _retartBtn[1].onClick.AddListener(() => Restart(Difficulty.Normal));
        _retartBtn[2].onClick.AddListener(() => Restart(Difficulty.Hard));

        Restart(Difficulty.Easy); // 스타트 안끊고 돌리면 Update 뻗어서 넣었음.
    }

    void Restart(Difficulty difficulty)
    {
        // 랜덤 먼셀색상(난이도 따라 색은 같은)
        Tuple<Color, Color> targetColor = colors.GetRandom();
        _testManager.startColor = targetColor.Item1;
        _testManager.endColor = targetColor.Item2;


        // 난이도 따라 시간과 카운트만 바꾸기
        switch (difficulty)
        {
            case Difficulty.Easy:
                _remainingTime = 15;
                _testManager.cellCount = 7;
                break;
            case Difficulty.Normal:
                _remainingTime = 30;
                _testManager.cellCount = 10;
                break;
            case Difficulty.Hard:
                _remainingTime = 60;
                _testManager.cellCount = 15;
                break;
        }
        _scoretext.text = "";
        _finishedTime = 0;
        _isFinished = false;

        _testManager.RegenerateTest();
    }
    // Update is called once per frame
    void Update()
    {
        _remainingTime -= Time.deltaTime;
        
        _testManager.GetScore(out int score, out int maxScore);
        
        _scoretext.text = $"점수: {score}/{maxScore}({score * 100f / maxScore:00}%)";

        if (score == maxScore && _isFinished == false)
        {
            _testManager.LockCells();
            _isFinished = true;
            _finishedTime = _remainingTime;
            _remainingTime = 0;
            _scoretext.text = $"점수: {score}/{maxScore}({score * 100f / maxScore:00}%)";

            _testManager.OnTestEnd();
        }

        if (_remainingTime <= 0)
        {
            if (_isFinished == false)
            {
                _isFinished = true;
                _testManager.LockCells();
            }
            
            _timetext.text = $"{_finishedTime:00.000}";
        }
        else
        {
            _timetext.text = $"{_remainingTime:00.000}";
        }
        
    }
}
