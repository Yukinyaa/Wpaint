using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Master_Drawing : MonoBehaviour {

    //명화 그리기 전용
    [SerializeField] Material _goghMat;
    [SerializeField] Material _monetMat;
    [SerializeField] Material _delaMat;
    [SerializeField] Material _seuratMat;
    [SerializeField] Material _turnerMat;


    [SerializeField] private GameObject _picturesPanel; //명화 5개 있는 패널. 그 자체로 InputBlocking중
    [SerializeField] private Button[] _buttons;
    [SerializeField] private PictureObject[] _pictureObjs;
    [SerializeField] private Image _dim;
    [SerializeField] private GameObject _inputBlocker;

    [SerializeField] private DrawingCanvasDisplay _drawTarget;

    [SerializeField] private Button _btnFinish;
    [SerializeField] private GameObject _finishPanel;
    [SerializeField] private Image _finishDim;
    [SerializeField] private Button _btnFinishAndReset;

    [SerializeField] private Image _imgOriginalView;    //원본 무슨색인지 참고용 오브젝트

    [SerializeField] private bool _isColorWheel = false;

    [SerializeField] private BrushHandler _brushHandler;
    [SerializeField] private PalleteMixer _pallete;

    private void Awake() {
        Input.multiTouchEnabled = false;
    }

    private void Start() {
        _finishPanel.SetActive(false);
        _imgOriginalView.gameObject.SetActive(false);

        if (_isColorWheel) {
            _btnFinish.gameObject.SetActive(false);
            _picturesPanel.SetActive(false);
            return;
        }

        _btnFinish.gameObject.SetActive(true);
        _picturesPanel.SetActive(true);

        for (int i = 0; i < _buttons.Length; i++) {
            int buttonIndex = i;
            _buttons[i].onClick.AddListener(() => { OnClickButton(buttonIndex); });
        }

        _btnFinish.onClick.AddListener(OnClick_Finish);

        _btnFinishAndReset.onClick.AddListener(OnClick_FinishAndReset);
    }
    private int _currentSelectedPics;
    private void OnClickButton(int index) {
        _currentSelectedPics = index;
        _inputBlocker.SetActive(true);
        for (int i = 0; i < _pictureObjs.Length; i++) {
            if (i == index) {
                _pictureObjs[i].OnSelected();
            } else {
                _pictureObjs[i].OnNotSelected();
            }
        }
        _drawTarget.ResetCanvas();
        _pallete.ResetPallete();
        switch (index) {
            case 0:
                _drawTarget.SetMat(_goghMat);
                break;
            case 1:
                _drawTarget.SetMat(_monetMat);
                break;
            case 2:
                _drawTarget.SetMat(_delaMat);
                break;
            case 3:
                _drawTarget.SetMat(_seuratMat);
                break;
            case 4:
                _drawTarget.SetMat(_turnerMat);
                break;
        }
        _imgOriginalView.sprite = _pictureObjs[index].croppedImage;

        StartCoroutine(AnimateRoutine());

        IEnumerator AnimateRoutine() {
            yield return new WaitForSeconds(3f);
            //3초 기다리고
            GameObject croppedObject = _pictureObjs[index].GetCroppedObjectClone;
            croppedObject.transform.GetChild(0).DOLocalMove(Vector3.zero, 0.5f);
            croppedObject.transform.GetChild(1).DOLocalMove(Vector3.zero, 0.5f);
            croppedObject.transform.DOMove(_imgOriginalView.transform.position, 0.5f);
            croppedObject.transform.DOScale(Vector3.one, 0.5f);
            _dim.DOColor(Color.clear, 0.5f);
            yield return new WaitForSeconds(0.5f);
            _picturesPanel.SetActive(false);
            _inputBlocker.SetActive(false);
            _brushHandler.OnClickReset();   //튜토리셋
            _imgOriginalView.gameObject.SetActive(true);
            Destroy(croppedObject);
        }
    }

    private GameObject _finishPicObj;
    private void OnClick_Finish() {

        StartCoroutine(FinishRoutine());

        IEnumerator FinishRoutine() {
            _finishPanel.SetActive(true);
            _btnFinishAndReset.gameObject.SetActive(false);

            _finishDim.color = Color.clear;

            _finishDim.DOColor(Color.black, 0.5f);
            yield return new WaitForSeconds(0.5f);
            PictureObject pic = Instantiate(_pictureObjs[_currentSelectedPics], _finishPanel.transform);
            _finishPicObj = pic.gameObject;
            pic.SetFinish(_drawTarget.material);
            pic.transform.localScale = Vector3.one * 2;

            yield return new WaitForSeconds(2);

            _btnFinishAndReset.gameObject.SetActive(true);
            _btnFinishAndReset.transform.SetAsLastSibling();
        }
    }

    private void OnClick_FinishAndReset() {
        if (_finishPicObj != null) { Destroy(_finishPicObj); } 
        _picturesPanel.SetActive(true);
        _finishPanel.SetActive(false);
        _imgOriginalView.gameObject.SetActive(false);
        _dim.color = Color.black;

        for(int i = 0; i < _pictureObjs.Length; i++) {
            _pictureObjs[i].ResetAll();
        }
    }
}
