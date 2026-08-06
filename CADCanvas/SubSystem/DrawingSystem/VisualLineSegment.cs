using CADCanvas.SubSystem.EditerSystem.Component.Tool.Snap;
using System.Windows;
using System.Windows.Media;

namespace CADCanvas.SubSystem.DrawingSystem
{
    public class VisualLineSegment : GeoVisual
    {
        public Point Start { get; set; } = new Point(0, 0);

        public Point End { get; set; } = new Point(0, 0);

        public Color LineColor { get; set; } = Colors.White;

        public double LineWidth { get; set; } = 1.0;

        public override Rect Bounds
        {
            get
            {
                double x1 = Math.Min(Start.X, End.X);
                double y1 = Math.Min(Start.Y, End.Y);
                double x2 = Math.Max(Start.X, End.X);
                double y2 = Math.Max(Start.Y, End.Y);
                return new Rect(new Point(x1, y1), new Point(x2, y2));
            }
        }

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

        private Pen? _pen = null;
    }
}