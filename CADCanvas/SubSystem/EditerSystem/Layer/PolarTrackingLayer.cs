using CADCanvas.SubSystem.DebugSystem;
using System.Windows;
using System.Windows.Media;
using XLogic.Wpf;
using XLogic.Wpf.Drawing;

namespace CADCanvas.SubSystem.EditerSystem.Layer
{
    /// <summary>
    /// 极轴追踪图层
    /// </summary>
    public class PolarTrackingLayer : DrawingLayer
    {
        #region 属性

        public GridLayer? Grid { get; set; } = null;

        public bool Snapped => _snapped;

        #endregion

        #region 公开方法

        public override void Init()
        {
            _pen.DashStyle = new DashStyle([3, 3], 0);
            _pen.StartLineCap = PenLineCap.Square;
            _pen.EndLineCap = PenLineCap.Square;
            _pen.Freeze();
        }

        public void Reset()
        {
            _worldStart = new Point();
            _angle = 0;
            _snapped = false;
        }

        /// <summary>
        /// 更新追踪角度
        /// </summary>
        public double UpdateTrackingAngle(Point worldStart, Point worldEnd)
        {
            // 记录起点
            _worldStart = worldStart;
            // 计算角度
            double angle = PointTool.GetAngle(worldStart, worldEnd);
            // 吸附并记录角度
            _angle = SnapAngle(angle);
            DebugInfoManager.Instance.UpdateInfo("极轴追踪", $"{angle:0.########} > {_angle:0.########}");
            // 返回吸附后的角度
            return _angle;
        }

        #endregion

        #region 内部方法

        protected override void OnUpdate()
        {
            if (_snapped)
            {
                Point screenStart = Grid.ToScreen(_worldStart);
                Point point = GetPointIntersectionScreen(screenStart, _angle);
                _dc.DrawLine(_pen, screenStart, point);
            }
        }

        #endregion

        #region 私有方法

        private double SnapAngle(double angle)
        {
            double nearest = Math.Round(angle / _trackingAngle) * _trackingAngle;
            if (Math.Abs(nearest - angle) <= _snapThreshold)
            {
                _snapped = true;
                return nearest % 360;
            }
            _snapped = false;
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

        #endregion

        #region 字段

        private readonly Pen _pen = new Pen(new SolidColorBrush(Color.FromRgb(0, 160, 0)), 1);

        private Point _worldStart = new Point();
        private double _angle = 0;

        /// <summary>追踪角度</summary>
        private double _trackingAngle = 45;
        /// <summary>吸附阈值</summary>
        private readonly double _snapThreshold = 2;

        private bool _snapped = false;

        #endregion
    }
}