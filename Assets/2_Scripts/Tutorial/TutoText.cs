using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TutoText : MonoBehaviour
{
    [SerializeField] private Text _refText;
    private Coroutine _loopRoutine;

    private bool _isOn = false;

    public void On() {
        if (_isOn) return;
        _isOn = true;

        _loopRoutine = StartCoroutine(LoopRoutine());
    }

    public void Off() {
        if (_isOn == false) return;
        _isOn = false;

        if (_loopRoutine != null) {
            StopCoroutine(_loopRoutine);
        }
        _refText.DOKill();
        _refText.DOColor(Color.clear, 0.5f);
    }
    
    private IEnumerator LoopRoutine() {
        Color c1 = Color.white;
        Color c2 = new Color(1, 1, 1, 0.7f);
        while (true) {
            _refText.DOColor(c1, 1f);
            yield return new WaitForSeconds(1f);

            _refText.DOColor(c2, 1f);
            yield return new WaitForSeconds(1f);
        }
    }
}
