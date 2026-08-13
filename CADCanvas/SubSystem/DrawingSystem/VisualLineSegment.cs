using CADCanvas.SubSystem.EditerSystem.Component.Tool.Snap;
using CADCanvas.SubSystem.ResourceSystem;
using System.Windows;
using System.Windows.Media;

namespace CADCanvas.SubSystem.DrawingSystem
{
    public class VisualLineSegment : GeoVisual
    {
        public Point Start { get; set; } = new Point(0, 0);

        public Point End { get; set; } = new Point(0, 0);

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

        public override List<GeoVisual> SplitByIntersectionPoint(List<Point> pointList)
        {
            // 容差
            const double tolerance = 1e-8;
            // 结果
            List<GeoVisual> result = new List<GeoVisual>();
            // 方向向量
            Vector direction = End - Start;
            // 长度平方
            double lengthSquared = direction.X * direction.X + direction.Y * direction.Y;

            if (lengthSquared <= tolerance) return result;

            // 创建分割点列表，并添加起点
            List<SegmentPoint> splitPointList = new List<SegmentPoint>();
            splitPointList.Add(new SegmentPoint(0.0, Start));
            // 遍历交点列表，添加到分割点列表中
            foreach (Point point in pointList)
            {
                // 创建起点至交点的向量
                Vector toPoint = point - Start;
                // 计算交点在起点至终点的参数 t
                double t = Vector.Multiply(toPoint, direction) / lengthSquared;
                splitPointList.Add(new SegmentPoint(t, point));
            }
            // 添加终点，然后按 t 排序分割点列表
            splitPointList.Add(new SegmentPoint(1.0, End));
            splitPointList.Sort((a, b) => a.T.CompareTo(b.T));
            
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
                if (Math.Abs(item.T - last.T) <= tolerance || (item.Point - last.Point).Length <= tolerance)
                {
                    continue;
                }

                uniquePointList.Add(item);
            }
            // 遍历去重后的分割点列表，创建线段
            for (int index = 0; index < uniquePointList.Count - 1; index++)
            {
                Point start = uniquePointList[index].Point;
                Point end = uniquePointList[index + 1].Point;

                if ((end - start).Length <= tolerance) continue;

                VisualLineSegment segment = new VisualLineSegment
                {
                    Handle = LineInterop.CreateLineSegment(start.X, start.Y, end.X, end.Y),
                    Start = start,
                    End = end,
                    LineColor = BrushManager.Instance.GetColor(),
                    LineWidth = LineWidth
                };
                segment.Init();

                result.Add(segment);
            }
            BrushManager.Instance.ResetIndex();

            // 返回结果
            return result;
        }

        private readonly record struct SegmentPoint(double T, Point Point);
        private Pen? _pen = null;
    }
}