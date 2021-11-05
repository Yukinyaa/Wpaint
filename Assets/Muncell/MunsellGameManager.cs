using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MunsellTestManager))]
public class MunsellGameManager : MonoBehaviour
{
    MunsellTestManager _testManager;
    [SerializeField] Text _timetext;
    [SerializeField] Text _scoretext;
    [SerializeField] Button _retartBtn;
    // Start is called before the first frame update
    void Start()
    {
        _testManager = GetComponent<MunsellTestManager>();
        
        _retartBtn.onClick.AddListener(Restart);
        
        Restart();
    }

    float _remainingTime, _finishedTime ;
    bool _isFinished;
    void Restart()
    {
        //_testManager.startColor = 뭐시기
        //_testManager.endColor = 저시기
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
