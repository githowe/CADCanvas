using CADCanvas.SubSystem.EditerSystem.Component.Tool.Snap;
using CADCanvas.SubSystem.ResourceSystem;
using System.Windows;
using System.Windows.Media;
using XLogic.Wpf;

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
            List<GeoVisual> result = new List<GeoVisual>();

            // 小于两个交点时，创建完整的圆
            if (pointList.Count < 2)
            {
                VisualCircle circle = new VisualCircle
                {
                    Handle = CircleInterop.CreateCircle(Center.X, Center.Y, Radius),
                    Center = Center,
                    Radius = Radius,
                };
                circle.Init();
                result.Add(circle);

                return result;
            }

            // 容差
            const double tolerance = 1e-8;
            // 创建分割点列表
            List<SegmentPoint> splitPointList = new List<SegmentPoint>();
            // 遍历交点列表，计算每个交点的角度，并添加到分割点列表中
            foreach (var point in pointList)
            {
                double radian = Math.Atan2(point.Y - Center.Y, point.X - Center.X);
                double angle = radian.ToAngle();
                if (angle < 0) angle += 360;
                splitPointList.Add(new SegmentPoint(angle, point));
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
            // 添加一个起点，形成循环
            SegmentPoint first = new SegmentPoint(uniquePointList[0].Angle + 360, uniquePointList[0].Point);
            uniquePointList.Add(first);
            // 遍历分割点，创建圆弧段
            for (int index = 0; index < uniquePointList.Count - 1; index++)
            {
                double startAngle = uniquePointList[index].Angle;
                double endAngle = uniquePointList[index + 1].Angle;
                if (Math.Abs(endAngle - startAngle) <= tolerance) continue;

                VisualArc arc = new VisualArc
                {
                    Center = Center,
                    Radius = Radius,
                    StartRadian = startAngle.ToRadian(),
                    EndRadian = endAngle.ToRadian(),
                    Handle = ArcInterop.CreateArc(Center.X, Center.Y, Radius, startAngle.ToRadian(), endAngle.ToRadian()),
                    // LineColor = BrushManager.Instance.GetColor(),
                };
                arc.Init();
                result.Add(arc);
            }
            BrushManager.Instance.ResetIndex();

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
            // 最后一个圆弧如果能与第一个圆弧首尾相连，则合并
            first = result[0] as VisualArc;
            double endRadian = current.EndRadian % (2 * Math.PI);
            double radianDiff = Math.Abs(endRadian - first.StartRadian);
            if (radianDiff < 1e-8) first.StartRadian = current.StartRadian;
            // 否则，添加最后一个圆弧
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

        #region 字段

        private Pen? _pen = null;
        private Pen? _hoverPen = null;
        private Pen? _selectPen = null;
        private readonly Pen _pointBorder = new Pen(Brushes.White, 1);
        private readonly Brush _pointFill = new SolidColorBrush(Color.FromArgb(255, 0, 127, 255));

        private readonly record struct SegmentPoint(double Angle, Point Point);

        #endregion
    }
}