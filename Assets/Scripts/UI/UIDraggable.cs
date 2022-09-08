using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class UIDraggable : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Vector2 _dragStartClickPoint;
    private Vector2 _dragStartPosition;

    public void OnBeginDrag(PointerEventData eventData)
    {
        _dragStartClickPoint = eventData.position;
        _dragStartPosition = transform.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = _dragStartPosition + eventData.position - _dragStartClickPoint;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        //?
    }
}
