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
        public Point? WorldStart
        {
            get => _worldStart;
            set
            {
                _worldStart = value;
                if (_worldStart == null)
                    DebugInfoManager.Instance.UpdateInfo("起点世界坐标", "无");
                else
                    DebugInfoManager.Instance.UpdateInfo("起点世界坐标", _worldStart.Value.ToPointString("G17"));
            }
        }

        /// <summary>终点世界坐标</summary>
        public Point? WorldEnd
        {
            get => _worldEnd;
            set
            {
                _worldEnd = value;
                if (_worldEnd == null)
                    DebugInfoManager.Instance.UpdateInfo("终点世界坐标", "无");
                else
                    DebugInfoManager.Instance.UpdateInfo("终点世界坐标", _worldEnd.Value.ToPointString("G17"));
            }
        }

        /// <summary>
        /// 原物理长度：根据起点与终点，计算出来的长度
        /// </summary>
        public double 原物理长度 { get; private set; } = 0;

        /// <summary>
        /// 原周角：根据起点与终点，计算出来的角度
        /// </summary>
        public double 原周角 { get; private set; } = 0;

        /// <summary>
        /// 当前物理长度：手动设置的物理长度
        /// </summary>
        public double 当前物理长度 { get => _当前物理长度; set => _当前物理长度 = value; }

        /// <summary>
        /// 当前周角：手动设置的角度
        /// </summary>
        public double 当前周角 { get => _当前周角; set => _当前周角 = value; }

        /// <summary>直线中点</summary>
        public Point LinearMid => PointTool.GetMidPoint(_linearStart, _linearEnd);

        /// <summary>圆弧中点</summary>
        public Point ArcMid { get; private set; } = new Point();

        #endregion

        #region 公开方法

        public override void Init()
        {
            _linePen.Freeze();
            _linearPen.Brush = new SolidColorBrush(Color.FromArgb(128, 255, 255, 255));
            _linearPen.DashStyle = new DashStyle([1, 3], 0);
            _linearPen.StartLineCap = PenLineCap.Square;
            _linearPen.EndLineCap = PenLineCap.Square;
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

            原物理长度 = PointTool.GetLength(_worldStart.Value, _worldEnd.Value);
            _当前物理长度 = 原物理长度;
            原周角 = PointTool.GetAngle(_worldStart.Value, _worldEnd.Value);
            _当前周角 = 原周角;
        }

        /// <summary>
        /// 根据当前物理长度移动世界终点
        /// </summary>
        public void MoveWorldEnd()
        {
            double offsetx = 0;
            double offsety = 0;
            // 特殊角处理，避免计算误差
            if (_当前周角 == 0) offsetx = _当前物理长度;
            else if (_当前周角 == 90) offsety = _当前物理长度;
            else if (_当前周角 == 180) offsetx = -_当前物理长度;
            else if (_当前周角 == 270) offsety = -_当前物理长度;
            // 其他角度处理
            else
            {
                double radians = _当前周角 * Math.PI / 180;
                offsetx = _当前物理长度 * Math.Cos(radians);
                offsety = _当前物理长度 * Math.Sin(radians);
            }
            // 更新终点坐标
            double endX = _worldStart!.Value.X + offsetx;
            double endY = _worldStart.Value.Y + offsety;
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

            List<LinearInfo> linearList = new List<LinearInfo>();
            ArcInfo arcInfo = new ArcInfo();

            _manager.UpdateInfo("起点坐标", $"({_screenStart.X:0.####}, {_screenStart.Y:0.####})");
            _manager.UpdateInfo("终点坐标", $"({_screenEnd.X:0.####}, {_screenEnd.Y:0.####})");

            EndPosition position = PointTool.GetEndPosition(_worldStart.Value, _worldEnd.Value);
            _manager.UpdateInfo("终点位置", _positionName[position]);

            // 填充标注线
            FillLineInfo(position, linearList, arcInfo);
            // 绘制标注线
            foreach (var line in linearList)
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

        private void FillLineInfo(EndPosition position, List<LinearInfo> linearList, ArcInfo arcInfo)
        {
            Point mathStart = new Point(_screenStart.X, -_screenStart.Y);
            Point mathEnd = new Point(_screenEnd.X, -_screenEnd.Y);
            double 垂直偏移 = mathEnd.Y - mathStart.Y;
            double 水平偏移 = mathEnd.X - mathStart.X;
            _manager.UpdateInfo("垂直偏移", $"{垂直偏移:G17}");
            _manager.UpdateInfo("水平偏移", $"{水平偏移:G17}");
            double 斜边长度 = Math.Sqrt(水平偏移 * 水平偏移 + 垂直偏移 * 垂直偏移);
            _manager.UpdateInfo("斜边长度", $"{斜边长度:G17}");
            _manager.UpdateInfo("斜边物理长度", $"{_当前物理长度:G17}");

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
                        ArcMid = new Point(arcCenterX, arcCenterY);
                        double 角度 = 斜边弧度 * 180 / Math.PI;
                        _manager.UpdateInfo("斜边角度", $"{角度:G17}°");
                        // 逆时针旋转90度
                        double 旋转后弧度 = 斜边弧度 + Math.PI / 2;
                        double 旋转后角度 = 旋转后弧度 * 180 / Math.PI;
                        _manager.UpdateInfo("旋转后角度", $"{旋转后角度:G17}°");
                        double 旋转后横坐标偏移 = Math.Cos(旋转后弧度) * 60;
                        double 旋转后纵坐标偏移 = Math.Sin(旋转后弧度) * 60;
                        _manager.UpdateInfo("旋转后横坐标偏移", $"{旋转后横坐标偏移:G17}");
                        _manager.UpdateInfo("旋转后纵坐标偏移", $"{旋转后纵坐标偏移:G17}");
                        // 添加标注线
                        LinearInfo line = new LinearInfo
                        {
                            Start = _screenStart,
                            End = new Point(_screenStart.X + 旋转后横坐标偏移, _screenStart.Y - 旋转后纵坐标偏移)
                        };
                        linearList.Add(line);
                        line = new LinearInfo
                        {
                            Start = line.End,
                            End = new Point(_screenEnd.X + 旋转后横坐标偏移, _screenEnd.Y - 旋转后纵坐标偏移)
                        };
                        linearList.Add(line);
                        _linearStart = line.Start;
                        _linearEnd = line.End;
                        line = new LinearInfo
                        {
                            Start = line.End,
                            End = _screenEnd
                        };
                        linearList.Add(line);
                        // 添加半径线
                        line = new LinearInfo
                        {
                            Start = new Point(_screenStart.X, _screenStart.Y),
                            End = new Point(_screenStart.X + 斜边长度, _screenStart.Y)
                        };
                        linearList.Add(line);
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
                        ArcMid = new Point(arcCenterX, arcCenterY);
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
                        LinearInfo line = new LinearInfo
                        {
                            Start = _screenStart,
                            End = new Point(_screenStart.X + 旋转后横坐标偏移, _screenStart.Y - 旋转后纵坐标偏移)
                        };
                        linearList.Add(line);
                        line = new LinearInfo
                        {
                            Start = line.End,
                            End = new Point(_screenEnd.X + 旋转后横坐标偏移, _screenEnd.Y - 旋转后纵坐标偏移)
                        };
                        linearList.Add(line);
                        _linearStart = line.Start;
                        _linearEnd = line.End;
                        line = new LinearInfo
                        {
                            Start = line.End,
                            End = _screenEnd
                        };
                        linearList.Add(line);
                        // 添加半径线
                        line = new LinearInfo
                        {
                            Start = new Point(_screenStart.X, _screenStart.Y),
                            End = new Point(_screenStart.X + 斜边长度, _screenStart.Y)
                        };
                        linearList.Add(line);
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

        #endregion

        #region 私有类型

        /// <summary>
        /// 标注线信息
        /// </summary>
        private class LinearInfo
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

        private Point _screenStart = new Point();
        private Point _screenEnd = new Point();
        private Point _linearStart;
        private Point _linearEnd;

        private readonly Pen _linePen = new Pen(Brushes.White, 1);
        private readonly Pen _linearPen = new Pen(Brushes.White, 1);
        private readonly DebugInfoManager _manager = DebugInfoManager.Instance;
        private readonly Dictionary<EndPosition, string> _positionName = new Dictionary<EndPosition, string>();

        #endregion

        #region 属性字段

        private Point? _worldStart = null;
        private Point? _worldEnd = null;
        private double _当前物理长度 = 0;
        private double _当前周角 = 0;

        #endregion
    }
}