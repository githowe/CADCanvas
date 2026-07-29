using CADCanvas.SubSystem.DrawingSystem;
using CADCanvas.SubSystem.EditerSystem.Layer;
using CADCanvas.SubSystem.EditerSystem.Tool;
using CADCanvas.SubSystem.ResourceSystem;
using System.Windows;
using System.Windows.Controls;
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

        public DrawLineTool DrawLineTool => _drawLineTool;

        #endregion

        #region 公开方法

        public void SwitchTool(CanvasToolBase tool)
        {
            _currentTool?.Clear();
            _currentTool = tool;
            _host.Layer_Mouse.Cursor = tool.Cursor;
            _currentTool.Active();
        }

        #endregion

        #region 输入处理

        public void HandleKeyDown(KeyEventArgs e) => _currentTool.OnKeyDown(e);

        public void HandleMouseEnter() => _currentTool.OnMouseEnter();

        public void HandleMouseLeave() => _currentTool.OnMouseLeave();

        public void HandleMouseMove() => _currentTool.OnMouseMove();

        public void HandleMouseDown(MouseButton button) => _currentTool.OnMouseDown(button);

        public void HandleMouseUp(MouseButton button) => _currentTool.OnMouseUp(button);

        public void HandleMouseWheel(MouseWheelEventArgs e) => _currentTool.OnMouseWheel(e);

        #endregion

        #region 通用工具方法

        public void CaptureOperationLayer() => _host.Layer_Mouse.CaptureMouse();

        public void ReleaseOperationLayer() => _host.Layer_Mouse.ReleaseMouseCapture();

        public void OnMouseMove() { }

        public Point GetMousePoint() => Mouse.GetPosition(_host.Layer_Mouse);

        public Point GetWorldPoint() => _layerComponent.GetWorldPoint();

        public Size GetLayerSize() => _host.Layer_Mouse.RenderSize;

        public GridLayer GetGridLayer() => _layerComponent.GetGridLayer();

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
            _host.Layer_Mouse.Cursor = _currentTool.Cursor;
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

        public UserControl LoadControl(PopupType popupType, Point point)
        {
            return GetComponent<ControlComponent>().LoadControl(popupType, point);
        }

        public void MoveControl(Point point)
        {
            GetComponent<ControlComponent>().MoveControl(point);
        }

        public void SetControlSize(double width, double height)
        {
            GetComponent<ControlComponent>().SetControlSize(width, height);
        }

        public void UnloadControl(PopupType popupType)
        {
            GetComponent<ControlComponent>().UnloadControl(popupType);
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
            _layerComponent.SetLineToolStart(end);
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

        protected override void Enable()
        {
            _selectTool.Enable();
            _drawLineTool.Enable();
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

        private CanvasToolBase _currentTool;

        /// <summary>鼠标按下时的坐标</summary>
        private Point _mouseDown = new Point();
        /// <summary>鼠标按下时的世界坐标</summary>
        private Point _worldPointDown = new Point();

        private LayerComponent? _layerComponent;

        #endregion
    }
}