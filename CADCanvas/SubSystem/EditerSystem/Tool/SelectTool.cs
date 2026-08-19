using CADCanvas.SubSystem.EditerSystem.Component;
using CADCanvas.SubSystem.EditerSystem.Layer;
using CADCanvas.SubSystem.ResourceSystem;
using System.Windows.Input;
using XLogic.Wpf.Behavior;
using XLogic.Wpf.Tool;

namespace CADCanvas.SubSystem.EditerSystem.Tool
{
    public enum SelectState
    {
        SelectStart,
        SelectEnd,
    }

    public class SelectTool : CanvasToolBase
    {
        public SelectTool(ToolComponent host) : base(host) { }

        #region 生命周期

        public override void Init()
        {
            CursorImage = ImageManager.Instance.Cursor_Select;

            鼠标进入();
            鼠标离开();
            鼠标移动();

            选择起点();
            选择终点();
            命中对象();
            中键按下();

            滚轮();
        }

        public override void Enable()
        {
            工具图层 = _host.GetComponent<LayerComponent>().SelectToolLayer;
            光标图层 = _host.GetComponent<LayerComponent>().CursorLayer;
        }

        public override void Clear()
        {

        }

        #endregion

        #region 行为

        private void 鼠标进入()
        {
            NewTree(Behaviors.Enter, (_) =>
            {
                ResetTree();
                光标图层.ShowCursor();
                光标图层.MoveCursor(_host.GetMousePoint());
                _host.UpdateHoverObject();
            });
            Finish();
        }

        private void 鼠标离开()
        {
            NewTree(Behaviors.Leave, (_) =>
            {
                ResetTree();
                光标图层.HideCursor();
            });
            Finish();
        }

        private void 鼠标移动()
        {
            NewTree(Behaviors.Move, (_) =>
            {
                ResetTree();
                光标图层.MoveCursor(_host.GetMousePoint());
                switch (_stage)
                {
                    case SelectState.SelectStart:
                        _host.UpdateHoverObject();
                        break;
                    case SelectState.SelectEnd:
                        工具图层.SelectEnd = _host.GetWorldPoint();
                        工具图层.Update();
                        break;
                }
            });
            Finish();
        }

        private void 选择起点()
        {
            NewTree("选择起点", (_) =>
            {
                _host.CaptureOperationLayer();
                工具图层.SelectStart = _host.GetWorldPoint();
                光标图层.SwitchCursor(ImageManager.Instance.Cursor_Draw);
            });
            NewNode(Behaviors.LeftUp, (_) =>
            {
                ResetTree();
                _host.ReleaseOperationLayer();
                _stage = SelectState.SelectEnd;
            });
            BackToRoot();
            NewNode(Behaviors.Move, (_) =>
            {
                光标图层.MoveCursor(_host.GetMousePoint());
                工具图层.SelectEnd = _host.GetWorldPoint();
                工具图层.Update();
            });
            NewNode(Behaviors.LeftUp, (_) =>
            {
                ResetTree();
                _host.ReleaseOperationLayer();
                工具图层.SelectStart = null;
                工具图层.SelectEnd = null;
                工具图层.Clear();
                光标图层.SwitchCursor(ImageManager.Instance.Cursor_Select);
            });
            Finish();
        }

        private void 选择终点()
        {
            NewTree("选择终点", (_) =>
            {
                工具图层.SelectStart = null;
                工具图层.SelectEnd = null;
                工具图层.Clear();
                光标图层.SwitchCursor(ImageManager.Instance.Cursor_Select);
            });
            NewNode(Behaviors.LeftUp, (_) =>
            {
                ResetTree();
                _stage = SelectState.SelectStart;
            });
            BackToRoot();
            NewNode(Behaviors.Move, (_) =>
            {
                光标图层.MoveCursor(_host.GetMousePoint());
            });
            NewNode(Behaviors.LeftUp, (_) =>
            {
                ResetTree();
                _stage = SelectState.SelectStart;
            });
            Finish();
        }

        private void 命中对象()
        {

        }

        private void 中键按下()
        {
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
                if (_stage == SelectState.SelectEnd)
                    光标图层.SwitchCursor(ImageManager.Instance.Cursor_Draw);
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
                if (_stage == SelectState.SelectEnd)
                    光标图层.SwitchCursor(ImageManager.Instance.Cursor_Draw);
                _host.UpdateHoverObject();
                _host.ReleaseOperationLayer();
            });
            Finish();
        }

        private void 滚轮()
        {
            NewTree(Behaviors.Wheel, (args) =>
            {
                ResetTree();
                _host.ResizeCanvas(((MouseWheelBehaviorArgs)args).WheelArgs);
            });
            Finish();
        }

        #endregion

        #region 工具事件

        public override void OnLeftButtonDown(BehaviorArgs? args = null)
        {
            switch (_stage)
            {
                case SelectState.SelectStart:
                    if (_host.HoveredVisual()) Invoke("命中对象");
                    else Invoke("选择起点");
                    break;
                case SelectState.SelectEnd:
                    Invoke("选择终点");
                    break;
            }
        }

        public override void OnKeyDown(KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                if (_stage == SelectState.SelectEnd)
                {
                    ResetTree();
                    _host.ReleaseOperationLayer();
                    工具图层.SelectStart = null;
                    工具图层.SelectEnd = null;
                    工具图层.Clear();
                    光标图层.SwitchCursor(ImageManager.Instance.Cursor_Select);
                    _stage = SelectState.SelectStart;
                }
            }
            // 禁止系统处理Tab、Esc、Enter键
            if (e.Key is Key.Tab or Key.Escape or Key.Enter) e.Handled = true;
        }

        #endregion

        #region 字段

        private SelectState _stage = SelectState.SelectStart;

        private SelectToolLayer 工具图层;
        private CursorLayer 光标图层;

        #endregion
    }
}