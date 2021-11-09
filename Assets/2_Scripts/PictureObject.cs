using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PictureObject : MonoBehaviour {

    [SerializeField] private Image _imageOriginal = default;
    [SerializeField] private Image _imageCropped = default;
    [SerializeField] private Image _croppedOutline = default;
    [SerializeField] private Text _text = default;

    [SerializeField] private Transform _cropppedPos = default;

    [SerializeField] private Transform _masterCanvas = default;

    [SerializeField] private RawImage _croppedRawImage = default;

    public Sprite croppedImage { get { return _imageCropped.sprite; } }
    public GameObject GetCroppedObjectClone {
        get {
            GameObject go = Instantiate(_cropppedPos.gameObject, _masterCanvas);
            go.transform.position = _cropppedPos.position;
            go.transform.localScale = _cropppedPos.localScale * 2;
            return go;
        }
    }

    private Vector3 _originPos;

    private void Awake() {
        _originPos = transform.position;
        ResetAll();
    }

    public void ResetAll() {
        transform.localScale = Vector3.one;
        transform.position = _originPos;
        _imageOriginal.color = Color.white;
        _imageCropped.color = Color.white;
        _croppedOutline.color = Color.clear;

        _croppedOutline.transform.localPosition = Vector3.zero;
        _imageCropped.transform.localPosition = Vector3.zero;
        _text.color = Color.white;
    }

    public void OnSelected() {
        StartCoroutine(AnimateRoutine());

        IEnumerator AnimateRoutine() {
            yield return new WaitForSeconds(0.5f);

            transform.DOMove(Vector3.zero, 0.5f);
            transform.DOScale(Vector3.one * 2, 0.5f);

            yield return new WaitForSeconds(1f);
            _croppedOutline.DOColor(Color.black, 0.5f);
            _imageOriginal.DOColor(Color.grey, 0.5f);

            yield return new WaitForSeconds(0.5f);
            _imageCropped.transform.DOMove(Vector3.zero, 0.5f);
            _imageOriginal.DOColor(Color.clear, 0.5f);
            _croppedOutline.DOColor(Color.clear, 0.5f);

            yield return new WaitForSeconds(1f);
            _imageCropped.DOColor(Color.clear, 0f);
            _text.DOColor(Color.clear, 0.5f);
        }
    }

    public void OnNotSelected() {
        _imageOriginal.DOColor(Color.clear, 0.5f);
        _text.DOColor(Color.clear, 0.5f);
        _imageCropped.color = Color.clear;
    }

    public void DisableMask() {
        GetComponent<Mask>().enabled = false;
    }

    public void SetFinish(Material material) {
        _croppedRawImage.material = material;

        _croppedRawImage.gameObject.SetActive(true); 
    }
}
