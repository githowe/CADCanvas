using CADCanvas.SubSystem.EditerSystem.Component.Tool.Snap;
using System.Windows;
using System.Windows.Media;

namespace CADCanvas.SubSystem.DrawingSystem
{
    public class VisualCircle : GeoVisual
    {
        public Point Center { get; set; } = new Point();

        public double Radius { get; set; } = 0;

        public Color LineColor { get; set; } = Colors.White;

        public double LineWidth { get; set; } = 1.0;

        public override Rect Bounds => new Rect(Center.X - Radius, Center.Y - Radius, Radius * 2, Radius * 2);

        public override void Init()
        {
            _pen = new Pen(new SolidColorBrush(LineColor), LineWidth);
            _pen.StartLineCap = PenLineCap.Square;
            _pen.EndLineCap = PenLineCap.Square;
            _pen.Freeze();
        }

        public override void Draw(DrawingContext dc, IWorldGrid grid)
        {
            double screenRadius = grid.ToScreenLength(Radius);
            dc.DrawEllipse(null, _pen, grid.ToScreen(Center), screenRadius, screenRadius);
        }

        public override List<SnapPoint> GetSnapPointList()
        {
            List<SnapPoint> result = new List<SnapPoint>();
            result.Add(new SnapPoint() { Type = SnapType.Center, WorldPoint = Center });
            return result;
        }

        private Pen? _pen = null;
    }
}