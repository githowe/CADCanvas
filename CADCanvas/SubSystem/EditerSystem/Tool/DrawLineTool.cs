using CADCanvas.SubSystem.EditerSystem.Component;
using XLogic.Wpf.Behavior;

namespace CADCanvas.SubSystem.EditerSystem.Tool
{
    /// <summary>
    /// 绘制直线阶段
    /// </summary>
    public enum DrawLineStage
    {
        /// <summary>选择起点</summary>
        SelectStart,
        /// <summary>选择下一点</summary>
        SelectNext,
    }

    public class DrawLineTool : CanvasToolBase
    {
        public DrawLineTool(ToolComponent host) : base(host) { }

        public override void Init()
        {
            // 移动
            NewTree(Behaviors.Move, (_) =>
            {
                ResetTree();
                switch (_stage)
                {
                    case DrawLineStage.SelectStart:
                        _host.LineTool_SelectStart();
                        break;
                    case DrawLineStage.SelectNext:
                        _host.LineTool_SelectNext();
                        break;
                }
                _host.OnMouseMove();
            });
            Finish();

            // 左键按下（设置起点） -> 松开
            NewTree("设置起点", (_) =>
            {
                _host.CaptureOperationLayer();
                _host.LineTool_SetStart();
            });
            NewNode(Behaviors.LeftUp, (_) =>
            {
                ResetTree();
                _host.ReleaseOperationLayer();
                _stage = DrawLineStage.SelectNext;
            });
            BackToRoot();
            // 左键按下（设置起点） -> 移动 -> 松开
            NewNode(Behaviors.Move, (_) =>
            {
                _host.LineTool_SelectNext();
            });
            NewNode(Behaviors.LeftUp, (_) =>
            {
                ResetTree();
                _host.ReleaseOperationLayer();
                _stage = DrawLineStage.SelectNext;
            });
            Finish();

            // 左键按下（设置下一点） -> 松开
            NewTree("设置下一点", (_) =>
            {
                _host.CaptureOperationLayer();
                _host.LineTool_SetNext();
            });
            NewNode(Behaviors.LeftUp, (_) =>
            {
                ResetTree();
                _host.ReleaseOperationLayer();
            });
            BackToRoot();
            // 左键按下（设置下一点） -> 移动 -> 松开
            NewTree(Behaviors.Move, (_) =>
            {
                _host.LineTool_SelectNext();
            });
            NewNode(Behaviors.LeftUp, (_) =>
            {
                ResetTree();
                _host.ReleaseOperationLayer();
            });
            Finish();

            // 中键按下 -> 松开
            NewTree(Behaviors.MiddleDown, (_) =>
            {
                _host.BeginDragCanvas();
            });
            NewNode(Behaviors.MiddleUp, (_) =>
            {
                ResetTree();
                _host.EndDragCanvas();
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
            });
            Finish();
        }

        public override void OnLeftButtonDown(BehaviorArgs? args = null)
        {
            switch (_stage)
            {
                case DrawLineStage.SelectStart:
                    Invoke("设置起点");
                    break;
                case DrawLineStage.SelectNext:
                    Invoke("设置下一点");
                    break;
            }
        }

        private DrawLineStage _stage = DrawLineStage.SelectStart;
    }
}