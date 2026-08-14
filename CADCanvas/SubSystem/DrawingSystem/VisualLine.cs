using CADCanvas.SubSystem.EditerSystem.Component.Tool.Snap;
using System.Windows;
using System.Windows.Media;

namespace CADCanvas.SubSystem.DrawingSystem
{
    /// <summary>
    /// 表示无限延伸的直线的可视化对象
    /// </summary>
    public class VisualLine : GeoVisual
    {
        public Point Start { get; set; } = new Point();

        public Point End { get; set; } = new Point();

        public Color LineColor { get; set; } = Colors.White;

        public double LineWidth { get; set; } = 1.0;

        public override Rect Bounds => Rect.Empty;

        public override void Init()
        {
            _pen = new Pen(new SolidColorBrush(LineColor), LineWidth);
            _pen.StartLineCap = PenLineCap.Square;
            _pen.EndLineCap = PenLineCap.Square;
            _pen.Freeze();
        }

        public override void Draw(DrawingContext dc, IWorldGrid grid)
        {
            dc.DrawLine(_pen, grid.ToScreen(Start), grid.ToScreen(End));
        }

        public override List<SnapPoint> GetSnapPointList()
        {
            List<SnapPoint> result = new List<SnapPoint>();

            // 添加起点、中点、终点
            result.Add(new SnapPoint { Type = SnapType.Endpoint, WorldPoint = Start });
            result.Add(new SnapPoint { Type = SnapType.Midpoint, WorldPoint = new Point((Start.X + End.X) / 2, (Start.Y + End.Y) / 2) });
            result.Add(new SnapPoint { Type = SnapType.Endpoint, WorldPoint = End });

            return result;
        }

        public override List<GeoVisual> SplitByIntersectionPoint(List<Point> pointList) => new List<GeoVisual>();

        public override List<GeoVisual> JointSplitVisual(List<GeoVisual> visualList)
        {
            return new List<GeoVisual>();
        }

        private Pen? _pen = null;
    }
}