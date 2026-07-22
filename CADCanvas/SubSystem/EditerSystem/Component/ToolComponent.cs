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

        }

        /// <summary>
        /// 设置直线起点
        /// </summary>
        public void LineTool_SetStart()
        {

        }

        /// <summary>
        /// 选择下一点
        /// </summary>
        public void LineTool_SelectNext()
        {

        }

        /// <summary>
        /// 设置下一点
        /// </summary>
        public void LineTool_SetNext()
        {

        }

        #endregion

        #region 生命周期

        protected override void Init()
        {
            _selectTool = new SelectTool(this);
            _drawLineTool = new DrawLineTool(this);
            _currentTool = _selectTool;
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

        #endregion
    }
}