using System.Windows;
using System.Windows.Media;
using XLogic.Wpf;
using XLogic.Wpf.Drawing;

namespace CADCanvas.SubSystem.EditerSystem.Layer
{
    /// <summary>
    /// 圆形工具图层
    /// </summary>
    public class CircleToolLayer : DrawingLayer
    {
        #region 属性

        public GridLayer? Grid { get; set; } = null;

        /// <summary>世界起点。即圆心</summary>
        public Point? WorldStart { get => _worldStart; set => _worldStart = value; }

        /// <summary>世界终点。即半径终点</summary>
        public Point? WorldEnd { get => _worldEnd; set => _worldEnd = value; }

        /// <summary>
        /// 原物理半径：根据起点与终点计算出来的半径长度
        /// </summary>
        public double 原物理半径 { get; set; } = 0;

        /// <summary>
        /// 原周角：根据起点与终点，计算出来的角度
        /// </summary>
        public double 原周角 { get; private set; } = 0;

        /// <summary>
        /// 当前物理半径：手动设置的物理半径
        /// </summary>
        public double 当前物理半径 { get => _当前物理半径; set => _当前物理半径 = value; }

        /// <summary>
        /// 当前周角：手动设置的角度
        /// </summary>
        public double 当前周角 { get => _当前周角; set => _当前周角 = value; }

        /// <summary>标注线中点</summary>
        public Point LinearMid => PointTool.GetMidPoint(_linearStart, _linearEnd);

        #endregion

        #region 公开方法

        public override void Init()
        {
            // 圆画笔
            _circlePen.Freeze();
            // 半径画笔
            _radiusPen.Brush = new SolidColorBrush(Color.FromArgb(255, 255, 200, 50));
            _radiusPen.DashStyle = new DashStyle([15, 9], 0);
            _radiusPen.StartLineCap = PenLineCap.Square;
            _radiusPen.EndLineCap = PenLineCap.Square;
            _radiusPen.Freeze();
            // 标注线画笔
            _linearPen.Brush = new SolidColorBrush(Color.FromArgb(128, 255, 255, 255));
            _linearPen.DashStyle = new DashStyle([1, 3], 0);
            _linearPen.StartLineCap = PenLineCap.Square;
            _linearPen.EndLineCap = PenLineCap.Square;
            _linearPen.Freeze();
            // 禁用命中
            IsHitTestVisible = false;
        }

        /// <summary>
        /// 更新信息。根据起点与终点，计算半径与角度
        /// </summary>
        public void UpdateInfo()
        {
            if (_worldStart == null || _worldEnd == null)
                return;

            原物理半径 = PointTool.GetLength(_worldStart.Value, _worldEnd.Value);
            _当前物理半径 = 原物理半径;
            原周角 = PointTool.GetAngle(_worldStart.Value, _worldEnd.Value);
            _当前周角 = 原周角;
        }

        /// <summary>
        /// 移动世界终点。即根据物理半径与角度移动半径终点
        /// </summary>
        public void MoveWorldEnd()
        {
            double offsetx = 0;
            double offsety = 0;
            // 特殊角处理，避免计算误差
            if (_当前周角 == 0) offsetx = _当前物理半径;
            else if (_当前周角 == 90) offsety = _当前物理半径;
            else if (_当前周角 == 180) offsetx = -_当前物理半径;
            else if (_当前周角 == 270) offsety = -_当前物理半径;
            // 其他角度处理
            else
            {
                double radians = _当前周角 * Math.PI / 180;
                offsetx = _当前物理半径 * Math.Cos(radians);
                offsety = _当前物理半径 * Math.Sin(radians);
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
            double radius = Grid.ToScreenLength(_当前物理半径);

            // 绘制圆
            _dc.DrawEllipse(null, _circlePen, _screenStart, radius, radius);
            // 绘制半径线
            _dc.DrawLine(_radiusPen, _screenStart, _screenEnd);

            List<LinearInfo> linearList = new List<LinearInfo>();
            EndPosition position = PointTool.GetEndPosition(_worldStart.Value, _worldEnd.Value);
            // 填充标注线
            FillLinear(position, linearList);
            // 绘制标注线
            foreach (var line in linearList)
                _dc.DrawLine(_linearPen, line.Start, line.End);
        }

        #endregion

        #region 私有方法

        private void FillLinear(EndPosition position, List<LinearInfo> linearList)
        {
            Point mathStart = new Point(_screenStart.X, -_screenStart.Y);
            Point mathEnd = new Point(_screenEnd.X, -_screenEnd.Y);
            double 垂直偏移 = mathEnd.Y - mathStart.Y;
            double 水平偏移 = mathEnd.X - mathStart.X;
            double 斜边长度 = Math.Sqrt(水平偏移 * 水平偏移 + 垂直偏移 * 垂直偏移);

            switch (position)
            {
                case EndPosition.RightAxis:
                case EndPosition.RightTop:
                case EndPosition.TopAxis:
                case EndPosition.LeftTop:
                case EndPosition.LeftAxis:
                    {
                        double 斜边弧度 = Math.Atan2(垂直偏移, 水平偏移);
                        double 旋转后弧度 = 斜边弧度 + Math.PI / 2;
                        double 旋转后横坐标偏移 = Math.Cos(旋转后弧度) * 60;
                        double 旋转后纵坐标偏移 = Math.Sin(旋转后弧度) * 60;
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
                    }
                    break;
                case EndPosition.LeftBottom:
                case EndPosition.BottomAxis:
                case EndPosition.RightBottom:
                    {
                        double 斜边弧度 = Math.Atan2(垂直偏移, 水平偏移) + Math.PI * 2;
                        // 顺时针旋转90度
                        double 旋转后弧度 = 斜边弧度 - Math.PI / 2;
                        double 旋转后横坐标偏移 = Math.Cos(旋转后弧度) * 60;
                        double 旋转后纵坐标偏移 = Math.Sin(旋转后弧度) * 60;
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
                    }
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

        #endregion

        #region 字段

        private Point _screenStart = new Point();
        private Point _screenEnd = new Point();
        private Point _linearStart;
        private Point _linearEnd;

        /// <summary>圆画笔</summary>
        private readonly Pen _circlePen = new Pen(Brushes.White, 1);
        /// <summary>半径画笔</summary>
        private readonly Pen _radiusPen = new Pen();
        /// <summary>标注线画笔</summary>
        private readonly Pen _linearPen = new Pen();

        #endregion

        #region 属性字段

        private Point? _worldStart = null;
        private Point? _worldEnd = null;
        private double _当前物理半径 = 0;
        private double _当前周角 = 0;

        #endregion
    }
}