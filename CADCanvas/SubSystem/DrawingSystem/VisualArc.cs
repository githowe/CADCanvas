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
                    GetPointAtRadian(StartRadian),
                    GetPointAtRadian(EndRadian)
                };

                double[] quadrantAngles =
                [
                    0,
                    Math.PI / 2,
                    Math.PI,
                    Math.PI * 3 / 2
                ];

                foreach (double angle in quadrantAngles)
                {
                    if (IsAngleOnArc(angle))
                    {
                        points.Add(GetPointAtRadian(angle));
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
            DrawArc(dc, grid, _pen);
        }

        public override void DrawHover(DrawingContext dc, IWorldGrid grid)
        {
            DrawArc(dc, grid, _hoverPen);
        }

        public override void DrawSelect(DrawingContext dc, IWorldGrid grid)
        {
            DrawArc(dc, grid, _selectPen);
            DrawPoint(dc, grid.ToScreen(Center, true), _pointFill, _pointBorder);
            DrawPoint(dc, grid.ToScreen(GetPointAtRadian(StartRadian), true), _pointFill, _pointBorder);
            double midRadian = (StartRadian + EndRadian) / 2;
            DrawPoint(dc, grid.ToScreen(GetPointAtRadian(midRadian), true), _pointFill, _pointBorder);
            DrawPoint(dc, grid.ToScreen(GetPointAtRadian(EndRadian), true), _pointFill, _pointBorder);
        }

        public override List<SnapPoint> GetSnapPointList()
        {
            List<SnapPoint> result = new List<SnapPoint>();

            result.Add(new SnapPoint() { Type = SnapType.Center, WorldPoint = Center });
            result.Add(new SnapPoint() { Type = SnapType.Endpoint, WorldPoint = GetPointAtRadian(StartRadian) });
            double midRadian = (StartRadian + EndRadian) / 2;
            result.Add(new SnapPoint() { Type = SnapType.Midpoint, WorldPoint = GetPointAtRadian(midRadian) });
            result.Add(new SnapPoint() { Type = SnapType.Endpoint, WorldPoint = GetPointAtRadian(EndRadian) });

            return result;
        }

        public override List<GeoVisual> SplitByIntersectionPoint(List<Point> pointList)
        {
            const double tolerance = 1e-8;
            List<GeoVisual> result = new List<GeoVisual>();

            // 创建分割点列表，并添加起点
            List<SegmentPoint> splitPointList = new List<SegmentPoint>();
            splitPointList.Add(new SegmentPoint(StartRadian.ToAngle(), GetPointAtRadian(StartRadian)));
            // 遍历交点，添加至分割点列表
            foreach (var point in pointList)
            {
                // 计算交点的角度
                double radian = Math.Atan2(point.Y - Center.Y, point.X - Center.X);
                if (radian < StartRadian) radian += 2 * Math.PI;
                // 添加分割点
                splitPointList.Add(new SegmentPoint(radian.ToAngle(), point));
            }
            // 按角度排序分割点列表
            splitPointList.Sort((a, b) => a.Angle.CompareTo(b.Angle));
            // 去重分割点列表
            List<SegmentPoint> uniquePointList = new List<SegmentPoint>();
            foreach (SegmentPoint item in splitPointList)
            {
                if (uniquePointList.Count == 0)
                {
                    uniquePointList.Add(item);
                    continue;
                }

                SegmentPoint last = uniquePointList[uniquePointList.Count - 1];
                if (Math.Abs(item.Angle - last.Angle) <= tolerance) continue;

                uniquePointList.Add(item);
            }
            // 遍历分割点，创建新的弧段
            for (int index = 0; index < uniquePointList.Count - 1; index++)
            {
                SegmentPoint start = uniquePointList[index];
                SegmentPoint end = uniquePointList[index + 1];
                VisualArc newArc = new VisualArc
                {
                    Center = Center,
                    Radius = Radius,
                    StartRadian = start.Angle.ToRadian(),
                    EndRadian = end.Angle.ToRadian(),
                    Handle = ArcInterop.CreateArc(Center.X, Center.Y, Radius, start.Angle.ToRadian(), end.Angle.ToRadian()),
                };
                newArc.Init();
                result.Add(newArc);
            }

            return result;
        }

        public override List<GeoVisual> JointSplitVisual(List<GeoVisual> visualList)
        {
            List<GeoVisual> result = new List<GeoVisual>();

            if (visualList.Count == 0) return result;
            if (visualList.Count == 1)
            {
                VisualArc visual = visualList[0] as VisualArc;
                VisualArc arc = new VisualArc
                {
                    Center = visual.Center,
                    Radius = visual.Radius,
                    StartRadian = visual.StartRadian,
                    EndRadian = visual.EndRadian,
                };
                if (arc.EndRadian < arc.StartRadian) arc.EndRadian += 2 * Math.PI;
                arc.Handle = ArcInterop.CreateArc(arc.Center.X, arc.Center.Y, arc.Radius, arc.StartRadian, arc.EndRadian);
                arc.Init();
                result.Add(arc);
                return result;
            }

            // 拼接圆弧
            VisualArc first = visualList[0] as VisualArc;
            VisualArc current = new VisualArc
            {
                Center = first.Center,
                Radius = first.Radius,
                StartRadian = first.StartRadian,
                EndRadian = first.EndRadian,
            };

            for (int index = 1; index < visualList.Count; index++)
            {
                VisualArc visual = visualList[index] as VisualArc;
                // 如果首尾相连，更新当前圆弧的结束角度
                if (visual.StartRadian == current.EndRadian)
                    current.EndRadian = visual.EndRadian;
                // 否则，将当前圆弧添加至结果，并创建新的圆弧
                else
                {
                    result.Add(current);
                    current = new VisualArc
                    {
                        Center = visual.Center,
                        Radius = visual.Radius,
                        StartRadian = visual.StartRadian,
                        EndRadian = visual.EndRadian,
                    };
                }
            }
            if (result.Count > 0)
            {
                // 最后一个圆弧如果能与第一个圆弧首尾相连，则合并
                first = result[0] as VisualArc;
                double endRadian = current.EndRadian % (2 * Math.PI);
                double radianDiff = Math.Abs(endRadian - first.StartRadian);
                if (radianDiff < 1e-8) first.StartRadian = current.StartRadian;
                // 否则，添加最后一个圆弧
                else result.Add(current);
            }
            else result.Add(current);

            // 创建圆弧句柄并初始化
            foreach (VisualArc item in result)
            {
                if (item.EndRadian < item.StartRadian) item.EndRadian += 2 * Math.PI;
                item.Handle = ArcInterop.CreateArc(item.Center.X, item.Center.Y, item.Radius, item.StartRadian, item.EndRadian);
                item.Init();
            }

            return result;
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

        private Point GetPointAtRadian(double radian)
        {
            return new Point(Center.X + Radius * Math.Cos(radian), Center.Y + Radius * Math.Sin(radian));
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

        private readonly record struct SegmentPoint(double Angle, Point Point);
        private Pen? _pen = null;
        private Pen? _hoverPen = null;
        private Pen? _selectPen = null;
        private readonly Pen _pointBorder = new Pen(Brushes.White, 1);
        private readonly Brush _pointFill = new SolidColorBrush(Color.FromArgb(255, 0, 127, 255));

        #endregion
    }
}