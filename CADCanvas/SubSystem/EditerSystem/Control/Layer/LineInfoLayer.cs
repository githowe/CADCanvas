using CADCanvas.SubSystem.DebugSystem;
using CADCanvas.SubSystem.EditerSystem.Layer;
using System.Windows;
using System.Windows.Media;
using XLogic.Wpf.Drawing;

namespace CADCanvas.SubSystem.EditerSystem.Control.Layer
{
    /// <summary>
    /// 终点位置
    /// </summary>
    public enum EndPosition
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

    public class LineInfo
    {
        public Point Start { get; set; } = new Point();

        public Point End { get; set; } = new Point();
    }

    /// <summary>
    /// 圆弧信息
    /// </summary>
    public class ArcInfo
    {
        public Point Start { get; set; } = new Point();

        public Point End { get; set; } = new Point();

        public double Radius { get; set; } = 0;
    }

    public class LineInfoLayer : DrawingLayer
    {
        public GridLayer Grid { get; set; } = null;

        /// <summary>起点屏幕坐标</summary>
        public Point StartPoint { get; set; } = new Point();

        /// <summary>终点屏幕坐标</summary>
        public Point EndPoint { get; set; } = new Point();

        public override void Init()
        {
            _pen.Brush = new SolidColorBrush(Color.FromArgb(128, 255, 255, 255));
            _pen.DashStyle = new DashStyle([2, 2], 0);
            _pen.DashCap = PenLineCap.Flat;
            _pen.Freeze();

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
        /// 获取直线中点
        /// </summary>
        public Point GetLineCenter()
        {
            double x = _lineStart.X + (_lineEnd.X - _lineStart.X) / 2;
            double y = _lineStart.Y + (_lineEnd.Y - _lineStart.Y) / 2;
            return new Point(x, y);
        }

        /// <summary>
        /// 获取直线长度
        /// </summary>
        public double GetLineLength() => _worldLineLength;

        /// <summary>
        /// 获取直线角度
        /// </summary>
        public double GetLineAngle() => _lineAngle;

        /// <summary>
        /// 获取圆弧中点
        /// </summary>
        public Point GetArcCenter() => _arcCenter;

        protected override void OnUpdate()
        {
            _worldStart = Grid.ToWorld(StartPoint);
            _worldEnd = Grid.ToWorld(EndPoint);
            // 计算直线长度
            double dx = _worldEnd.X - _worldStart.X;
            double dy = _worldEnd.Y - _worldStart.Y;
            _worldLineLength = Math.Sqrt(dx * dx + dy * dy);

            List<LineInfo> LineList = new List<LineInfo>();
            ArcInfo arcInfo = new ArcInfo();

            _manager.UpdateInfo("起点坐标", $"({StartPoint.X}, {StartPoint.Y})");
            _manager.UpdateInfo("终点坐标", $"({EndPoint.X}, {EndPoint.Y})");

            EndPosition position = GetEndPosition();
            _manager.UpdateInfo("终点位置", _positionName[position]);

            // 填充标注线
            FillLineInfo(position, LineList, arcInfo);
            // 绘制标注线
            foreach (var line in LineList)
                _dc.DrawLine(_pen, line.Start, line.End);
            // 绘制圆弧
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
            _dc.DrawGeometry(null, _pen, pathGeometry);
        }

        private void FillLineInfo(EndPosition position, List<LineInfo> LineList, ArcInfo arcInfo)
        {
            Point mathStart = new Point(StartPoint.X, -StartPoint.Y);
            Point mathEnd = new Point(EndPoint.X, -EndPoint.Y);
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
                        double arcCenterX = StartPoint.X + Math.Cos(中线弧度) * 斜边长度;
                        double arcCenterY = StartPoint.Y - Math.Sin(中线弧度) * 斜边长度;
                        _arcCenter = new Point(arcCenterX, arcCenterY);
                        double 角度 = 斜边弧度 * 180 / Math.PI;
                        _lineAngle = 角度;
                        _manager.UpdateInfo("斜边弧度", $"{斜边弧度:0.######}°");
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
                            Start = StartPoint,
                            End = new Point(StartPoint.X + 旋转后横坐标偏移, StartPoint.Y - 旋转后纵坐标偏移)
                        };
                        LineList.Add(line);
                        line = new LineInfo
                        {
                            Start = line.End,
                            End = new Point(EndPoint.X + 旋转后横坐标偏移, EndPoint.Y - 旋转后纵坐标偏移)
                        };
                        LineList.Add(line);
                        _lineStart = line.Start;
                        _lineEnd = line.End;
                        line = new LineInfo
                        {
                            Start = line.End,
                            End = EndPoint
                        };
                        LineList.Add(line);
                        // 添加半径线
                        line = new LineInfo
                        {
                            Start = new Point(StartPoint.X, StartPoint.Y),
                            End = new Point(StartPoint.X + 斜边长度, StartPoint.Y)
                        };
                        LineList.Add(line);
                        // 设置圆弧信息
                        arcInfo.Start = new Point(StartPoint.X + 斜边长度, StartPoint.Y);
                        arcInfo.End = EndPoint;
                        arcInfo.Radius = 斜边长度;
                    }
                    break;
                case EndPosition.LeftBottom:
                case EndPosition.BottomAxis:
                case EndPosition.RightBottom:
                    {
                        double 斜边弧度 = Math.Atan2(垂直偏移, 水平偏移) + Math.PI * 2;
                        double 中线弧度 = 斜边弧度 / 2;
                        double arcCenterX = StartPoint.X - Math.Cos(中线弧度) * 斜边长度;
                        double arcCenterY = StartPoint.Y + Math.Sin(中线弧度) * 斜边长度;
                        _arcCenter = new Point(arcCenterX, arcCenterY);
                        double 角度 = 斜边弧度 * 180 / Math.PI;
                        _lineAngle = 360 - 角度;
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
                            Start = StartPoint,
                            End = new Point(StartPoint.X + 旋转后横坐标偏移, StartPoint.Y - 旋转后纵坐标偏移)
                        };
                        LineList.Add(line);
                        line = new LineInfo
                        {
                            Start = line.End,
                            End = new Point(EndPoint.X + 旋转后横坐标偏移, EndPoint.Y - 旋转后纵坐标偏移)
                        };
                        LineList.Add(line);
                        _lineStart = line.Start;
                        _lineEnd = line.End;
                        line = new LineInfo
                        {
                            Start = line.End,
                            End = EndPoint
                        };
                        LineList.Add(line);
                        // 添加半径线
                        line = new LineInfo
                        {
                            Start = new Point(StartPoint.X, StartPoint.Y),
                            End = new Point(StartPoint.X + 斜边长度, StartPoint.Y)
                        };
                        LineList.Add(line);
                        // 设置圆弧信息
                        arcInfo.Start = EndPoint;
                        arcInfo.End = new Point(StartPoint.X + 斜边长度, StartPoint.Y);
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
            if (EndPoint.X == StartPoint.X)
            {
                if (EndPoint.Y == StartPoint.Y) return EndPosition.Origin;
                else if (EndPoint.Y < StartPoint.Y) return EndPosition.TopAxis;
                else return EndPosition.BottomAxis;
            }
            // 横坐标终点小于起点
            else if (EndPoint.X < StartPoint.X)
            {
                if (EndPoint.Y == StartPoint.Y) return EndPosition.LeftAxis;
                else if (EndPoint.Y < StartPoint.Y) return EndPosition.LeftTop;
                else return EndPosition.LeftBottom;
            }
            // 横坐标终点大于起点
            else
            {
                if (EndPoint.Y == StartPoint.Y) return EndPosition.RightAxis;
                else if (EndPoint.Y < StartPoint.Y) return EndPosition.RightTop;
                else return EndPosition.RightBottom;
            }
        }

        private readonly Pen _pen = new Pen(Brushes.White, 1);
        private readonly DebugInfoManager _manager = DebugInfoManager.Instance;
        private readonly Dictionary<EndPosition, string> _positionName = new Dictionary<EndPosition, string>();

        private Point _lineStart;
        private Point _lineEnd;
        private double _lineAngle;
        
        private Point _worldStart;
        private Point _worldEnd;
        private double _worldLineLength;

        private Point _arcCenter;
    }
}