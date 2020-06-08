using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class DoubleScrollRect : ScrollRect, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    public ScrollRect parentScroll;

    protected override void Start()
    {
        base.Start();
        parentScroll = transform.parent.parent.parent.parent.GetComponent<ScrollRect>();
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {
        base.OnBeginDrag(eventData);
        parentScroll.OnBeginDrag(eventData);
    }
    public override void OnDrag(PointerEventData eventData)
    {
        base.OnDrag(eventData);
        parentScroll.OnDrag(eventData);
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
        parentScroll.OnEndDrag(eventData);
    }


}
