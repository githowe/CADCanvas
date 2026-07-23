using CADCanvas.SubSystem.DrawingSystem;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using XLogic.Wpf.Drawing;

namespace CADCanvas.SubSystem.EditerSystem.Layer
{
    /// <summary>
    /// 网格图层
    /// </summary>
    public class GridLayer : DrawingLayer, IWorldGrid
    {
        #region 构造方法

        public GridLayer() => ResizeLevel += 91;

        #endregion

        #region 属性

        /// <summary>网格中心</summary>
        public Point Center { get; set; }

        /// <summary>格子大小。单位：像素</summary>
        public int GridSize { get => _gridPixelSize; }

        /// <summary>格子大小。单位：米</summary>
        public int GridPhysicsSize { get => _gridPhysicsSize; }

        /// <summary>格子横线数量：实际数量会根据图层大小优化</summary>
        public int GridLineCount { get; private set; } = 40;

        /// <summary>格子垂线数量：实际数量会根据图层大小优化</summary>
        public int GridListCount { get; private set; } = 22;

        /// <summary>缩放级别</summary>
        public ResizeLevel ResizeLevel { get; set; } = new ResizeLevel();

        #endregion

        #region 公开方法

        public override void Init()
        {
            _normalLine.Freeze();
            _microLine.Freeze();
            _centerLine.Freeze();
            _centerList.Freeze();
        }

        /// <summary>
        /// 缩放图层
        /// </summary>
        /// <param name="resizeCenter">缩放中心</param>
        /// <param name="resizeIncrement">缩放增量</param>
        public void ResizeLayer(Point resizeCenter, int resizeIncrement)
        {
            // 计算缩放比
            double resizeRatio = CalculateResizeRatio(resizeIncrement);

            // 计算缩放中心与网格中心的偏移距离
            Point offset = new(Center.X - resizeCenter.X, Center.Y - resizeCenter.Y);
            // 根据缩放比计算新偏移距离
            Point newOffset = new(offset.X * resizeRatio, offset.Y * resizeRatio);
            // 更新中心偏移距离
            _centerOffset = new()
            {
                X = _centerOffset.X + (newOffset.X - offset.X),
                Y = _centerOffset.Y + (newOffset.Y - offset.Y),
            };

            ResizeLevel += resizeIncrement;
            Update();
        }

        /// <summary>
        /// 平移图层
        /// </summary>
        /// <param name="offset">偏移</param>
        public void MoveLayer(Point offset)
        {
            _moveOffset = offset;
            Update();
        }

        /// <summary>
        /// 应用偏移：平移结束后调用
        /// </summary>
        public void ApplyOffset()
        {
            _centerOffset = new Point(_centerOffset.X + _moveOffset.X, _centerOffset.Y + _moveOffset.Y);
            _moveOffset = new();
        }

        /// <summary>
        /// 屏幕坐标转世界坐标
        /// </summary>
        public Point ToWorld(Point screenPoint)
        {
            Point worldPoint = new Point();
            double pixelPhysicsLength = (double)_gridPhysicsSize / _gridPixelSize;
            worldPoint.X = (screenPoint.X - Center.X) * pixelPhysicsLength;
            worldPoint.Y = (screenPoint.Y - Center.Y) * pixelPhysicsLength;
            return worldPoint;
        }

        /// <summary>
        /// 世界坐标转屏幕坐标
        /// </summary>
        public Point ToScreen(Point worldPoint) => ToScreen(worldPoint.X, worldPoint.Y);

        /// <summary>
        /// 世界坐标转屏幕坐标
        /// </summary>
        public Point ToScreen(double x, double y)
        {
            Point screenPoint = new Point();
            double pixelPhysicsLength = (double)_gridPhysicsSize / _gridPixelSize;
            screenPoint.X = x / pixelPhysicsLength + Center.X;
            screenPoint.Y = y / pixelPhysicsLength + Center.Y;
            return screenPoint;
        }

        #endregion

        #region 内部方法

