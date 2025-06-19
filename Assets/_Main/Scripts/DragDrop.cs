using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DragDrop : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    public JournalPoint JournalPoint;
    
    private Transform _originalParent;
    private CanvasGroup _canvasGroup;
    private RectTransform _rectTransform;

    private void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        _rectTransform = GetComponent<RectTransform>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        _originalParent = transform.parent;
        transform.SetParent(transform.parent.parent.parent);
        _canvasGroup.blocksRaycasts = false;
        _canvasGroup.alpha = 0.6f;
    }

    public void OnDrag(PointerEventData eventData)
    {
        //transform.position = eventData.position;
        _rectTransform.anchoredPosition += eventData.delta;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        _canvasGroup.blocksRaycasts = true;
        _canvasGroup.alpha = 1f;

        var temp = eventData.pointerEnter?.transform;
        if (temp != null && temp.childCount > 0)
        {
            var dropSlot = temp.GetChild(0).gameObject.GetComponent<BriefSlot>();
            var originalSlot = _originalParent.GetComponent<BriefSlot>();

            if (dropSlot != null)
            {
                transform.SetParent(dropSlot.transform);

                //

                dropSlot.CreateBriefPoint(JournalPoint.ID, JournalPoint.BriefPointItems);
                originalSlot.RemoveBriefPoint(JournalPoint.ID);
            }
            else
            {
                transform.SetParent(_originalParent);
            }
        }
        else
        {
            transform.SetParent(_originalParent);
        }
    }
}
