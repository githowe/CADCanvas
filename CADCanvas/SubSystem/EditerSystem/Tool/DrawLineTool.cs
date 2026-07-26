using CADCanvas.SubSystem.EditerSystem.Component;
using CADCanvas.SubSystem.EditerSystem.Control;
using CADCanvas.SubSystem.ResourceSystem;
using System.Windows;
using System.Windows.Input;
using XLogic.Wpf.Behavior;
using XLogic.Wpf.Ex;

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
            Cursor = CursorManager.Instance.Draw;

            // 鼠标进入
            NewTree(Behaviors.Enter, (_) =>
            {
                ResetTree();
                Point point = _host.GetMousePoint().OffsetPoint(20, 20);
                if (_pointPopup == null)
                {
                    _pointPopup = (PointPopup?)_host.LoadControl(PopupType.PointPopup, point);
                    _pointPopup?.UpdatePoint(_host.GetWorldPoint());
                    if (_pointPopup != null)
                        _pointPopup.Loaded += (s, e) => _pointPopup.FocusX();
                }
            });
            Finish();

            // 移动
            NewTree(Behaviors.Move, (_) =>
            {
                ResetTree();
                switch (_stage)
                {
                    case DrawLineStage.SelectStart:
                        // 选择起点
                        _host.LineTool_SelectStart();
                        // 使控件保持在光标右下角
                        _host.MoveControl(_host.GetMousePoint().OffsetPoint(20, 20));
                        // 更新坐标值
                        _pointPopup?.UpdatePoint(_host.GetWorldPoint());
                        // 全选
                        _pointPopup?.SelectAll();
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
                _host.LineTool_SetStart(_pointPopup.Point);
                _host.UnloadControl(PopupType.PointPopup);
                _pointPopup = null;
            });
            NewNode(Behaviors.LeftUp, (_) =>
            {
                ResetTree();
                _host.ReleaseOperationLayer();
                _stage = DrawLineStage.SelectNext;
                _host.LineTool_SelectNext();
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
            NewNode(Behaviors.Move, (_) =>
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
                _host.CaptureOperationLayer();
            });
            NewNode(Behaviors.MiddleUp, (_) =>
            {
                ResetTree();
                _host.EndDragCanvas();
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
                _host.ReleaseOperationLayer();
            });
            Finish();

            // 监听行为树按键
            _handler.TreeRootKeyDown = HandleTreeRootKeyDown;
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

        public override void OnKeyDown(KeyEventArgs e)
        {
            // 先自己处理按键
            if (e.Key == Key.Escape)
            {
                switch (_stage)
                {
                    // 选择起点阶段：完成工具
                    case DrawLineStage.SelectStart:
                        _host.UnloadControl(PopupType.PointPopup);
                        _pointPopup = null;
                        Finished?.Invoke();
                        break;
                    // 选择下一点阶段：取消绘制并回到选择起点阶段
                    case DrawLineStage.SelectNext:
                        {
                            ResetTree();
                            _host.ReleaseOperationLayer();
                            _host.LineTool_Cancel();
                            _stage = DrawLineStage.SelectStart;
                            // 重新加载控件
                            Point point = _host.GetMousePoint().OffsetPoint(20, 20);
                            _pointPopup = (PointPopup?)_host.LoadControl(PopupType.PointPopup, point);
                            _pointPopup?.UpdatePoint(_host.GetWorldPoint());
                            if (_pointPopup != null)
                                _pointPopup.Loaded += (s, e) => _pointPopup.FocusX();
                        }
                        break;
                }
            }
            // 再交给行为树处理按键
            base.OnKeyDown(e);
            // 禁止系统处理Tab、Esc键
            if (e.Key is Key.Tab or Key.Escape) e.Handled = true;
        }

        private void HandleTreeRootKeyDown(KeyEventArgs e)
        {
            // 切换输入框
            if (e.Key == Key.Tab)
            {
                switch (_stage)
                {
                    case DrawLineStage.SelectStart:
                        _pointPopup?.SwitchFocus();
                        break;
                    case DrawLineStage.SelectNext:
                        break;
                }
            }
            // 处理回车
            else if (e.Key == Key.Enter)
            {
                switch (_stage)
                {
                    case DrawLineStage.SelectStart:
                        {
                            // 设置起点并卸载控件
                            _host.LineTool_SetStart(_pointPopup.Point);
                            _host.UnloadControl(PopupType.PointPopup);
                            _pointPopup = null;
                            // 进入选择下一点阶段
                            _stage = DrawLineStage.SelectNext;
                            // 更新直线
                            _host.LineTool_SelectNext();
                        }
                        break;
                    case DrawLineStage.SelectNext:
                        break;
                }
            }
        }

        #region 字段

        private DrawLineStage _stage = DrawLineStage.SelectStart;

        private PointPopup? _pointPopup = null;

        #endregion
    }
}