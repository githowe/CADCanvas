using CADCanvas.SubSystem.DebugSystem;
using System.Windows;
using System.Windows.Media;
using XLogic.Wpf.Drawing;

namespace CADCanvas.SubSystem.EditerSystem.Layer
{
    /// <summary>
    /// 直线工具图层
    /// </summary>
    public class LineToolLayer : DrawingLayer
    {
        #region 属性

        public GridLayer? Grid { get; set; } = null;

        /// <summary>起点世界坐标</summary>
        public Point? WorldStart
        {
            get => _worldStart;
            set
            {
                _worldStart = value;
                if (_worldStart.HasValue)
                    ScreenStart = Grid.ToScreen(_worldStart.Value);
            }
        }

        /// <summary>终点世界坐标</summary>
        public Point? WorldEnd
        {
            get => _worldEnd;
            set
            {
                _worldEnd = value;
                if (_worldEnd.HasValue)
                    ScreenEnd = Grid.ToScreen(_worldEnd.Value);
            }
        }

        /// <summary>起点屏幕坐标</summary>
        public Point ScreenStart { get; private set; } = new Point();

        /// <summary>终点屏幕坐标</summary>
        public Point ScreenEnd { get; private set; } = new Point();

        /// <summary>直线长度</summary>
        public double LineLength { get; private set; } = 0;

        /// <summary>固定长度</summary>
        public double FixedLength { get; set; } = double.NaN;

        /// <summary>直线角度</summary>
        public double LineAngle { get; private set; } = 0;

        /// <summary>固定角度</summary>
        public double FixedAngle { get; set; } = double.NaN;

        /// <summary>直线中点</summary>
        public Point LineCenter
        {
            get
            {
                double x = _lineStart.X + (_lineEnd.X - _lineStart.X) / 2;
                double y = _lineStart.Y + (_lineEnd.Y - _lineStart.Y) / 2;
                return new Point(x, y);
            }
        }

        /// <summary>圆弧中点</summary>
        public Point ArcCenter { get; private set; } = new Point();

        #endregion

        #region 公开方法

        public override void Init()
        {
            _linePen.Freeze();
            _linearPen.Brush = new SolidColorBrush(Color.FromArgb(128, 255, 255, 255));
            _linearPen.DashStyle = new DashStyle([2, 2], 0);
            _linearPen.DashCap = PenLineCap.Flat;
            _linearPen.Freeze();
            IsHitTestVisible = false;

            _positionName.Add(EndPosition.LeftTop, "左上");
            _positionName.Add(EndPosition.RightTop, "右上");
            _positionName.Add(EndPosition.LeftBottom, "左下");
            _positionName.Add(EndPosition.RightBottom, "右下");
            _positionName.Add(EndPosition.LeftAxis, "左半轴");
            _positionName.Add(EndPosition.RightAxis, "右半轴");
            _positionName.Add(EndPosition.TopAxis, "上半轴");
            _positionName.Add(EndPosition.BottomAxis, "下半轴");
            _positionName.Add(EndPosition.Origin, "原点");
        }

        /// <summary>
        /// 更新直线长度
        /// </summary>
        public void UpdateLineLength()
        {
            if (WorldStart == null || WorldEnd == null)
                return;

            // 计算直线长度
            double dx = WorldEnd.Value.X - WorldStart.Value.X;
            double dy = WorldEnd.Value.Y - WorldStart.Value.Y;
            LineLength = Math.Sqrt(dx * dx + dy * dy);
        }

        public void SnapTo(double angle)
        {
            // 获取起点、长度
            Point start = WorldStart.Value;
            double length = LineLength;
            // 计算终点
            double radians = angle * Math.PI / 180;
            double endX = start.X + length * Math.Cos(radians);
            double endY = start.Y - length * Math.Sin(radians);
            WorldEnd = new Point(endX, endY);
        }

        #endregion

        #region 内部方法

        protected override void OnUpdate()
        {
            if (WorldStart == null || WorldEnd == null)
                return;

            // 绘制直线
            _dc.DrawLine(_linePen, ScreenStart, ScreenEnd);

            List<LineInfo> LineList = new List<LineInfo>();
            ArcInfo arcInfo = new ArcInfo();

            _manager.UpdateInfo("起点坐标", $"({ScreenStart.X:0.####}, {ScreenStart.Y:0.####})");
            _manager.UpdateInfo("终点坐标", $"({ScreenEnd.X:0.####}, {ScreenEnd.Y:0.####})");

            EndPosition position = GetEndPosition();
            _manager.UpdateInfo("终点位置", _positionName[position]);

            // 填充标注线
            FillLineInfo(position, LineList, arcInfo);
            // 绘制标注线
            foreach (var line in LineList)
                _dc.DrawLine(_linearPen, line.Start, line.End);
            // 创建路径
            PathGeometry pathGeometry = new PathGeometry();
            PathFigure pathFigure = new PathFigure { StartPoint = arcInfo.Start };
            // 创建圆弧
            ArcSegment arcSegment = new ArcSegment
            {
                Point = arcInfo.End,
                Size = new Size(arcInfo.Radius, arcInfo.Radius),
                SweepDirection = SweepDirection.Counterclockwise,
                IsLargeArc = false,
            };
            // 添加圆弧到路径
            pathFigure.Segments.Add(arcSegment);
            pathGeometry.Figures.Add(pathFigure);
            // 绘制路径
            _dc.DrawGeometry(null, _linearPen, pathGeometry);
        }

