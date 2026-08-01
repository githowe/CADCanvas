using CADCanvas.SubSystem.DebugSystem;
using System.Windows;
using System.Windows.Media;
using XLogic.Wpf;
using XLogic.Wpf.Drawing;
using XLogic.Wpf.Ex;

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
        public Point? WorldStart { get => _worldStart; set => _worldStart = value; }

        /// <summary>终点世界坐标</summary>
        public Point? WorldEnd { get => _worldEnd; set => _worldEnd = value; }

        public double 物理长度 { get; set; } = 0;

        public double 周角 { get; set; } = 0;

        public double 当前物理长度 { get; set; } = 0;

        public double 当前周角 { get; set; } = 0;

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
        /// 更新直线信息：根据起点与终点，计算长度与角度
        /// </summary>
        public void UpdateLineInfo()
        {
            if (_worldStart == null || _worldEnd == null)
                return;

            物理长度 = PointTool.GetLength(_worldStart.Value, _worldEnd.Value);
            当前物理长度 = 物理长度;
            周角 = PointTool.GetAngle(_worldStart.Value.ToMathPoint(), _worldEnd.Value.ToMathPoint());
            当前周角 = 周角;
        }

        /// <summary>
        /// 根据当前物理长度移动世界终点
        /// </summary>
        public void MoveWorldEnd()
        {
            Point start = _worldStart.Value;
            double length = 当前物理长度;

            double radians = 当前周角 * Math.PI / 180;
            double endX = start.X + length * Math.Cos(radians);
            double endY = start.Y - length * Math.Sin(radians);

            _worldEnd = new Point(endX, endY);
        }

        #endregion

        #region 内部方法

        protected override void OnUpdate()
        {
            if (_worldStart == null || _worldEnd == null)
                return;

            _screenStart = Grid.ToScreen(_worldStart.Value);
            _screenEnd = Grid.ToScreen(_worldEnd.Value);

            // 绘制直线
            _dc.DrawLine(_linePen, _screenStart, _screenEnd);

            List<LineInfo> LineList = new List<LineInfo>();
            ArcInfo arcInfo = new ArcInfo();

            _manager.UpdateInfo("起点坐标", $"({_screenStart.X:0.####}, {_screenStart.Y:0.####})");
            _manager.UpdateInfo("终点坐标", $"({_screenEnd.X:0.####}, {_screenEnd.Y:0.####})");

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
            Point mathStart = new Point(_screenStart.X, -_screenStart.Y);
            Point mathEnd = new Point(_screenEnd.X, -_screenEnd.Y);
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
                        double arcCenterX = _screenStart.X + Math.Cos(中线弧度) * 斜边长度;
                        double arcCenterY = _screenStart.Y - Math.Sin(中线弧度) * 斜边长度;
                        ArcCenter = new Point(arcCenterX, arcCenterY);
                        double 角度 = 斜边弧度 * 180 / Math.PI;
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
                            Start = _screenStart,
                            End = new Point(_screenStart.X + 旋转后横坐标偏移, _screenStart.Y - 旋转后纵坐标偏移)
                        };
                        LineList.Add(line);
                        line = new LineInfo
                        {
                            Start = line.End,
                            End = new Point(_screenEnd.X + 旋转后横坐标偏移, _screenEnd.Y - 旋转后纵坐标偏移)
                        };
                        LineList.Add(line);
                        _lineStart = line.Start;
                        _lineEnd = line.End;
                        line = new LineInfo
                        {
                            Start = line.End,
                            End = _screenEnd
                        };
                        LineList.Add(line);
                        // 添加半径线
                        line = new LineInfo
                        {
                            Start = new Point(_screenStart.X, _screenStart.Y),
                            End = new Point(_screenStart.X + 斜边长度, _screenStart.Y)
                        };
                        LineList.Add(line);
                        // 设置圆弧信息
                        arcInfo.Start = new Point(_screenStart.X + 斜边长度, _screenStart.Y);
                        arcInfo.End = _screenEnd;
                        arcInfo.Radius = 斜边长度;
                    }
                    break;
                case EndPosition.LeftBottom:
                case EndPosition.BottomAxis:
                case EndPosition.RightBottom:
                    {
                        double 斜边弧度 = Math.Atan2(垂直偏移, 水平偏移) + Math.PI * 2;
                        double 中线弧度 = 斜边弧度 / 2;
                        double arcCenterX = _screenStart.X - Math.Cos(中线弧度) * 斜边长度;
                        double arcCenterY = _screenStart.Y + Math.Sin(中线弧度) * 斜边长度;
                        ArcCenter = new Point(arcCenterX, arcCenterY);
                        double 角度 = 斜边弧度 * 180 / Math.PI;
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
                            Start = _screenStart,
                            End = new Point(_screenStart.X + 旋转后横坐标偏移, _screenStart.Y - 旋转后纵坐标偏移)
                        };
                        LineList.Add(line);
                        line = new LineInfo
                        {
                            Start = line.End,
                            End = new Point(_screenEnd.X + 旋转后横坐标偏移, _screenEnd.Y - 旋转后纵坐标偏移)
                        };
                        LineList.Add(line);
                        _lineStart = line.Start;
                        _lineEnd = line.End;
                        line = new LineInfo
                        {
                            Start = line.End,
                            End = _screenEnd
                        };
                        LineList.Add(line);
                        // 添加半径线
                        line = new LineInfo
                        {
                            Start = new Point(_screenStart.X, _screenStart.Y),
                            End = new Point(_screenStart.X + 斜边长度, _screenStart.Y)
                        };
                        LineList.Add(line);
                        // 设置圆弧信息
                        arcInfo.Start = _screenEnd;
                        arcInfo.End = new Point(_screenStart.X + 斜边长度, _screenStart.Y);
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
            if (_screenEnd.X == _screenStart.X)
            {
                if (_screenEnd.Y == _screenStart.Y) return EndPosition.Origin;
                else if (_screenEnd.Y < _screenStart.Y) return EndPosition.TopAxis;
                else return EndPosition.BottomAxis;
            }
            // 横坐标终点小于起点
            else if (_screenEnd.X < _screenStart.X)
            {
                if (_screenEnd.Y == _screenStart.Y) return EndPosition.LeftAxis;
                else if (_screenEnd.Y < _screenStart.Y) return EndPosition.LeftTop;
                else return EndPosition.LeftBottom;
            }
            // 横坐标终点大于起点
            else
            {
                if (_screenEnd.Y == _screenStart.Y) return EndPosition.RightAxis;
                else if (_screenEnd.Y < _screenStart.Y) return EndPosition.RightTop;
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
        private Point _screenStart = new Point();
        private Point _screenEnd = new Point();

        #endregion
    }
}