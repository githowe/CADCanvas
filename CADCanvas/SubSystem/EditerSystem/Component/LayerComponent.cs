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

        public GridLayer GridLayer => _gridLayer;

        public GraphicLayer GraphicLayer => _graphicLayer;

        public LineToolLayer LineToolLayer => _lineToolLayer;

        public RTreeViewLayer RTreeViewLayer => _rTreeViewLayer;

        public PolarTrackingLayer PolarTrackingLayer => _polarTrackingLayer;

        public CatchMarkLayer CatchMarkLayer => _catchMarkLayer;

        #endregion

        #region 公开方法

        public Point GetScreenPoint() => Mouse.GetPosition(_host.Layer_Mouse);

        /// <summary>
        /// 获取当前鼠标的世界坐标
        /// </summary>
        public Point GetWorldPoint() => _gridLayer.ToWorld(Mouse.GetPosition(_host.Layer_Mouse));

        public Point GetWorldPoint(Point screenPoint) => _gridLayer.ToWorld(screenPoint);

        public void UpdateGrid()
        {
            _gridLayer.Width = _host.LayerBox.ActualWidth;
            _gridLayer.Height = _host.LayerBox.ActualHeight;
            _gridLayer.Update();
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
        public void MoveGrid(Point offset) => _gridLayer.MoveLayer(offset);

        /// <summary>
        /// 应用平移
        /// </summary>
        public void ApplyMoveGrid() => _gridLayer.ApplyOffset();

        /// <summary>
        /// 缩放网格
        /// </summary>
        public void ResizeGrid(MouseWheelEventArgs e)
        {
            _gridLayer.ResizeLayer(Mouse.GetPosition(_host.Layer_Mouse), e.Delta / 120);
        }

        #region 图形图层

        /// <summary>
        /// 添加图形
        /// </summary>
        public void AddGraphic(GeoVisual graphic)
        {
            graphic.Init();
            _graphicLayer.GeoVisualList.Add(graphic);
        }

        public void UpdateGraphic()
        {
            _graphicLayer.Update();
        }

        #endregion

        #region 直线工具图层

        /// <summary>
        /// 获取直线工具世界起点
        /// </summary>
        public Point GetLineToolWorldStart() => _lineToolLayer.WorldStart.Value;

        /// <summary>
        /// 获取直线工具世界终点
        /// </summary>
        public Point GetLineToolWorldEnd() => _lineToolLayer.WorldEnd.Value;

        /// <summary>
        /// 设置直线工具起点
        /// </summary>
        public void SetLineToolStart(Point point)
        {
            _lineToolLayer.WorldStart = point;
        }

        /// <summary>
        /// 清空直线工具
        /// </summary>
        public void ClearLineTool()
        {
            _lineToolLayer.WorldStart = null;
            _lineToolLayer.WorldEnd = null;
            _lineToolLayer.Clear();
        }

        #endregion

        #endregion

        #region 生命周期

        protected override void Init()
        {
            _gridLayer = new GridLayer();
            _host.LayerBox.Children.Add(_gridLayer);
            _gridLayer.Init();
            _layerList.Add(_gridLayer);

            _graphicLayer = new GraphicLayer { Grid = _gridLayer };
            _host.LayerBox.Children.Add(_graphicLayer);
            _graphicLayer.Init();
            _layerList.Add(_graphicLayer);

            _lineToolLayer = new LineToolLayer { Grid = _gridLayer };
            _host.LayerBox.Children.Add(_lineToolLayer);
            _lineToolLayer.Init();
            _layerList.Add(_lineToolLayer);

            _rTreeViewLayer = new RTreeViewLayer { Grid = _gridLayer };
            _host.Layer_RTree.Children.Add(_rTreeViewLayer);
            _rTreeViewLayer.Init();
            _layerList.Add(_rTreeViewLayer);

            _polarTrackingLayer = new PolarTrackingLayer { Grid = _gridLayer };
            _host.Layer_Mark.Children.Add(_polarTrackingLayer);
            _polarTrackingLayer.Init();
            _layerList.Add(_polarTrackingLayer);

            _catchMarkLayer = new CatchMarkLayer { Grid = _gridLayer };
            _host.Layer_Mark.Children.Add(_catchMarkLayer);
            _catchMarkLayer.Init();
            _layerList.Add(_catchMarkLayer);
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
        private GridLayer? _gridLayer;
        /// <summary>图形 </summary>
        private GraphicLayer? _graphicLayer;

        #endregion

        #region 工具图层

        /// <summary>直线工具</summary>
        private LineToolLayer? _lineToolLayer;

        #endregion

        #region 可视化图层

        private RTreeViewLayer? _rTreeViewLayer;

        #endregion

        #region 标记图层

        /// <summary>极轴追踪</summary>
        private PolarTrackingLayer? _polarTrackingLayer;
        /// <summary>捕捉标记</summary>
        private CatchMarkLayer? _catchMarkLayer;

        #endregion

        /// <summary>图层列表</summary>
        private readonly List<DrawingLayer> _layerList = new List<DrawingLayer>();

        #endregion
    }
}