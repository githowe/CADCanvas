using CADCanvas.SubSystem.EditerSystem.Component;
using CADCanvas.SubSystem.EditerSystem.Control;
using CADCanvas.SubSystem.EditerSystem.Layer;
using CADCanvas.SubSystem.ResourceSystem;
using System.Windows;
using System.Windows.Input;
using XLogic.Wpf.Behavior;
using XLogic.Wpf.Ex;

namespace CADCanvas.SubSystem.EditerSystem.Tool
{
    /// <summary>
    /// 圆形工具阶段
    /// </summary>
    public enum CircleToolStage
    {
        /// <summary>选择圆心</summary>
        SelectCenter,
        /// <summary>选择半径</summary>
        SelectRadius
    }

    /// <summary>
    /// 圆形工具。使用圆心和半径来创建圆形
    /// </summary>
    public class CircleTool : CanvasToolBase
    {
        #region 构造方法

        public CircleTool(ToolComponent host) : base(host) { }

        #endregion

        #region 生命周期

        public override void Init()
        {
            CursorImage = ImageManager.Instance.Cursor_Draw;

            鼠标进入();
            鼠标离开();
            鼠标移动();

            设置圆心();
            设置半径();

            中键按下();

            // 监听行为树按键
            _handler.TreeRootKeyDown = HandleTreeRootKeyDown;
        }

        public override void Enable()
        {
            LayerComponent layerComponent = _host.GetComponent<LayerComponent>();
            网格图层 = layerComponent.GridLayer;
            工具图层 = layerComponent.CircleToolLayer;
            极轴追踪图层 = layerComponent.PolarTrackingLayer;
            捕捉标记图层 = layerComponent.SnapMarkLayer;
            光标图层 = layerComponent.CursorLayer;
        }

