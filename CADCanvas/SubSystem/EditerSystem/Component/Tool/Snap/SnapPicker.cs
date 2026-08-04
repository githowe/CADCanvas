using CADCanvas.SubSystem.DrawingSystem;
using System.Windows;

namespace CADCanvas.SubSystem.EditerSystem.Component.Tool.Snap
{
    /// <summary>
    /// 捕捉拾取器
    /// </summary>
    public class SnapPicker
    {
        public static List<SnapPoint> PickSnapPoint(List<GeoVisual> visualList)
        {
            List<SnapPoint> result = new List<SnapPoint>();

            // 先获取静态捕捉点：端点、中点、圆心
            foreach (var visual in visualList)
                result.AddRange(visual.GetSnapPointList());
            // 再获取动态捕捉点：交点

            return result;
        }

        public static List<SnapPoint> PickSnapPoint(List<GeoVisual> visualList, Rect rect)
        {
            List<SnapPoint> result = new List<SnapPoint>();
            // 选获取全部捕捉点
            List<SnapPoint> all = PickSnapPoint(visualList);
            // 再选择在矩形范围内的捕捉点
            foreach (var item in all)
            {
                // 找到一个就退出
                if (rect.Contains(item.WorldPoint))
                {
                    result.Add(item);
                    break;
                }
            }
            return result;
        }
    }
}