        protected override void OnUpdate()
        {
            int line;
            int list;
            int subdivide;

            // 网格像素大小
            _gridPixelSize = CalculateGridSize(ResizeLevel);

            // 更新中心点
            Point realOffset = new(_centerOffset.X + _moveOffset.X, _centerOffset.Y + _moveOffset.Y);
            Center = new Point((int)Width / 2 + realOffset.X, (int)Height / 2 + realOffset.Y);

            // 更新起始点
            UpdateDrawStart();
            // 更新网格线数量
            UpdateGridLineCount();

            // 绘制细分线
            _currentPen = _microLine;
            for (line = 0; line < GridLineCount; line++)
                for (subdivide = 1; subdivide < 5; subdivide++)
                    DrawHorizontalLine(line * _gridPixelSize + _gridPixelSize / 5 * subdivide);
            for (list = 0; list < GridListCount; list++)
                for (subdivide = 1; subdivide < 5; subdivide++)
                    DrawVerticalLine(list * _gridPixelSize + _gridPixelSize / 5 * subdivide);

            // 绘制主线
            _currentPen = _normalLine;
            for (line = 0; line < GridLineCount; line++)
            {
                // 跳过中心线
                if (_drawStart.Y + line * _gridPixelSize == Center.Y) continue;
                DrawHorizontalLine(line * _gridPixelSize);
            }
            for (list = 0; list < GridListCount; list++)
            {
                // 跳过中心线
                if (_drawStart.X + list * _gridPixelSize == Center.X) continue;
                DrawVerticalLine(list * _gridPixelSize);
            }

            // 绘制中心线
            _currentPen = _centerLine;
            DrawHorizontalLine((int)Center.Y - (int)_drawStart.Y);
            _currentPen = _centerList;
            DrawVerticalLine((int)Center.X - (int)_drawStart.X);
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 计算缩放比
        /// </summary>
        private double CalculateResizeRatio(int resizeIncrement)
        {
            // 缩放级别
            ResizeLevel oldResizeLevel = ResizeLevel;
            ResizeLevel newResizeLevel = ResizeLevel + resizeIncrement;
            // 网格物理大小
            double oldPhysicSize = Math.Pow(5, oldResizeLevel.MainLevelMax - oldResizeLevel.MainLevel);
            double newPhysicSize = Math.Pow(5, newResizeLevel.MainLevelMax - newResizeLevel.MainLevel);
            // 单像素物理长度
            double oldPixelLength = oldPhysicSize / CalculateGridSize(oldResizeLevel);
            double newPixelLength = newPhysicSize / CalculateGridSize(newResizeLevel);

            // 应用物理大小
            _gridPhysicsSize = (int)newPhysicSize;

            return oldPixelLength / newPixelLength;
        }

        /// <summary>
        /// 计算网格大小
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int CalculateGridSize(ResizeLevel resizeLevel)
        {
            return _gridBaseSize + resizeLevel.SubLevel * _gridIncrementSize;
        }

        /// <summary>
        /// 更新绘图起始点
        /// </summary>
        private void UpdateDrawStart()
        {
            // 横坐标
            if (Center.X < 0)
                _drawStart.X = Center.X + (int)(0 - Center.X) / _gridPixelSize * _gridPixelSize;
            else
                _drawStart.X = Center.X - ((int)Center.X / _gridPixelSize + 1) * _gridPixelSize;

            // 纵坐标
            if (Center.Y < 0)
                _drawStart.Y = Center.Y + (int)(0 - Center.Y) / _gridPixelSize * _gridPixelSize;
            else
                _drawStart.Y = Center.Y - ((int)Center.Y / _gridPixelSize + 1) * _gridPixelSize;
        }

        /// <summary>
        /// 更新网格线数量
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateGridLineCount()
        {
            GridLineCount = (int)(Height / _gridPixelSize) + 2;
            GridListCount = (int)(Width / _gridPixelSize) + 2;
        }

        /// <summary>
        /// 绘制水平线
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DrawHorizontalLine(int y)
        {
            int realy = (int)_drawStart.Y + y;
            if (realy < 0 || realy > Height || _dc == null) return;
            _dc.DrawLine(_currentPen, new Point(0, realy), new Point(Width, realy));
        }

        /// <summary>
        /// 绘制垂直线
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void DrawVerticalLine(int x)
        {
            int realx = (int)_drawStart.X + x;
            if (realx < 0 || realx > Width || _dc == null) return;
            _dc.DrawLine(_currentPen, new Point(realx, 0), new Point(realx, Height));
        }

        #endregion

        #region 画笔

        // 网格线
        private readonly Pen _normalLine = new(new SolidColorBrush(Color.FromArgb(255, 30, 30, 30)), 2);
        private readonly Pen _microLine = new(new SolidColorBrush(Color.FromArgb(255, 20, 20, 20)), 2);
        private readonly Pen _centerLine = new(new SolidColorBrush(Color.FromArgb(255, 103, 43, 45)), 2);
        private readonly Pen _centerList = new(new SolidColorBrush(Color.FromArgb(255, 42, 104, 45)), 2);
        // 当前画笔
        private Pen? _currentPen;

        #endregion

        #region 网格绘制

        /// <summary>网格基础大小</summary>
        private readonly int _gridBaseSize = 50;
        /// <summary>网格增量大小</summary>
        private readonly int _gridIncrementSize = 10;

        /// <summary>绘图起始点</summary>
        private Point _drawStart = new();
        /// <summary>中心偏移</summary>
        private Point _centerOffset = new();
        /// <summary>临时偏移</summary>
        private Point _moveOffset = new();

        #endregion

        #region 属性字段

        /// <summary>网格像素大小</summary>
        private int _gridPixelSize;
        /// <summary>网格物理大小</summary>
        private int _gridPhysicsSize = 1;

        #endregion
    }
}