using CADCanvas.SubSystem.EditerSystem.Component.Tool.Snap;
using System.Windows;
using System.Windows.Media;

namespace CADCanvas.SubSystem.DrawingSystem
{
    public class VisualCircle : GeoVisual
    {
        #region 属性

        public Point Center { get; set; } = new Point();

        public double Radius { get; set; } = 0;

        public override Rect Bounds => new Rect(Center.X - Radius, Center.Y - Radius, Radius * 2, Radius * 2);

        #endregion

        #region 公开方法

        public override void Init()
        {
            _pen = new Pen(new SolidColorBrush(LineColor), LineWidth);
            _pen.StartLineCap = PenLineCap.Square;
            _pen.EndLineCap = PenLineCap.Square;
            _pen.Freeze();

            _hoverPen = new Pen(new SolidColorBrush(Color.FromArgb(96, 255, 255, 255)), LineWidth + 4);
            _hoverPen.StartLineCap = PenLineCap.Round;
            _hoverPen.EndLineCap = PenLineCap.Round;
            _hoverPen.Freeze();

            _selectPen = new Pen(new SolidColorBrush(Color.FromArgb(128, 47, 110, 234)), LineWidth + 4);
            _selectPen.StartLineCap = PenLineCap.Round;
            _selectPen.EndLineCap = PenLineCap.Round;
            _selectPen.Freeze();

            _pointBorder.Freeze();
            _pointFill.Freeze();
        }

        public override void Draw(DrawingContext dc, IWorldGrid grid)
        {
            double screenRadius = grid.ToScreenLength(Radius);
            dc.DrawEllipse(null, _pen, grid.ToScreen(Center), screenRadius, screenRadius);
        }

        public override void DrawHover(DrawingContext dc, IWorldGrid grid)
        {
            double screenRadius = grid.ToScreenLength(Radius);
            dc.DrawEllipse(null, _hoverPen, grid.ToScreen(Center), screenRadius, screenRadius);
        }

        public override void DrawSelect(DrawingContext dc, IWorldGrid grid)
        {
            double screenRadius = grid.ToScreenLength(Radius);
            dc.DrawEllipse(null, _selectPen, grid.ToScreen(Center), screenRadius, screenRadius);
            // 绘制圆心点
            DrawPoint(dc, grid.ToScreen(Center, true), _pointFill, _pointBorder);
            // 绘制象限点
            DrawPoint(dc, grid.ToScreen(new Point(Center.X + Radius, Center.Y), true), _pointFill, _pointBorder);
            DrawPoint(dc, grid.ToScreen(new Point(Center.X - Radius, Center.Y), true), _pointFill, _pointBorder);
            DrawPoint(dc, grid.ToScreen(new Point(Center.X, Center.Y + Radius), true), _pointFill, _pointBorder);
            DrawPoint(dc, grid.ToScreen(new Point(Center.X, Center.Y - Radius), true), _pointFill, _pointBorder);
        }

        public override List<SnapPoint> GetSnapPointList()
        {
            List<SnapPoint> result = new List<SnapPoint>();
            result.Add(new SnapPoint() { Type = SnapType.Center, WorldPoint = Center });
            return result;
        }

        public override List<GeoVisual> SplitByIntersectionPoint(List<Point> pointList)
        {


            return new List<GeoVisual>();
        }

        public override List<GeoVisual> JointSplitVisual(List<GeoVisual> visualList)
        {
            return new List<GeoVisual>();
        }

        #endregion

        #region 字段

        private Pen? _pen = null;
        private Pen? _hoverPen = null;
        private Pen? _selectPen = null;
        private readonly Pen _pointBorder = new Pen(Brushes.White, 1);
        private readonly Brush _pointFill = new SolidColorBrush(Color.FromArgb(255, 0, 127, 255));

        #endregion
    }
}