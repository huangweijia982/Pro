using UnityEngine;
using UnityEngine.UI;

public class EventPenetrate : MonoBehaviour, ICanvasRaycastFilter
{

    //作为目标点击事件渗透区域
    private GameObject target;

    public void SetTargetImage(GameObject tg)
    {
        target = tg;
    }

    public bool IsRaycastLocationValid(Vector2 sp, Camera eventCamera)
    {
        //没有目标则捕捉事件渗透
        if (target == null)
        {
            return true;
        }

        //在目标范围内做事件渗透
        return !RectTransformUtility.RectangleContainsScreenPoint(target.GetComponent<RectTransform>(),
            sp, eventCamera);
    }

}