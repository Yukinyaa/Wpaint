using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class Munsell_FixedCell : MonoBehaviour, IPointerDownHandler {
    UnityAction _onDownFixedCell;
    public void Init(UnityAction onDownFixedCell) {
        _onDownFixedCell = onDownFixedCell;
    }
    public void OnPointerDown(PointerEventData eventData) {
        _onDownFixedCell?.Invoke();
    }
}
