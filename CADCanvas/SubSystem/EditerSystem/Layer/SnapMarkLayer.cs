using CADCanvas.SubSystem.EditerSystem.Component.Tool.Snap;
using System.Windows;
using System.Windows.Media;
using XLogic.Wpf;
using XLogic.Wpf.Drawing;

namespace CADCanvas.SubSystem.EditerSystem.Layer
{
    /// <summary>
    /// 捕捉标记
    /// </summary>
    public class SnapMarkLayer : DrawingLayer
    {
        public GridLayer? Grid { get; set; } = null;

        public List<SnapPoint> SnapPointList { get; set; } = new List<SnapPoint>();

        public override void Init()
        {
            _pen.StartLineCap = PenLineCap.Square;
            _pen.EndLineCap = PenLineCap.Square;
            _pen.Freeze();
            _pen2.StartLineCap = PenLineCap.Square;
            _pen2.EndLineCap = PenLineCap.Square;
            _pen2.Freeze();
            _pen3.StartLineCap = PenLineCap.Square;
            _pen3.EndLineCap = PenLineCap.Square;
            _pen3.Freeze();
        }

        protected override void OnUpdate()
        {
            foreach (var snapPoint in SnapPointList)
            {
                switch (snapPoint.Type)
                {
                    case SnapType.Endpoint:
                        DrawEndpoint(snapPoint.WorldPoint);
                        break;
                    case SnapType.Midpoint:
                        DrawMidpoint(snapPoint.WorldPoint);
                        break;
                    case SnapType.Center:
                        DrawCenter(snapPoint.WorldPoint);
                        break;
                    case SnapType.Intersection:
                        DrawIntersection(snapPoint.WorldPoint);
                        break;
                    case SnapType.Tangent:
                        break;
                    case SnapType.Perpendicular:
                        break;
                    case SnapType.Parallel:
                        break;
                    case SnapType.Point:
                        break;
                }
            }
        }

        /// <summary>
        /// 绘制端点
        /// </summary>
        private void DrawEndpoint(Point point)
        {
            double radius = _pointSize / 2 - 0.5;
            Point screenPoint = Grid.ToScreen(point, true);
            Point leftTop = new Point(screenPoint.X - radius, screenPoint.Y - radius);
            Point rightBottom = new Point(screenPoint.X + radius, screenPoint.Y + radius);
            _dc.DrawRectangle(null, _pen, new Rect(leftTop, rightBottom));
        }

        /// <summary>
        /// 绘制中点
        /// </summary>
        private void DrawMidpoint(Point point)
        {
            // 获取屏幕坐标
            Point screenPoint = Grid.ToScreen(point, true);
            // 计算半径
            double radius = _pointSize / 2 + 1;
            // 计算等边三角形的三个顶点坐标
            Point firstPoint = new Point(screenPoint.X, screenPoint.Y - radius);
            Point unitEndPoint = MathTool.GetEndPointFromAngle(210);
            unitEndPoint.Y = -unitEndPoint.Y;
            Point secondPoint = new Point(screenPoint.X + unitEndPoint.X * radius, screenPoint.Y + unitEndPoint.Y * radius);
            unitEndPoint = MathTool.GetEndPointFromAngle(330);
            unitEndPoint.Y = -unitEndPoint.Y;
            Point thirdPoint = new Point(screenPoint.X + unitEndPoint.X * radius, screenPoint.Y + unitEndPoint.Y * radius);
            // 绘制等边三角形
            PathGeometry geometry = new PathGeometry();
            PathFigure figure = new PathFigure();
            geometry.Figures.Add(figure);
            figure.StartPoint = firstPoint;
            figure.Segments.Add(new LineSegment(secondPoint, true));
            figure.Segments.Add(new LineSegment(thirdPoint, true));
            figure.IsClosed = true;
            geometry.Freeze();
            _dc.DrawGeometry(null, _pen, geometry);
        }

        /// <summary>
        /// 绘制圆心
        /// </summary>
        private void DrawCenter(Point point)
        {
            double radius = _pointSize / 2 + 1;
            Point screenPoint = Grid.ToScreen(point, true);
            // 绘制圆形
            _dc.DrawEllipse(null, _pen2, screenPoint, radius, radius);
            // 绘制十字线
            double lineRadius = radius - 3;
            double left = screenPoint.X - lineRadius;
            double right = screenPoint.X + lineRadius;
            double top = screenPoint.Y - lineRadius;
            double bottom = screenPoint.Y + lineRadius;
            double centerX = screenPoint.X;
            double centerY = screenPoint.Y;
            _dc.DrawLine(_pen3, new Point(left, centerY), new Point(right, centerY));
            _dc.DrawLine(_pen3, new Point(centerX, top), new Point(centerX, bottom));
        }

        /// <summary>
        /// 绘制交点
        /// </summary>
        private void DrawIntersection(Point point)
        {
            double radius = _pointSize / 2 - 1;
            Point screenPoint = Grid.ToScreen(point, true);
            Point leftTop = new Point(screenPoint.X - radius, screenPoint.Y - radius);
            Point rightBottom = new Point(screenPoint.X + radius, screenPoint.Y + radius);
            Point leftBottom = new Point(screenPoint.X - radius, screenPoint.Y + radius);
            Point rightTop = new Point(screenPoint.X + radius, screenPoint.Y - radius);
            _dc.DrawLine(_pen, leftTop, rightBottom);
            _dc.DrawLine(_pen, leftBottom, rightTop);
        }

        private readonly Pen _pen = new Pen(new SolidColorBrush(Color.FromArgb(255, 0, 160, 0)), 2);
        private readonly Pen _pen2 = new Pen(new SolidColorBrush(Color.FromArgb(255, 0, 160, 0)), 1);
        private readonly Pen _pen3 = new Pen(new SolidColorBrush(Color.FromArgb(255, 255, 255, 255)), 1);
        private readonly double _pointSize = 12;
    }
}