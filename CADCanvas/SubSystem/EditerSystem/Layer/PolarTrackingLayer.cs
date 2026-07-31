using CADCanvas.SubSystem.DebugSystem;
using System.Windows;
using System.Windows.Media;
using XLogic.Wpf.Drawing;

namespace CADCanvas.SubSystem.EditerSystem.Layer
{
    /// <summary>
    /// 极轴追踪图层
    /// </summary>
    public class PolarTrackingLayer : DrawingLayer
    {
        public double TrackingAngle
        {
            get => _trackingAngle;
            set => _trackingAngle = value;
        }

        public bool Snaped => _snaped;

        public override void Init()
        {
            _pen.DashStyle = new DashStyle([4, 2], 0);
            _pen.DashCap = PenLineCap.Flat;
            _pen.Freeze();
        }

        /// <summary>
        /// 更新追踪角度
        /// </summary>
        public double UpdateTrackingAngle(Point screenStart, Point screenEnd)
        {
            _screenStart = screenStart;

            // 将屏幕坐标转换为数学坐标
            Point mathStart = new Point(screenStart.X, -screenStart.Y);
            Point mathEnd = new Point(screenEnd.X, -screenEnd.Y);
            // 计算偏移
            Point offset = new Point(mathEnd.X - mathStart.X, mathEnd.Y - mathStart.Y);
            // 计算角度
            double radians;
            double angle;
            if (mathEnd.Y >= mathStart.Y)
            {
                radians = Math.Atan2(offset.Y, offset.X);
                angle = radians * 180 / Math.PI;
            }
            else
            {
                radians = Math.Atan2(offset.Y, offset.X) + 2 * Math.PI;
                angle = radians * 180 / Math.PI;
            }

            _angle = SnapAngle(angle);
            DebugInfoManager.Instance.UpdateInfo("极轴追踪", $"{angle:F2} > {_angle:F2}");
            return _angle;
        }

        protected override void OnUpdate()
        {
            if (_snaped)
            {
                Point point = GetPointIntersectionScreen(_screenStart, _angle);
                _dc.DrawLine(_pen, _screenStart, point);
            }
        }

        private double SnapAngle(double angle)
        {
            double nearest = Math.Round(angle / _trackingAngle) * _trackingAngle;
            if (Math.Abs(nearest - angle) <= _snapThreshold)
            {
                _snaped = true;
                return nearest % 360;
            }
            _snaped = false;
            return angle;
        }
        
        /// <summary>
        /// 获取射线与屏幕的交点
        /// </summary>
        private Point GetPointIntersectionScreen(Point start, double angle)
        {
            double rad = angle * Math.PI / 180;
            double dx = Math.Cos(rad);
            double dy = -Math.Sin(rad);

            double left = 0;
            double top = 0;
            double right = Width;
            double bottom = Height;

            List<(double t, Point point)> candidates = new List<(double t, Point point)>();

            if (Math.Abs(dx) > 1e-6)
            {
                double t = (left - start.X) / dx;
                if (t >= 0)
                {
                    double y = start.Y + t * dy;
                    if (y >= top && y <= bottom)
                        candidates.Add((t, new Point(left, y)));
                }
                double tR = (right - start.X) / dx;
                if (tR >= 0)
                {
                    double y = start.Y + tR * dy;
                    if (y >= top && y <= bottom)
                        candidates.Add((tR, new Point(right, y)));
                }
            }
            if (Math.Abs(dy) > 1e-6)
            {
                double t = (top - start.Y) / dy;
                if (t >= 0)
                {
                    double x = start.X + t * dx;
                    if (x >= left && x <= right)
                        candidates.Add((t, new Point(x, top)));
                }
                double tB = (bottom - start.Y) / dy;
                if (tB >= 0)
                {
                    double x = start.X + tB * dx;
                    if (x >= left && x <= right)
                        candidates.Add((tB, new Point(x, bottom)));
                }
            }

            if (candidates.Count == 0) throw new Exception("与屏幕边缘无交点");

            candidates.Sort((a, b) => b.t.CompareTo(a.t));
            return candidates[0].point;
        }

        private readonly Pen _pen = new Pen(new SolidColorBrush(Color.FromRgb(254, 210, 103)), 1);

        private Point _screenStart = new Point();
        private double _angle = 0;

        /// <summary>追踪角度</summary>
        private double _trackingAngle = 45;
        /// <summary>吸附阈值</summary>
        private readonly double _snapThreshold = 2;

        private bool _snaped = false;
    }
}