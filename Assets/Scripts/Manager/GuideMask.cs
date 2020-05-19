using UnityEngine;
using UnityEngine.UI;

public class GuideMask : MonoBehaviour {
    [SerializeField] RawImage _rawImage; //遮罩图片
    [SerializeField] RectTransform _rectTrans;
    Material _materia;
    Canvas _canvas;
    private EventPenetrate ev;

    public static GuideMask Instance
    {
        private set;
        get;
    }

    void Awake()
    {

        Instance = this;
        _materia = _rawImage.material;
        ev = transform.GetComponent<EventPenetrate>();
    }
    /// <summary>
    /// 创建圆形点击区域
    /// </summary>
    /// <param name="target">目标位置</param>
    /// <param name="offeset">大小偏移量</param>
    /// <param name="CallBack">点击的回调</param>
    public void CreateCircleMask(GameObject target, float offset)
    {
        if (target != null && ev != null)
        {
            RectTransform rec = target.GetComponent<RectTransform>();
            CreateCircleMask(GetTargetCenter(rec), GetTargetRad(rec) + offset);
            ev.SetTargetImage(target);
        }
    }


    /// <summary>
    /// 创建圆形点击区域
    /// </summary>
    /// <param name="target">目标位置</param>
    /// <param name="offeset">大小偏移量</param>
    /// <param name="CallBack">点击的回调</param>
    public void CreateCircleMask(GameObject target, float offset,float x,float y)
    {
        if (target != null && ev != null)
        {
            RectTransform rec = target.GetComponent<RectTransform>();
            CreateCircleMask(GetTargetCenter(rec, x, y), GetTargetRad(rec) + offset);
            ev.SetTargetImage(target);
        }
    }


    /// <summary>
    /// 创建圆形点击区域
    /// </summary>
    /// <param name="pos">圆心的屏幕位置</param>
    /// <param name="rad">圆的半径</param>
    /// <param name="CallBack">点击的回调</param>
    public void CreateCircleMask(Vector3 pos, float rad)
    {
        ShowGuideMask();
        _rectTrans.sizeDelta = Vector2.zero;
        _materia.SetFloat("_MaskType", 0f);
        _materia.SetVector("_Origin", new Vector4(pos.x, pos.y, rad, 20));
    }

    /// <summary>
    /// 创建矩形点击区域
    /// </summary>
    /// <param name="obj">目标位置</param>
    /// <param name="CallBack">回调</param>
    public void CreateRectangleMask(GameObject target)
    {
        if (target != null && ev != null)
        {
            RectTransform rec = target.GetComponent<RectTransform>();
            Vector3[] _corners = new Vector3[4];
            rec.GetWorldCorners(_corners);
            Vector2 pos1 = WorldToCanvasPos(_corners[0]);//选取左下角
            Vector2 pos2 = WorldToCanvasPos(_corners[2]);//选取右上角
            CreateRectangleMask(pos1, pos2);
            ev.SetTargetImage(target);
        }
    }

    /// <summary>
    /// 创建矩形点击区域
    /// </summary>
    /// <param name="pos">矩形的屏幕位置</param>
    /// <param name="pos1">左下角位置</param>
    /// <param name="pos2">右上角位置</param>
    /// <param name="CallBack">回调</param>
    public void CreateRectangleMask(Vector3 pos1, Vector3 pos2)
    {
        ShowGuideMask();
        _rectTrans.sizeDelta = Vector2.zero;
        _materia.SetFloat("_MaskType", 1.0f);
        _materia.SetVector("_Origin", new Vector4(pos1.x, pos1.y, pos2.x, pos2.y));
    }

    /// <summary>
    /// 获取对象RectTransform的半径
    /// </summary>
    /// <param name="rect"></param>
    /// <returns></returns>
    public float GetTargetRad(RectTransform rect)
    {
        Vector3[] _corners = new Vector3[4];
        rect.GetWorldCorners(_corners);
        //计算最终高亮显示区域的半径       
        float _radius = Vector2.Distance(WorldToCanvasPos(_corners[0]),
                     WorldToCanvasPos(_corners[2])) / 2f;
        return _radius;
    }

    /// <summary>
    /// 获取对象RectTransform的中心点
    /// </summary>
    /// <param name="rect"></param>
    /// <returns></returns>
    public Vector2 GetTargetCenter(RectTransform rect)
    {
        Vector3[] _corners = new Vector3[4];
        rect.GetWorldCorners(_corners);

        float x = _corners[0].x + ((_corners[3].x - _corners[0].x) / 2f);
        float y = _corners[0].y + ((_corners[1].y - _corners[0].y) / 2f);
        Vector3 centerWorld = new Vector3(x, y, 0);
        Vector2 center = WorldToCanvasPos(centerWorld);
        return center;
    }
    /// <summary>
    /// 获取对象RectTransform的中心点
    /// </summary>
    /// <param name="rect"></param>
    /// <returns></returns>
    public Vector2 GetTargetCenter(RectTransform rect, float _x, float _y)
    {
        Vector3[] _corners = new Vector3[4];
        rect.GetWorldCorners(_corners);

        float x = _corners[0].x + ((_corners[3].x - _corners[0].x) / 2f);
        float y = _corners[0].y + ((_corners[1].y - _corners[0].y) / 2f);
        Vector3 centerWorld = new Vector3(x + _x, y + _y, 0);
        Vector2 center = WorldToCanvasPos(centerWorld);
        return center;
    }


    /// <summary>
    /// 世界坐标向画布坐标转换
    /// </summary>
    /// <param name="world">世界坐标</param>
    /// <returns>返回画布上的二维坐标</returns>
    private Vector2 WorldToCanvasPos(Vector3 world)
    {
        if (null == _canvas) _canvas = transform.GetComponentInParent<Canvas>();
        Vector2 position;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(_canvas.transform as RectTransform,
            world, _canvas.GetComponent<Camera>(), out position);
        return position;
    }
    public  void ShowGuideMask()
    {
        if (_rawImage != null &&!_rawImage.enabled)
        {
            _rawImage.enabled = true;
        }
    }
    /// <summary>
    /// 将可点击区域设置为空
    /// </summary>
    public void SetTargetNil()
    {
        if (ev != null)
        {
            ev.SetTargetImage(null);
        }
    }

    /// <summary>
    /// 关闭引导遮罩
    /// </summary>
    public void CloseGuideMask()
    {
        if (_rawImage != null)
        {
            _rawImage.enabled = false;
        }
    }
    public void OnDestroy()
    {
#if UNITY_EDITOR
        if (_materia != null)
        {
            _materia.SetFloat("_MaskType", 1.0f);
            _materia.SetVector("_Origin", new Vector4(0, 0, 0, 0));
        }
#endif
    }
}
