using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

[RequireComponent(typeof(MunsellTestManager))]
public class MunsellGameManager : MonoBehaviour
{
    MunsellTestManager _testManager;
    [SerializeField] Text _timetext;
    [SerializeField] Text _scoretext;
    [SerializeField] Button _retartBtn;

    float _remainingTime, _finishedTime;
    bool _isFinished;
    enum Difficulty { Easy, Normal, Hard }

    static Color ParseHTMLString(string str) { ColorUtility.TryParseHtmlString(str, out Color col); return col; }

    static List<Tuple<Color, Color>> colors;


    // Start is called before the first frame update
    void Start()
    {
        colors = new List<Tuple<Color, Color>> {// ㄹㅇ 먼셀테스트 색상임
            new Tuple<Color, Color>(ParseHTMLString("#B2766F"), ParseHTMLString("#9D8E48")),
            new Tuple<Color, Color>(ParseHTMLString("#97914B"), ParseHTMLString("#529687")),
            new Tuple<Color, Color>(ParseHTMLString("#4E9689"), ParseHTMLString("#7B84A3")),
            new Tuple<Color, Color>(ParseHTMLString("#8484A3"), ParseHTMLString("#B37673"))
        };
        _testManager = GetComponent<MunsellTestManager>();
        
        _retartBtn.onClick.AddListener(Restart);
        
        Restart();
    }

    void Restart()
    {
        Tuple<Color, Color> targetColor = colors.GetRandom();
        
        
        _testManager.startColor = targetColor.Item1;
        _testManager.endColor = targetColor.Item2;
        //_testManager.cellCount = 난이도조정

        _testManager.RegenerateTest();


        _remainingTime = 30;
        _scoretext.text = "";
        _finishedTime = 0;
        _isFinished = false;
    }
    // Update is called once per frame
    void Update()
    {
        _remainingTime -= Time.deltaTime;
        
        _testManager.GetScore(out int score, out int maxScore);
        
        _scoretext.text = $"점수: {score}/{maxScore}({score * 100f / maxScore:00}%)";

        if (score == maxScore && _isFinished == false)
        {
            _testManager.FinishGame();
            _isFinished = true;
            _finishedTime = _remainingTime;
            _remainingTime = 0;
            _scoretext.text = $"점수: {score}/{maxScore}({score * 100f / maxScore:00}%)";
        }

        if (_remainingTime <= 0)
        {
            _timetext.text = $"{_finishedTime:00.000}";
        }
        else
        {
            _timetext.text = $"{_remainingTime:00.000}";
        }
        
    }
}
