using CADCanvas.SubSystem.EditerSystem.Component.Tool.Snap;
using System.Windows;
using System.Windows.Media;
using XLogic.Wpf;

namespace CADCanvas.SubSystem.DrawingSystem
{
    public class VisualArc : GeoVisual
    {
        #region 属性

        public Point Center { get; set; } = new Point();

        public double Radius { get; set; } = 0;

        public double StartRadian { get; set; } = 0;

        public double EndRadian { get; set; } = 0;

        public SweepDirection Direction { get; set; } = SweepDirection.Counterclockwise;

        public override Rect Bounds
        {
            get
            {
                if (Radius <= 0)
                    return new Rect(Center, Center);

                List<Point> points = new List<Point>
                {
                    GetPointAtAngle(StartRadian),
                    GetPointAtAngle(EndRadian)
                };

                double[] quadrantAngles = new double[]
                {
                    0,
                    Math.PI / 2,
                    Math.PI,
                    Math.PI * 3 / 2
                };

                foreach (double angle in quadrantAngles)
                {
                    if (IsAngleOnArc(angle))
                    {
                        points.Add(GetPointAtAngle(angle));
                    }
                }

                double minX = points.Min(p => p.X);
                double minY = points.Min(p => p.Y);
                double maxX = points.Max(p => p.X);
                double maxY = points.Max(p => p.Y);

                return new Rect(new Point(minX, minY), new Point(maxX, maxY));
            }
        }

        #endregion

        #region 公开方法

        public override string ToString() => $"{StartRadian.ToAngle()} -> {EndRadian.ToAngle()}";

        public override void Init()
        {
            _pen = new Pen(new SolidColorBrush(LineColor), LineWidth);
            _pen.StartLineCap = PenLineCap.Square;
            _pen.EndLineCap = PenLineCap.Square;
            _pen.Freeze();
        }

        public override void Draw(DrawingContext dc, IWorldGrid grid)
        {
            DrawArc(dc, grid, _pen);
        }

        public override void DrawHover(DrawingContext dc, IWorldGrid grid)
        {

        }

        public override void DrawSelect(DrawingContext dc, IWorldGrid grid)
        {

        }

        public override List<SnapPoint> GetSnapPointList()
        {
            return new List<SnapPoint>();
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

        #region 私有方法

        private void DrawArc(DrawingContext dc, IWorldGrid grid, Pen pen)
        {
            // 计算半径
            double screenRadius = grid.ToScreenLength(Radius);
            // 计算起点和终点坐标
            Point start = new Point
            {
                X = Center.X + Radius * Math.Cos(StartRadian),
                Y = Center.Y + Radius * Math.Sin(StartRadian)
            };
            Point end = new Point
            {
                X = Center.X + Radius * Math.Cos(EndRadian),
                Y = Center.Y + Radius * Math.Sin(EndRadian)
            };
            Point screenStart = grid.ToScreen(start);
            Point screenEnd = grid.ToScreen(end);
            // 判断旋转方向
            if (StartRadian > EndRadian) Direction = SweepDirection.Clockwise;
            // 创建路径
            PathGeometry pathGeometry = new PathGeometry();
            PathFigure pathFigure = new PathFigure { StartPoint = screenStart };
            // 创建圆弧
            ArcSegment arcSegment = new ArcSegment
            {
                Point = screenEnd,
                Size = new Size(screenRadius, screenRadius),
                SweepDirection = Direction,
                IsLargeArc = Math.Abs(EndRadian - StartRadian) > Math.PI,
            };
            // 添加圆弧到路径
            pathFigure.Segments.Add(arcSegment);
            pathGeometry.Figures.Add(pathFigure);
            // 绘制路径
            dc.DrawGeometry(null, pen, pathGeometry);
        }

        private Point GetPointAtAngle(double angle)
        {
            return new Point(
                Center.X + Radius * Math.Cos(angle),
                Center.Y + Radius * Math.Sin(angle));
        }

        private bool IsAngleOnArc(double angle)
        {
            double start = NormalizeAngle(StartRadian);
            double end = NormalizeAngle(EndRadian);
            double current = NormalizeAngle(angle);

            if (Direction == SweepDirection.Counterclockwise)
            {
                if (end < start)
                    end += Math.PI * 2;

                if (current < start)
                    current += Math.PI * 2;

                return current >= start && current <= end;
            }

            if (start < end)
                start += Math.PI * 2;

            if (current > start)
                current -= Math.PI * 2;

            return current <= start && current >= end;
        }

        private double NormalizeAngle(double angle)
        {
            double result = angle % (Math.PI * 2);
            if (result < 0)
                result += Math.PI * 2;

            return result;
        }

        #endregion

        #region 字段

        private Pen? _pen = null;

        #endregion
    }
}