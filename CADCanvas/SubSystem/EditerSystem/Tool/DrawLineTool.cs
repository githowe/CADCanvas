using CADCanvas.SubSystem.DebugSystem;
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
            CursorImage = ImageManager.Instance.Cursor_Draw;

            // 鼠标进入
            NewTree(Behaviors.Enter, (_) =>
            {
                ResetTree();
                光标图层.ShowCursor();
                光标图层.MoveCursor(_host.GetMousePoint());
                if (_stage == DrawLineStage.SelectStart)
                {
                    if (坐标信息 == null)
                    {
                        Point point = _host.GetMousePoint().OffsetTo(20, 20);
                        坐标信息 = (PointPopup?)_host.LoadControl(PopupType.PointPopup, point);
                        坐标信息?.UpdatePoint(_host.GetWorldPoint());
                        if (坐标信息 != null)
                            坐标信息.Loaded += (s, e) => 坐标信息.FocusX();
                    }
                }
            });
            Finish();

            // 鼠标离开
            NewTree(Behaviors.Leave, (_) =>
            {
                ResetTree();
                光标图层.HideCursor();
            });
            Finish();

            // 移动
            NewTree(Behaviors.Move, (_) =>
            {
                ResetTree();
                光标图层.MoveCursor(_host.GetMousePoint());
                switch (_stage)
                {
                    case DrawLineStage.SelectStart:
                        // 选择起点
                        _host.LineTool_SelectStart();
                        // 使控件保持在光标右下角
                        _host.MoveControl(_host.GetMousePoint().OffsetTo(20, 20));
                        // 更新坐标值
                        Point snapedWorldPoint = _host.GetSnapWorldPoint(out bool snapped, out string snapName);
                        if (snapped) 坐标信息!.SnapPointName = snapName;
                        else 坐标信息!.SnapPointName = "";
                        坐标信息?.UpdatePoint(snapedWorldPoint);
                        // 将光标移动至捕捉点处
                        Point screenPoint = 网格图层.ToScreen(snapedWorldPoint, true);
                        光标图层.MoveCursor(screenPoint.OffsetTo(-0.5, -0.5));
                        // 全选
                        坐标信息?.SelectAll();
                        break;
                    case DrawLineStage.SelectNext:
                        SelectNext();
                        break;
                }
                _host.OnMouseMove();
            });
            Finish();

            // 左键按下（设置起点） -> 松开
            NewTree("设置起点", (_) =>
            {
                _host.CaptureOperationLayer();
                工具图层.WorldStart = 坐标信息.Point;
                _host.UnloadControl(PopupType.PointPopup);
                坐标信息 = null;
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
                SelectNext();
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
                // 结束坐标为空，表示鼠标没有进行过移动，所以忽略此次操作
                if (工具图层.WorldEnd == null) return;

                // 长度或角度发生变化，则通过长度与角度移动终点位置
                if (直线信息.输入长度 != 工具图层.原物理长度 || 直线信息.输入角度 != 工具图层.原周角)
                {
                    工具图层.当前物理长度 = 直线信息.输入长度;
                    工具图层.当前周角 = 直线信息.输入角度;
                    工具图层.MoveWorldEnd();
                }
                _host.LineTool_SetNext();
                直线信息?.Reset();
                极轴追踪图层.Clear();
                极轴追踪图层.Reset();
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
                SelectNext();
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

        public override void Enable()
        {
            LayerComponent layerComponent = _host.GetComponent<LayerComponent>();
            网格图层 = layerComponent.GridLayer;
            工具图层 = layerComponent.LineToolLayer;
            极轴追踪图层 = layerComponent.PolarTrackingLayer;
            光标图层 = layerComponent.CursorLayer;
        }

        public override void Active()
        {
            DebugInfoManager manager = DebugInfoManager.Instance;
            manager.AddInfo("起点坐标", "终点坐标", "");
            manager.AddInfo("起点世界坐标", "终点世界坐标", "");
            manager.AddInfo("垂直偏移", "水平偏移", "");
            manager.AddInfo("终点位置", "斜边长度", "斜边角度", "旋转后角度", "");
            manager.AddInfo("斜边物理长度", "");
            manager.AddInfo("旋转后横坐标偏移", "旋转后纵坐标偏移", "");
            manager.AddInfo("极轴追踪", "");
            manager.AddInfo("捕捉点类型", "捕捉点坐标", "");
            manager.AddInfo("添加直线起点", "添加直线终点", "");
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
                        坐标信息 = null;
                        DebugInfoManager.Instance.ClearInfo();
                        Finished?.Invoke();
                        break;
                    // 选择下一点阶段：取消绘制并回到选择起点阶段
                    case DrawLineStage.SelectNext:
                        {
                            ResetTree();
                            _host.ReleaseOperationLayer();
                            CancelDraw();
                            _stage = DrawLineStage.SelectStart;
                            // 卸载直线信息控件
                            _host.UnloadControl(PopupType.DrawLineInfo);
                            直线信息 = null;
                            // 清空极轴追踪图层
                            极轴追踪图层.Clear();
                            极轴追踪图层.Reset();
                            // 重新加载坐标控件
                            Point point = _host.GetMousePoint().OffsetTo(20, 20);
                            坐标信息 = (PointPopup?)_host.LoadControl(PopupType.PointPopup, point);
                            坐标信息?.UpdatePoint(_host.GetWorldPoint());
                            if (坐标信息 != null)
                                坐标信息.Loaded += (s, e) => 坐标信息.FocusX();
                        }
                        break;
                }
            }
            // 再交给行为树处理按键
            base.OnKeyDown(e);
            // 禁止系统处理Tab、Esc键
            if (e.Key is Key.Tab or Key.Escape or Key.Enter) e.Handled = true;
        }

        private void HandleTreeRootKeyDown(KeyEventArgs e)
        {
            // 切换输入框
            if (e.Key == Key.Tab)
            {
                switch (_stage)
                {
                    case DrawLineStage.SelectStart:
                        坐标信息?.SwitchFocus();
                        break;
                    case DrawLineStage.SelectNext:
                        直线信息?.SwitchFocus();
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
                            工具图层.WorldStart = 坐标信息.Point;
                            _host.UnloadControl(PopupType.PointPopup);
                            坐标信息 = null;
                            // 进入选择下一点阶段
                            _stage = DrawLineStage.SelectNext;
                            // 更新直线
                            SelectNext();
                        }
                        break;
                    case DrawLineStage.SelectNext:
                        break;
                }
            }
        }

        #region 私有方法

        /// <summary>
        /// 选择下一点
        /// </summary>
        private void SelectNext()
        {
            // 直线信息控件为空，则加载直线信息控件
            if (直线信息 == null)
            {
                直线信息 = (LineInfoPopup?)_host.LoadControl(PopupType.DrawLineInfo, new Point());
                Size size = _host.GetLayerSize();
                _host.SetControlSize(size.Width, size.Height);
                直线信息.Loaded += (s, e) => 直线信息.InitFocus();
            }

            // 设置世界终点
            工具图层.WorldEnd = _host.GetSnapWorldPoint(out bool snapped, out string _);
            // 将光标移动至捕捉点处
            if (snapped)
            {
                Point screenPoint = 网格图层.ToScreen(工具图层.WorldEnd.Value, true);
                光标图层.MoveCursor(screenPoint.OffsetTo(-0.5, -0.5));
            }
            // 更新直线信息
            工具图层.UpdateLineInfo();
            // 设置直线信息控件的原长度与原角度
            直线信息.原长度 = 工具图层.原物理长度;
            直线信息.原角度 = 工具图层.原周角;

            // 获取已锁定的长度
            工具图层.当前物理长度 = 直线信息.锁定长度;
            // 如果角度已锁定，则使用已锁定的角度
            if (直线信息.AngleLocked) 工具图层.当前周角 = 直线信息.锁定角度;
            // 否则，使用极轴追踪的角度
            else
            {
                // 先重置极轴追踪
                极轴追踪图层.Reset();
                极轴追踪图层.Clear();
                // 未吸附至捕捉点时，才使用极轴追踪
                if (!snapped)
                {
                    double angle = 极轴追踪图层.UpdateTrackingAngle(工具图层.WorldStart.Value, 工具图层.WorldEnd.Value);
                    极轴追踪图层.Update();
                    工具图层.当前周角 = angle;
                    直线信息.原角度 = angle;
                    // 如果吸附至极轴，则计算极轴与附近曲线的交点，并将终点吸附至交点
                    if (极轴追踪图层.Snapped)
                    {
                        工具图层.WorldEnd = _host.GetSnapToIntersectionWithPolarAxis(工具图层.WorldStart.Value, angle, out bool snappedToIntersection);
                        if (snappedToIntersection)
                        {
                            Point screenPoint = 网格图层.ToScreen(工具图层.WorldEnd.Value, true);
                            光标图层.MoveCursor(screenPoint.OffsetTo(-0.5, -0.5));
                            // 更新直线信息
                            工具图层.UpdateLineInfo();
                            // 设置直线信息控件的原长度与原角度
                            直线信息.原长度 = 工具图层.原物理长度;
                            直线信息.原角度 = 工具图层.原周角;
                        }
                    }
                }
            }

            // 根据长度与角度确定世界终点
            if (工具图层.当前物理长度 != 工具图层.原物理长度 ||
                工具图层.当前周角 != 工具图层.原周角)
                工具图层.MoveWorldEnd();

            // 更新直线
            工具图层.Update();
            // 更新直线信息控件
            直线信息.UpdateLineInfo(工具图层.LineCenter, 工具图层.ArcCenter);
        }

        /// <summary>
        /// 取消绘制
        /// </summary>
        private void CancelDraw()
        {
            工具图层.WorldStart = null;
            工具图层.WorldEnd = null;
            工具图层.Clear();
        }

        #endregion

        #region 字段

        private DrawLineStage _stage = DrawLineStage.SelectStart;

        private GridLayer? 网格图层 = null;
        private LineToolLayer? 工具图层 = null;
        private PolarTrackingLayer? 极轴追踪图层 = null;
        private CursorLayer 光标图层;
        private PointPopup? 坐标信息 = null;
        private LineInfoPopup? 直线信息 = null;

        #endregion
    }
}