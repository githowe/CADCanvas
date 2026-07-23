using CADCanvas.SubSystem.DrawingSystem;
using CADCanvas.SubSystem.EditerSystem.Tool;
using CADCanvas.SubSystem.ResourceSystem;
using System.Windows;
using System.Windows.Input;
using XLogic.Base.UI;

namespace CADCanvas.SubSystem.EditerSystem.Component
{
    /// <summary>
    /// 工具组件
    /// </summary>
    public class ToolComponent : Component<Editer>
    {
        #region 属性

        /// <summary>当前工具</summary>
        public CanvasToolBase? CurrentTool => _currentTool;

        public DrawLineTool DrawLineTool => _drawLineTool;

        #endregion

        #region 公开方法

        public void SwitchTool(CanvasToolBase tool)
        {
            _currentTool?.Clear();
            _currentTool = tool;
            _host.Layer_Mouse.Cursor = tool.Cursor;
        }

        #endregion

        #region 通用工具方法 

        public void CaptureOperationLayer() => _host.Layer_Mouse.CaptureMouse();

        public void ReleaseOperationLayer() => _host.Layer_Mouse.ReleaseMouseCapture();

        public void OnMouseMove() { }

        /// <summary>
        /// 开始拖动画布
        /// </summary>
        public void BeginDragCanvas()
        {
            _host.Layer_Mouse.Cursor = CursorManager.Instance.Move;
            _mouseDown = Mouse.GetPosition(_host.Layer_Mouse);
        }

        /// <summary>
        /// 拖动画布
        /// </summary>
        public void DragCanvas()
        {
            // 当前鼠标坐标
            Point currentPoint = Mouse.GetPosition(_host.Layer_Mouse);
            // 计算偏移
            Point offset = new Point(currentPoint.X - _mouseDown.X, currentPoint.Y - _mouseDown.Y);
            // 获取图层组件
            LayerComponent layer = GetComponent<LayerComponent>();
            // 平移网格
            layer.MoveGrid(offset);
            // 更新图形
            layer.UpdateLayerPosition();
        }

        /// <summary>
        /// 结束拖动画布
        /// </summary>
        public void EndDragCanvas()
        {
            _host.Layer_Mouse.Cursor = CurrentTool.Cursor;
            _layerComponent.ApplyMoveGrid();
        }

        /// <summary>
        /// 缩放画布
        /// </summary>
        public void ResizeCanvas(MouseWheelEventArgs e)
        {
            _layerComponent.ResizeGrid(e);
            _layerComponent.UpdateLayerPosition();
        }

        #endregion

        #region 选择工具方法

        /// <summary>
        /// 更新悬停对象
        /// </summary>
        public void UpdateHoverObject()
        {

        }

        #endregion

        #region 直线工具方法

        /// <summary>
        /// 选择起点
        /// </summary>
        public void LineTool_SelectStart()
        {
            // 吸附点
        }

        /// <summary>
        /// 设置直线起点
        /// </summary>
        public void LineTool_SetStart()
        {
            _layerComponent.SetLineToolStart();
        }

        /// <summary>
        /// 选择下一点
        /// </summary>
        public void LineTool_SelectNext()
        {
            _layerComponent.SetLineToolEnd();
            _layerComponent.UpdateLineTool();
        }

        /// <summary>
        /// 设置下一点
        /// </summary>
        public void LineTool_SetNext()
        {
            // 获取起点与终点
            Point start = _layerComponent.GetLineToolStart();
            Point end = _layerComponent.GetWorldPoint();
            // 创建直线
            VisualLine line = GeoCreator.Instance.CreateLine(start.X, start.Y, end.X, end.Y);
            // 添加直线
            _layerComponent.AddGraphic(line);
            // 更新图形
            _layerComponent.UpdateGraphic();

            // 清除直线工具并设置起点
            _layerComponent.ClearLineTool();
            _layerComponent.SetLineToolStart();
        }

        /// <summary>
        /// 取消绘制直线
        /// </summary>
        public void LineTool_Cancel()
        {
            _layerComponent.ClearLineTool();
        }

        #endregion

        #region 生命周期

        protected override void Init()
        {
            // 初始化工具
            _selectTool = new SelectTool(this);
            _drawLineTool = new DrawLineTool(this);
            _drawLineTool.Finished = OnToolFinished;
            // 初始化当前工具为选择工具
            SwitchTool(_selectTool);

            _layerComponent = GetComponent<LayerComponent>();
        }

        #endregion

        #region 私有方法

        private void OnToolFinished()
        {
            SwitchTool(_selectTool);
        }

        #endregion

        #region 字段

        private SelectTool _selectTool;
        private DrawLineTool _drawLineTool;

        private CanvasToolBase? _currentTool = null;

        /// <summary>鼠标按下时的坐标</summary>
        private Point _mouseDown = new Point();
        /// <summary>鼠标按下时的世界坐标</summary>
        private Point _worldPointDown = new Point();

        private LayerComponent? _layerComponent;

        #endregion
    }
}