        #endregion

        #region 私有方法

        private void FillLineInfo(EndPosition position, List<LineInfo> LineList, ArcInfo arcInfo)
        {
            Point mathStart = new Point(ScreenStart.X, -ScreenStart.Y);
            Point mathEnd = new Point(ScreenEnd.X, -ScreenEnd.Y);
            double 垂直偏移 = mathEnd.Y - mathStart.Y;
            double 水平偏移 = mathEnd.X - mathStart.X;
            _manager.UpdateInfo("垂直偏移", $"{垂直偏移:0.##}");
            _manager.UpdateInfo("水平偏移", $"{水平偏移:0.##}");
            double 斜边长度 = Math.Sqrt(水平偏移 * 水平偏移 + 垂直偏移 * 垂直偏移);
            _manager.UpdateInfo("斜边长度", $"{斜边长度:0.##}");

            switch (position)
            {
                case EndPosition.RightAxis:
                case EndPosition.RightTop:
                case EndPosition.TopAxis:
                case EndPosition.LeftTop:
                case EndPosition.LeftAxis:
                    {
                        double 斜边弧度 = Math.Atan2(垂直偏移, 水平偏移);
                        double 中线弧度 = 斜边弧度 / 2;
                        double arcCenterX = ScreenStart.X + Math.Cos(中线弧度) * 斜边长度;
                        double arcCenterY = ScreenStart.Y - Math.Sin(中线弧度) * 斜边长度;
                        ArcCenter = new Point(arcCenterX, arcCenterY);
                        double 角度 = 斜边弧度 * 180 / Math.PI;
                        LineAngle = 角度;
                        _manager.UpdateInfo("斜边弧度", $"{斜边弧度:0.####}°");
                        _manager.UpdateInfo("斜边角度", $"{角度:0.##}°");
                        // 逆时针旋转90度
                        double 旋转后弧度 = 斜边弧度 + Math.PI / 2;
                        double 旋转后角度 = 旋转后弧度 * 180 / Math.PI;
                        _manager.UpdateInfo("旋转后角度", $"{旋转后角度:0.##}°");
                        double 旋转后横坐标偏移 = Math.Cos(旋转后弧度) * 60;
                        double 旋转后纵坐标偏移 = Math.Sin(旋转后弧度) * 60;
                        _manager.UpdateInfo("旋转后横坐标偏移", $"{旋转后横坐标偏移:0.##}°");
                        _manager.UpdateInfo("旋转后纵坐标偏移", $"{旋转后纵坐标偏移:0.##}°");
                        // 添加标注线
                        LineInfo line = new LineInfo
                        {
                            Start = ScreenStart,
                            End = new Point(ScreenStart.X + 旋转后横坐标偏移, ScreenStart.Y - 旋转后纵坐标偏移)
                        };
                        LineList.Add(line);
                        line = new LineInfo
                        {
                            Start = line.End,
                            End = new Point(ScreenEnd.X + 旋转后横坐标偏移, ScreenEnd.Y - 旋转后纵坐标偏移)
                        };
                        LineList.Add(line);
                        _lineStart = line.Start;
                        _lineEnd = line.End;
                        line = new LineInfo
                        {
                            Start = line.End,
                            End = ScreenEnd
                        };
                        LineList.Add(line);
                        // 添加半径线
                        line = new LineInfo
                        {
                            Start = new Point(ScreenStart.X, ScreenStart.Y),
                            End = new Point(ScreenStart.X + 斜边长度, ScreenStart.Y)
                        };
                        LineList.Add(line);
                        // 设置圆弧信息
                        arcInfo.Start = new Point(ScreenStart.X + 斜边长度, ScreenStart.Y);
                        arcInfo.End = ScreenEnd;
                        arcInfo.Radius = 斜边长度;
                    }
                    break;
                case EndPosition.LeftBottom:
                case EndPosition.BottomAxis:
                case EndPosition.RightBottom:
                    {
                        double 斜边弧度 = Math.Atan2(垂直偏移, 水平偏移) + Math.PI * 2;
                        double 中线弧度 = 斜边弧度 / 2;
                        double arcCenterX = ScreenStart.X - Math.Cos(中线弧度) * 斜边长度;
                        double arcCenterY = ScreenStart.Y + Math.Sin(中线弧度) * 斜边长度;
                        ArcCenter = new Point(arcCenterX, arcCenterY);
                        double 角度 = 斜边弧度 * 180 / Math.PI;
                        LineAngle = 360 - 角度;
                        _manager.UpdateInfo("斜边弧度", $"{斜边弧度:0.######}°");
                        _manager.UpdateInfo("斜边角度", $"{角度:0.##}°");
                        // 顺时针旋转90度
                        double 旋转后弧度 = 斜边弧度 - Math.PI / 2;
                        double 旋转后角度 = 旋转后弧度 * 180 / Math.PI;
                        _manager.UpdateInfo("旋转后角度", $"{旋转后角度:0.##}°");
                        double 旋转后横坐标偏移 = Math.Cos(旋转后弧度) * 60;
                        double 旋转后纵坐标偏移 = Math.Sin(旋转后弧度) * 60;
                        _manager.UpdateInfo("旋转后横坐标偏移", $"{旋转后横坐标偏移:0.##}°");
                        _manager.UpdateInfo("旋转后纵坐标偏移", $"{旋转后纵坐标偏移:0.##}°");
                        LineInfo line = new LineInfo
                        {
                            Start = ScreenStart,
                            End = new Point(ScreenStart.X + 旋转后横坐标偏移, ScreenStart.Y - 旋转后纵坐标偏移)
                        };
                        LineList.Add(line);
                        line = new LineInfo
                        {
                            Start = line.End,
                            End = new Point(ScreenEnd.X + 旋转后横坐标偏移, ScreenEnd.Y - 旋转后纵坐标偏移)
                        };
                        LineList.Add(line);
                        _lineStart = line.Start;
                        _lineEnd = line.End;
                        line = new LineInfo
                        {
                            Start = line.End,
                            End = ScreenEnd
                        };
                        LineList.Add(line);
                        // 添加半径线
                        line = new LineInfo
                        {
                            Start = new Point(ScreenStart.X, ScreenStart.Y),
                            End = new Point(ScreenStart.X + 斜边长度, ScreenStart.Y)
                        };
                        LineList.Add(line);
                        // 设置圆弧信息
                        arcInfo.Start = ScreenEnd;
                        arcInfo.End = new Point(ScreenStart.X + 斜边长度, ScreenStart.Y);
                        arcInfo.Radius = 斜边长度;
                    }
                    break;
                case EndPosition.Origin:
                    break;
            }
        }

