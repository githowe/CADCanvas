using CADCanvas.SubSystem.EditerSystem.Component;
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
            Cursor = CursorManager.Instance.Select;

            // 移动
            NewTree(Behaviors.Move, (_) =>
            {
                ResetTree();
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
    }
}