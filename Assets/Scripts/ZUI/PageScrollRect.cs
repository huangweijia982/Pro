using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

/// <summary>
///在脚本初始化时需要调用initData，动态加载类型的传进来相应的获取信息的函数，在设置完item后还需要调用initNextPage，传递是否是最后一页
/// </summary>
public class PageScrollRect : ScrollRect
{
    public Button prePageBtn;
    public Button nextPageBtn;
    public Text pageTxt;
    public GridLayoutGroup contentGG;//需带有GridLayoutGroup
    public LoadItemMode loadItemMode = LoadItemMode.dynamic;//加载的模式
    [Tooltip("如果是动态加载的该值设置为0")]
    public int pageNum = 0;//最大页数
    public int curPage = 1;
    public float[] interval;
    public SlidMode slidMode = SlidMode.vertical;
    public bool needMove;
    public float slidSpeed = 5;
    public Action onPagingEnd;//拖拽结束的事件
    public float startDragNorPos;//开始拖拽时的坐标
    public Action getInfoEvent;//获取信息的事件
    public float pageSize;//横向滑动为页面的y值，纵向为x值
    [Range(0, 1)]
    public float dragSensitive = 0.1f;
    public bool allPageIsInit = false;//所有的页面都加载完毕
    public float curDragPos;
    public MovementType _movementType = MovementType.Elastic;

    /// <summary>
    /// 初始化
    /// </summary>
    /// <param name="initPage">初始页数</param>
    /// <param name="onGetInfo">如果是普通类型的加载该事件直接为空就好，
    /// 否则获取信息事件,需要保证每次拿的数据个数和每一页的个数相等</param>
    ///  <param name="pagingEnd">拖拽结束后事件</param>
    public void initData(Action getInfoEvent = null)
    {
        if (pageTxt != null)
            pageTxt.text = curPage.ToString();
        if (prePageBtn != null)
        {
            prePageBtn.interactable = false;
            prePageBtn.onClick.AddListener(() =>
            {
                if (!allPageIsInit)
                    prePageBtn.interactable = false;
                jumpToPage(curPage - 1);
            });
        }
        if (nextPageBtn != null)
        {
            nextPageBtn.onClick.AddListener(() =>
            {
                if (!allPageIsInit)
                    nextPageBtn.interactable = false;
                jumpToPage(curPage + 1);
            });
        }
        horizontal = slidMode == SlidMode.horizontal;
        vertical = slidMode == SlidMode.vertical;
        movementType = _movementType;
        inertia = false;
        if (getInfoEvent != null)
        {
            this.getInfoEvent = getInfoEvent;
            getInfoEvent();
        }
    }

    public override void OnBeginDrag(PointerEventData eventData)
    {

        if (slidMode == SlidMode.horizontal) startDragNorPos = content.anchoredPosition.x;
        else startDragNorPos = content.anchoredPosition.y;
        base.OnBeginDrag(eventData);
    }

    public override void OnDrag(PointerEventData eventData)
    {
        needMove = false;
        base.OnDrag(eventData);
    }

    public override void OnEndDrag(PointerEventData eventData)
    {
        base.OnEndDrag(eventData);
        float curDragPos;
        if (slidMode == SlidMode.horizontal) curDragPos = content.anchoredPosition.x;
        else curDragPos = content.anchoredPosition.y;
        float dis = startDragNorPos - curDragPos;
        if (Math.Abs(dis) < dragSensitive * pageSize)
        {
            jumpToPage(curPage);
            return;
        }

        int target = curPage;
        if (Math.Abs(dis) < pageSize)
        {
            if ((dis < 0&&slidMode==SlidMode.vertical)||(dis>0&&slidMode==SlidMode.horizontal)) target += 1;
            else  target -= 1;
        }
        else
        {
            target = (int)(Math.Abs(curDragPos) / pageSize) + 1;
            float remain = Math.Abs(curDragPos) % pageSize;
            if (remain > pageSize / 2)
            {
                target += 1;
            }
        }
        jumpToPage(target);

    }

    /// <summary>
    /// 跳转到第几页
    /// </summary>
    /// <param name="page"></param>
    private void jumpToPage(int page)
    {
        if (allPageIsInit)
        {
            page = page > pageNum ? pageNum : page;
            if (nextPageBtn != null)
                nextPageBtn.interactable = page != pageNum;
        }
        page = page < 1 ? 1 : page;
        page = page >pageNum ? pageNum : page;
        if (prePageBtn != null)
            prePageBtn.interactable = page != 1;
        if (loadItemMode == LoadItemMode.normal)
        {
            if (nextPageBtn != null)
                nextPageBtn.interactable = page != pageNum;
        }
        curPage = page;
        if (pageTxt != null)
            pageTxt.text = curPage.ToString();
        if (curPage == pageNum)
        {
            if (getInfoEvent != null)
                getInfoEvent();
        }
        needMove = true;

    }
    private void Update()
    {

        if (needMove)
        {
            if (slidMode == SlidMode.horizontal)
            {
                content.anchoredPosition = new Vector2(Mathf.Lerp(content.anchoredPosition.x, -(curPage - 1) * pageSize, slidSpeed * Time.deltaTime), content.anchoredPosition.y);
                if (Math.Abs(content.anchoredPosition.x + (curPage - 1) * pageSize) <= 0.01f)
                {
                    needMove = false;
                    content.anchoredPosition = new Vector2(-(curPage - 1) * pageSize, content.anchoredPosition.y);
                    if (onPagingEnd != null) onPagingEnd();
                }
            }
            else
            {
                content.anchoredPosition = new Vector2(content.anchoredPosition.x, Mathf.Lerp(content.anchoredPosition.y, (curPage - 1) * pageSize, slidSpeed * Time.deltaTime));
                if (Math.Abs(content.anchoredPosition.y - (curPage - 1) * pageSize) <= 0.01f)
                {
                    needMove = false;
                    content.anchoredPosition = new Vector2(content.anchoredPosition.x, (curPage - 1) * pageSize);
                    if (onPagingEnd != null) onPagingEnd();
                }
            }
        }
    }

    /// <summary>
    /// 初始化下一页，每次消息返回后都要调用，
    /// </summary>
    /// <param name="isLastPage">是否是最后一页了</param>
    public void initNextPage(bool isLastPage)
    {
        if (isLastPage)
        {
            getInfoEvent = null;
            allPageIsInit = true;
        }
        else
        {
            pageNum += 1;
            if (curPage == pageNum)
            {
                if (getInfoEvent != null)
                    getInfoEvent();
            }
        }
        if (nextPageBtn != null)
            nextPageBtn.interactable = !isLastPage;
    }
    public enum SlidMode { horizontal, vertical }
    public enum LoadItemMode { normal, dynamic }

}
