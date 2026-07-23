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
        #region 公开方法

        public void UpdateGrid()
        {
            _gridLayer.Width = _host.LayerBox.ActualWidth;
            _gridLayer.Height = _host.LayerBox.ActualHeight;
            _gridLayer.Update();
        }

        public void UpdateAll()
        {
            foreach (var item in _layerList)
            {
                item.Width = _host.LayerBox.ActualWidth;
                item.Height = _host.LayerBox.ActualHeight;
                item.Update();
            }
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
        /// 获取直线工具起点
        /// </summary>
        public Point GetLineToolStart() => _lineToolLayer.StartPoint.Value;

        /// <summary>
        /// 设置直线工具起点
        /// </summary>
        public void SetLineToolStart()
        {
            _lineToolLayer.StartPoint = GetWorldPoint();
        }

        /// <summary>
        /// 清空直线工具
        /// </summary>
        public void ClearLineTool()
        {
            _lineToolLayer.StartPoint = null;
            _lineToolLayer.EndPoint = null;
            _lineToolLayer.Clear();
        }

        /// <summary>
        /// 设置直线工具终点
        /// </summary>
        public void SetLineToolEnd()
        {
            _lineToolLayer.EndPoint = GetWorldPoint();
        }

        public void UpdateLineTool()
        {
            _lineToolLayer.Update();
        }

        #endregion

        public Point GetScreenPoint() => Mouse.GetPosition(_host.Layer_Mouse);

        /// <summary>
        /// 获取当前鼠标的世界坐标
        /// </summary>
        public Point GetWorldPoint() => _gridLayer.ToWorld(Mouse.GetPosition(_host.Layer_Mouse));

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
        }

        #endregion

        #region 字段

        private GridLayer? _gridLayer;
        private GraphicLayer? _graphicLayer;

        private LineToolLayer? _lineToolLayer;

        private readonly List<DrawingLayer> _layerList = new List<DrawingLayer>();

        #endregion
    }
}