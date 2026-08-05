using System.Windows;

namespace CADCanvas.SubSystem.EditerSystem.Component.Tool.Snap
{
    /// <summary>
    /// 捕捉点
    /// </summary>
    public class SnapPoint
    {
        public SnapType Type { get; set; } = SnapType.None;

        public string TypeName => Type switch
        {
            SnapType.Endpoint => "端点",
            SnapType.Midpoint => "中点",
            SnapType.Center => "圆心",
            SnapType.Intersection => "交点",
            SnapType.Tangent => "切点",
            SnapType.Perpendicular => "垂足",
            SnapType.Parallel => "平行点",
            SnapType.Point => "坐标点",
            _ => "未知",
        };

        public Point WorldPoint { get; set; } = new Point();
    }
}