        public override void Clear()
        {
            ResetTree();
            _host.ReleaseOperationLayer();
            switch (_stage)
            {
                case CircleToolStage.SelectCenter:
                    _host.UnloadControl(PopupType.PointPopup);
                    坐标控件 = null;
                    break;
                case CircleToolStage.SelectRadius:
                    工具图层.WorldStart = null;
                    工具图层.WorldEnd = null;
                    工具图层.Clear();
                    极轴追踪图层.Clear();
                    极轴追踪图层.Reset();
                    _stage = CircleToolStage.SelectCenter;
                    break;
            }
            捕捉标记图层.SnapPointList.Clear();
            捕捉标记图层.Clear();
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
                if (_stage == CircleToolStage.SelectCenter)
                {
                    if (坐标控件 == null)
                    {
                        Point point = _host.GetMousePoint().OffsetTo(20, 20);
                        坐标控件 = (PointPopup?)_host.LoadControl(PopupType.PointPopup, point);
                        坐标控件?.UpdatePoint(_host.GetWorldPoint());
                        if (坐标控件 != null)
                            坐标控件.Loaded += (s, e) => 坐标控件.FocusX();
                    }
                }
            });
            Finish();
        }

        private void 鼠标离开()
        {
            // 鼠标离开
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
                    case CircleToolStage.SelectCenter:
                        // 使控件保持在光标右下角
                        _host.MoveControl(_host.GetMousePoint().OffsetTo(20, 20));
                        // 更新坐标值
                        Point snapedWorldPoint = _host.GetSnapWorldPoint(out bool snapped, out string snapName);
                        if (snapped) 坐标控件!.SnapPointName = snapName;
                        else 坐标控件!.SnapPointName = "";
                        坐标控件?.UpdatePoint(snapedWorldPoint);
                        // 将光标移动至捕捉点处
                        Point screenPoint = 网格图层.ToScreen(snapedWorldPoint, true);
                        光标图层.MoveCursor(screenPoint.OffsetTo(-0.5, -0.5));
                        // 全选
                        坐标控件?.SelectAll();
                        break;
                    case CircleToolStage.SelectRadius:
                        SelectRadiusEnd();
                        break;
                }
                _host.OnMouseMove();
            });
            Finish();
        }

        private void 设置圆心()
        {
            NewTree("设置圆心", (_) =>
            {
                _host.CaptureOperationLayer();
                工具图层.WorldStart = 坐标控件.Point;
                _host.UnloadControl(PopupType.PointPopup);
                坐标控件 = null;
            });
            NewNode(Behaviors.LeftUp, (_) =>
            {
                ResetTree();
                _host.ReleaseOperationLayer();
                _stage = CircleToolStage.SelectRadius;
            });
            BackToRoot();
            NewNode(Behaviors.Move, (_) =>
            {
                SelectRadiusEnd();
            });
            NewNode(Behaviors.LeftUp, (_) =>
            {
                ResetTree();
                _host.ReleaseOperationLayer();
                _stage = CircleToolStage.SelectRadius;
            });
            Finish();
        }

        private void 设置半径()
        {
            NewTree("设置半径", (_) =>
            {
                _host.CaptureOperationLayer();
                if (工具图层.WorldEnd == null) return;

                if (长度控件.输入长度 != 工具图层.原物理半径)
                {
                    工具图层.当前物理半径 = 长度控件.输入长度;
                    工具图层.MoveWorldEnd();
                }

                _host.CircleTool_SetRadiusEnd();
                长度控件?.Reset();
                极轴追踪图层.Clear();
                极轴追踪图层.Reset();
                捕捉标记图层.SnapPointList.Clear();
                捕捉标记图层.Clear();
            });
            NewNode(Behaviors.LeftUp, (_) =>
            {
                ResetTree();
                _host.ReleaseOperationLayer();
                // 重新加载坐标控件
                Point point = _host.GetMousePoint().OffsetTo(20, 20);
                坐标控件 = (PointPopup?)_host.LoadControl(PopupType.PointPopup, point);
                坐标控件?.UpdatePoint(_host.GetWorldPoint());
                if (坐标控件 != null)
                    坐标控件.Loaded += (s, e) => 坐标控件.FocusX();
                // 回到选择圆心阶段
                _stage = CircleToolStage.SelectCenter;
            });
            Finish();
        }

        private void 中键按下()
        {
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
        }

        #endregion

        #region 工具事件

        public override void OnLeftButtonDown(BehaviorArgs? args = null)
        {
            switch (_stage)
            {
                case CircleToolStage.SelectCenter:
                    Invoke("设置圆心");
                    break;
                case CircleToolStage.SelectRadius:
                    Invoke("设置半径");
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
                    case CircleToolStage.SelectCenter:
                        ResetTree();
                        _host.ReleaseOperationLayer();
                        _host.UnloadControl(PopupType.PointPopup);
                        坐标控件 = null;
                        Finished?.Invoke();
                        break;
                    case CircleToolStage.SelectRadius:
                        ResetTree();
                        _host.ReleaseOperationLayer();
                        工具图层.WorldStart = null;
                        工具图层.WorldEnd = null;
                        工具图层.Clear();
                        // 清空极轴追踪图层
                        极轴追踪图层.Clear();
                        极轴追踪图层.Reset();
                        // 重新加载坐标控件
                        Point point = _host.GetMousePoint().OffsetTo(20, 20);
                        坐标控件 = (PointPopup?)_host.LoadControl(PopupType.PointPopup, point);
                        坐标控件?.UpdatePoint(_host.GetWorldPoint());
                        if (坐标控件 != null)
                            坐标控件.Loaded += (s, e) => 坐标控件.FocusX();
                        // 回到选择圆心阶段
                        _stage = CircleToolStage.SelectCenter;
                        break;
                }
            }
            // 再交给行为树处理按键
            base.OnKeyDown(e);
            // 禁止系统处理Tab、Esc、Enter键
            if (e.Key is Key.Tab or Key.Escape or Key.Enter) e.Handled = true;
        }

        private void HandleTreeRootKeyDown(KeyEventArgs e)
        {
            // 切换输入框
            if (e.Key == Key.Tab)
            {
                if (_stage == CircleToolStage.SelectCenter)
                    坐标控件?.SwitchFocus();
            }
            // 处理回车
            else if (e.Key == Key.Enter)
            {
                switch (_stage)
                {
                    case CircleToolStage.SelectCenter:
                        // 设置起点并卸载控件
                        工具图层.WorldStart = 坐标控件.Point;
                        _host.UnloadControl(PopupType.PointPopup);
                        坐标控件 = null;
                        _stage = CircleToolStage.SelectRadius;
                        SelectRadiusEnd();
                        break;
                    case CircleToolStage.SelectRadius:
                        break;
                }
            }
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 选择半径终点
        /// </summary>
        private void SelectRadiusEnd()
        {
            if (长度控件 == null)
            {
                长度控件 = _host.LoadControl(PopupType.LengthPopup, new Point()) as LengthPopup;
                Size size = _host.GetLayerSize();
                _host.SetControlSize(size.Width, size.Height);
                长度控件.Loaded += (s, e) => 长度控件.InitFocus();
            }

            // 设置世界终点
            工具图层.WorldEnd = _host.GetSnapWorldPoint(out bool snapped, out string _);
            // 将光标移动至捕捉点处
            if (snapped)
            {
                Point screenPoint = 网格图层.ToScreen(工具图层.WorldEnd.Value, true);
                光标图层.MoveCursor(screenPoint.OffsetTo(-0.5, -0.5));
            }
            // 更新图层信息
            工具图层.UpdateInfo();
            长度控件.原长度 = 工具图层.原物理半径;
            // 获取已锁定的长度
            工具图层.当前物理半径 = 长度控件.锁定长度;

            // 先重置极轴追踪
            极轴追踪图层.Reset();
            极轴追踪图层.Clear();
            // 未吸附至捕捉点时，才使用极轴追踪
            if (!snapped)
            {
                double angle = 极轴追踪图层.UpdateTrackingAngle(工具图层.WorldStart.Value, 工具图层.WorldEnd.Value);
                极轴追踪图层.Update();
                工具图层.当前周角 = angle;
                // 如果吸附至极轴，则计算极轴与附近曲线的交点，并将终点吸附至交点
                if (极轴追踪图层.Snapped)
                {
                    工具图层.WorldEnd = _host.GetSnapToIntersectionWithPolarAxis(工具图层.WorldStart.Value, angle, out bool snappedToIntersection);
                    if (snappedToIntersection)
                    {
                        Point screenPoint = 网格图层.ToScreen(工具图层.WorldEnd.Value, true);
                        光标图层.MoveCursor(screenPoint.OffsetTo(-0.5, -0.5));
                        // 更新图层信息
                        工具图层.UpdateInfo();
                        // 设置直线信息控件的原长度与原角度
                        长度控件.原长度 = 工具图层.原物理半径;
                    }
                }
            }

            // 根据长度与角度确定世界终点
            if (工具图层.当前物理半径 != 工具图层.原物理半径 ||
                工具图层.当前周角 != 工具图层.原周角)
                工具图层.MoveWorldEnd();
            // 更新图层
            工具图层.Update();
            // 更新控件
            长度控件.UpdateLength(工具图层.LinearMid);
        }

        #endregion

        #region 字段

        private CircleToolStage _stage = CircleToolStage.SelectCenter;

        private GridLayer 网格图层;
        private CircleToolLayer 工具图层;
        private PolarTrackingLayer 极轴追踪图层;
        private SnapMarkLayer 捕捉标记图层;
        private CursorLayer 光标图层;

        private PointPopup? 坐标控件 = null;
        private LengthPopup? 长度控件 = null;

        #endregion
    }
}