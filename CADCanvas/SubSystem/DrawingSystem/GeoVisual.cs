using CADCanvas.SubSystem.EditerSystem.Component.Tool.RTree;
using CADCanvas.SubSystem.EditerSystem.Component.Tool.Snap;
using System.Windows;
using System.Windows.Media;

namespace CADCanvas.SubSystem.DrawingSystem
{
    /// <summary>
    /// 表示几何图形的可视化对象
    /// </summary>
    public abstract class GeoVisual : IBox
    {
        /// <summary>图形句柄</summary>
        public IntPtr Handle { get; set; } = IntPtr.Zero;

        public abstract Rect Bounds { get; }

        public bool Hidden { get; set; } = false;

        public double Opacity { get; set; } = 1.0;

        public Color LineColor { get; set; } = Colors.White;

        public double LineWidth { get; set; } = 1.0;

        public virtual void Init() { }

        public abstract void Draw(DrawingContext dc, IWorldGrid grid);

        public abstract void DrawHover(DrawingContext dc, IWorldGrid grid);

        public abstract void DrawSelect(DrawingContext dc, IWorldGrid grid);

        /// <summary>
        /// 获取捕捉点列表
        /// </summary>
        public abstract List<SnapPoint> GetSnapPointList();

        /// <summary>
        /// 根据交点列表分割曲线，生成新的曲线对象
        /// </summary>
        public abstract List<GeoVisual> SplitByIntersectionPoint(List<Point> pointList);

        /// <summary>
        /// 拼接分割图形
        /// </summary>
        public abstract List<GeoVisual> JointSplitVisual(List<GeoVisual> visualList);
    }
}