        private EndPosition GetEndPosition()
        {
            // 横坐标终点等于起点
            if (ScreenEnd.X == ScreenStart.X)
            {
                if (ScreenEnd.Y == ScreenStart.Y) return EndPosition.Origin;
                else if (ScreenEnd.Y < ScreenStart.Y) return EndPosition.TopAxis;
                else return EndPosition.BottomAxis;
            }
            // 横坐标终点小于起点
            else if (ScreenEnd.X < ScreenStart.X)
            {
                if (ScreenEnd.Y == ScreenStart.Y) return EndPosition.LeftAxis;
                else if (ScreenEnd.Y < ScreenStart.Y) return EndPosition.LeftTop;
                else return EndPosition.LeftBottom;
            }
            // 横坐标终点大于起点
            else
            {
                if (ScreenEnd.Y == ScreenStart.Y) return EndPosition.RightAxis;
                else if (ScreenEnd.Y < ScreenStart.Y) return EndPosition.RightTop;
                else return EndPosition.RightBottom;
            }
        }

        #endregion

        #region 私有类型

        /// <summary>
        /// 终点位置
        /// </summary>
        private enum EndPosition
        {
            LeftTop,
            RightTop,
            LeftBottom,
            RightBottom,

            LeftAxis,
            RightAxis,
            TopAxis,
            BottomAxis,

            Origin,
        }

        /// <summary>
        /// 直线信息
        /// </summary>
        private class LineInfo
        {
            public Point Start { get; set; } = new Point();

            public Point End { get; set; } = new Point();
        }

        /// <summary>
        /// 圆弧信息
        /// </summary>
        private class ArcInfo
        {
            public Point Start { get; set; } = new Point();

            public Point End { get; set; } = new Point();

            public double Radius { get; set; } = 0;
        }

        #endregion

        #region 字段

        private readonly Pen _linePen = new Pen(Brushes.White, 1);
        private readonly Pen _linearPen = new Pen(Brushes.White, 1);
        private readonly DebugInfoManager _manager = DebugInfoManager.Instance;
        private readonly Dictionary<EndPosition, string> _positionName = new Dictionary<EndPosition, string>();

        private Point _lineStart;
        private Point _lineEnd;

        #endregion

        #region 属性字段

        private Point? _worldStart = null;
        private Point? _worldEnd = null;

        #endregion
    }
}