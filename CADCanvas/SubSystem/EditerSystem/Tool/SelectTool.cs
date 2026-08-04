using CADCanvas.SubSystem.EditerSystem.Component;
using CADCanvas.SubSystem.EditerSystem.Layer;
using CADCanvas.SubSystem.ResourceSystem;
using XLogic.Wpf.Behavior;
using XLogic.Wpf.Tool;

namespace CADCanvas.SubSystem.EditerSystem.Tool
{
    public class SelectTool : CanvasToolBase
    {
        public SelectTool(ToolComponent host) : base(host) { }

        public override void Init()
        {
            CursorImage = ImageManager.Instance.Cursor_Select;

            // 鼠标进入
            NewTree(Behaviors.Enter, (_) =>
            {
                ResetTree();
                _layer.ShowCursor();
                _layer.MoveCursor(_host.GetMousePoint());
                _host.UpdateHoverObject();
            });
            Finish();

            // 鼠标离开
            NewTree(Behaviors.Leave, (_) =>
            {
                ResetTree();
                _layer.HideCursor();
            });
            Finish();

            // 移动
            NewTree(Behaviors.Move, (_) =>
            {
                ResetTree();
                _layer.MoveCursor(_host.GetMousePoint());
                _host.UpdateHoverObject();
            });
            Finish();

            // 中键按下 -> 松开
            NewTree(Behaviors.MiddleDown, (_) =>
            {
                _host.BeginDragCanvas();
                _host.CaptureOperationLayer();
            });
            NewNode(Behaviors.MiddleUp, (_) =>
            {
                ResetTree();
                _host.EndDragCanvas();
                _host.UpdateHoverObject();
                _host.ReleaseOperationLayer();
            });
            BackToRoot();
            // 中键按下 -> 移动 -> 松开
            NewNode(Behaviors.Move, (_) =>
            {
                _host.DragCanvas();
            });
            NewNode(Behaviors.MiddleUp, (_) =>
            {
                ResetTree();
                _host.EndDragCanvas();
                _host.UpdateHoverObject();
                _host.ReleaseOperationLayer();
            });
            Finish();

            // 滚轮
            NewTree(Behaviors.Wheel, (args) =>
            {
                ResetTree();
                _host.ResizeCanvas(((MouseWheelBehaviorArgs)args).WheelArgs);
            });
            Finish();
        }

        public override void Enable()
        {
            _layer = _host.GetComponent<LayerComponent>().CursorLayer;
        }

        private CursorLayer _layer;
    }
}