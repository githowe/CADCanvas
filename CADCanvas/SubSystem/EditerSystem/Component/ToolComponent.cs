using CADCanvas.SubSystem.DrawingSystem;
using CADCanvas.SubSystem.EditerSystem.Tool;
using System.Windows;
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
            _currentTool.Clear();
            _currentTool = tool;
        }

        #endregion

        #region 通用工具方法 

        public void CaptureOperationLayer() => _host.Layer_Mouse.CaptureMouse();

        public void ReleaseOperationLayer() => _host.Layer_Mouse.ReleaseMouseCapture();

        public void OnMouseMove() { }

        /// <summary>
        /// 开始拖动画布
        /// </summary>
        public void BeginDragCanvas() { }

        /// <summary>
        /// 拖动画布
        /// </summary>
        public void DragCanvas() { }

        /// <summary>
        /// 结束拖动画布
        /// </summary>
        public void EndDragCanvas() { }

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
            _selectTool = new SelectTool(this);
            _drawLineTool = new DrawLineTool(this);
            _currentTool = _selectTool;

            _drawLineTool.Finished = OnToolFinished;

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