using CADCanvas.SubSystem.DrawingSystem;
using CADCanvas.SubSystem.EditerSystem.Layer;
using System.Windows;
using System.Windows.Input;
using XLogic.Base.UI;
using XLogic.Wpf.Drawing;

namespace CADCanvas.SubSystem.EditerSystem.Component
{
    /// <summary>
    /// 图层组件
    /// </summary>
    public class LayerComponent : Component<Editer>
    {
        #region 图层

        public GridLayer GridLayer => 网格图层;

        public GraphicLayer GraphicLayer => 图形图层;

        public LineToolLayer LineToolLayer => 直线工具图层;

        public CircleToolLayer CircleToolLayer => 圆形工具图层;

        public TrimToolLayer TrimToolLayer => 修剪工具图层;

        public RTreeViewLayer RTreeViewLayer => 空间索引可视化图层;

        public PolarTrackingLayer PolarTrackingLayer => 极轴追踪图层;

        public SnapMarkLayer SnapMarkLayer => 捕捉标记图层;

        public CursorLayer CursorLayer => 光标图层;

        #endregion

        #region 公开方法

        public Point GetScreenPoint() => Mouse.GetPosition(_host.Layer_Mouse);

        public Point GetScreenPoint(Point worldPoint) => 网格图层.ToScreen(worldPoint);

        /// <summary>
        /// 获取当前鼠标的世界坐标
        /// </summary>
        public Point GetWorldPoint() => 网格图层.ToWorld(Mouse.GetPosition(_host.Layer_Mouse));

        public Point GetWorldPoint(Point screenPoint) => 网格图层.ToWorld(screenPoint);

        public void UpdateGrid()
        {
            网格图层.Width = _host.LayerBox.ActualWidth;
            网格图层.Height = _host.LayerBox.ActualHeight;
            网格图层.Update();
        }

        /// <summary>
        /// 更新全部图层。调整窗口后调用
        /// </summary>
        public void UpdateAll()
        {
            foreach (var item in _layerList)
            {
                item.Width = _host.LayerBox.ActualWidth;
                item.Height = _host.LayerBox.ActualHeight;
                item.Update();
            }
        }

        /// <summary>
        /// 更新图层位置。拖动或缩放网格后调用
        /// </summary>
        public void UpdateLayerPosition()
        {
            foreach (var item in _layerList) item.Update();
        }

        /// <summary>
        /// 平移网格
        /// </summary>
        public void MoveGrid(Point offset) => 网格图层.MoveLayer(offset);

        /// <summary>
        /// 应用平移
        /// </summary>
        public void ApplyMoveGrid() => 网格图层.ApplyOffset();

        /// <summary>
        /// 缩放网格
        /// </summary>
        public void ResizeGrid(MouseWheelEventArgs e)
        {
            网格图层.ResizeLayer(Mouse.GetPosition(_host.Layer_Mouse), e.Delta / 120);
        }

        #region 图形图层

        /// <summary>
        /// 添加图形
        /// </summary>
        public void AddGraphic(GeoVisual graphic)
        {
            graphic.Init();
            图形图层.GeoVisualList.Add(graphic);
        }

        public void UpdateGraphic()
        {
            图形图层.Update();
        }

        #endregion

        #region 直线工具图层

        /// <summary>
        /// 获取直线工具世界起点
        /// </summary>
        public Point GetLineToolWorldStart() => 直线工具图层.WorldStart.Value;

        /// <summary>
        /// 获取直线工具世界终点
        /// </summary>
        public Point GetLineToolWorldEnd() => 直线工具图层.WorldEnd.Value;

        /// <summary>
        /// 设置直线工具起点
        /// </summary>
        public void SetLineToolStart(Point point)
        {
            直线工具图层.WorldStart = point;
        }

        /// <summary>
        /// 清空直线工具
        /// </summary>
        public void ClearLineTool()
        {
            直线工具图层.WorldStart = null;
            直线工具图层.WorldEnd = null;
            直线工具图层.Clear();
        }

        #endregion

        #endregion

        #region 生命周期

        protected override void Init()
        {
            // 添加绘图图层
            网格图层 = new GridLayer();
            _host.LayerBox.Children.Add(网格图层);
            _layerList.Add(网格图层);
            图形图层 = new GraphicLayer { Grid = 网格图层 };
            _host.LayerBox.Children.Add(图形图层);
            _layerList.Add(图形图层);
            // 添加工具图层
            直线工具图层 = new LineToolLayer { Grid = 网格图层 };
            _host.LayerBox.Children.Add(直线工具图层);
            _layerList.Add(直线工具图层);
            圆形工具图层 = new CircleToolLayer { Grid = 网格图层 };
            _host.LayerBox.Children.Add(圆形工具图层);
            _layerList.Add(圆形工具图层);
            修剪工具图层 = new TrimToolLayer { Grid = 网格图层 };
            _host.LayerBox.Children.Add(修剪工具图层);
            _layerList.Add(修剪工具图层);
            // 添加可视化图层
            空间索引可视化图层 = new RTreeViewLayer { Grid = 网格图层 };
            _host.Layer_RTree.Children.Add(空间索引可视化图层);
            _layerList.Add(空间索引可视化图层);
            // 添加标记图层
            极轴追踪图层 = new PolarTrackingLayer { Grid = 网格图层 };
            _host.Layer_Mark.Children.Add(极轴追踪图层);
            _layerList.Add(极轴追踪图层);
            捕捉标记图层 = new SnapMarkLayer { Grid = 网格图层 };
            _host.Layer_Mark.Children.Add(捕捉标记图层);
            _layerList.Add(捕捉标记图层);
            // 添加光标图层
            光标图层 = new CursorLayer();
            _host.Layer_Cursor.Children.Add(光标图层);
            _layerList.Add(光标图层);

            // 初始化图层
            foreach (var item in _layerList) item.Init();
        }

        protected override void Enable()
        {
            foreach (var item in _layerList)
            {
                item.Width = _host.LayerBox.ActualWidth;
                item.Height = _host.LayerBox.ActualHeight;
            }
        }

        #endregion

        #region 图层

        #region 绘图图层

        /// <summary>网格</summary>
        private GridLayer? 网格图层;
        /// <summary>图形 </summary>
        private GraphicLayer? 图形图层;

        #endregion

        #region 工具图层 - 各种工具的可视化图层

        /// <summary>直线工具</summary>
        private LineToolLayer? 直线工具图层;
        /// <summary>圆形工具</summary>
        private CircleToolLayer? 圆形工具图层;
        /// <summary>修剪工具</summary>
        private TrimToolLayer? 修剪工具图层;

        #endregion

        #region 可视化图层 - 用于调试

        private RTreeViewLayer? 空间索引可视化图层;

        #endregion

        #region 标记图层

        /// <summary>极轴追踪</summary>
        private PolarTrackingLayer? 极轴追踪图层;
        /// <summary>捕捉标记</summary>
        private SnapMarkLayer? 捕捉标记图层;

        #endregion

        private CursorLayer? 光标图层;

        /// <summary>图层列表</summary>
        private readonly List<DrawingLayer> _layerList = new List<DrawingLayer>();

        #endregion
    }
}