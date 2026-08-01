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
            Cursor = CursorManager.Instance.Draw;

            // 鼠标进入
            NewTree(Behaviors.Enter, (_) =>
            {
                ResetTree();
                if (_stage == DrawLineStage.SelectStart)
                {
                    if (_pointPopup == null)
                    {
                        Point point = _host.GetMousePoint().OffsetTo(20, 20);
                        _pointPopup = (PointPopup?)_host.LoadControl(PopupType.PointPopup, point);
                        _pointPopup?.UpdatePoint(_host.GetWorldPoint());
                        if (_pointPopup != null)
                            _pointPopup.Loaded += (s, e) => _pointPopup.FocusX();
                    }
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
                        _host.MoveControl(_host.GetMousePoint().OffsetTo(20, 20));
                        // 更新坐标值
                        _pointPopup?.UpdatePoint(_host.GetWorldPoint());
                        // 全选
                        _pointPopup?.SelectAll();
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
                _layer.WorldStart = _pointPopup.Point;
                _host.UnloadControl(PopupType.PointPopup);
                _pointPopup = null;
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
                // 直接应用当前输入框中的长度与角度
                _layer.当前物理长度 = _lineInfoPopup.输入长度;
                _layer.当前周角 = _lineInfoPopup.输入角度;
                _layer.MoveWorldEnd();
                _host.LineTool_SetNext();
                _lineInfoPopup?.Reset();
                _polarLayer.Clear();
                _polarLayer.Reset();
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
            _layer = _host.GetComponent<LayerComponent>().LineToolLayer;
            _polarLayer = _host.GetComponent<LayerComponent>().PolarTrackingLayer;
        }

        public override void Active()
        {
            DebugInfoManager manager = DebugInfoManager.Instance;
            manager.AddInfo("起点坐标", "终点坐标", "");
            manager.AddInfo("垂直偏移", "水平偏移", "");
            manager.AddInfo("终点位置", "斜边长度", "斜边弧度", "斜边角度", "旋转后角度", "");
            manager.AddInfo("旋转后横坐标偏移", "旋转后纵坐标偏移", "");
            manager.AddInfo("极轴追踪", "");
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
                            _lineInfoPopup = null;
                            // 清空极轴追踪图层
                            _polarLayer.Clear();
                            _polarLayer.Reset();
                            // 重新加载坐标控件
                            Point point = _host.GetMousePoint().OffsetTo(20, 20);
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
                        _pointPopup?.SwitchFocus();
                        break;
                    case DrawLineStage.SelectNext:
                        _lineInfoPopup?.SwitchFocus();
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
                            _layer.WorldStart = _pointPopup.Point;
                            _host.UnloadControl(PopupType.PointPopup);
                            _pointPopup = null;
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
            if (_lineInfoPopup == null)
            {
                _lineInfoPopup = (LineInfoPopup?)_host.LoadControl(PopupType.DrawLineInfo, new Point());
                Size size = _host.GetLayerSize();
                _host.SetControlSize(size.Width, size.Height);
                _lineInfoPopup.Loaded += (s, e) => _lineInfoPopup.InitFocus();
            }

            // 设置世界终点，并更新直线长度与角度
            _layer.WorldEnd = _host.GetWorldPoint();
            _layer.UpdateLineInfo();
            // 设置直线信息控件的原长度与原角度
            _lineInfoPopup.原长度 = _layer.物理长度;
            _lineInfoPopup.原角度 = _layer.周角;

            // 获取已锁定的长度
            _layer.当前物理长度 = _lineInfoPopup.锁定长度;
            // 如果角度已锁定，则使用已锁定的角度
            if (_lineInfoPopup.AngleLocked) _layer.当前周角 = _lineInfoPopup.锁定角度;
            // 否则，使用极轴追踪的角度
            else
            {
                double angle = _polarLayer.UpdateTrackingAngle(_layer.WorldStart.Value, _layer.WorldEnd.Value);
                _polarLayer.Update();
                _layer.当前周角 = angle;
                _lineInfoPopup.原角度 = angle;
            }

            // 根据长度与角度确定世界终点
            if (_layer.当前物理长度 != _layer.物理长度 ||
                _layer.当前周角 != _layer.周角)
                _layer.MoveWorldEnd();

            // 更新直线
            _layer.Update();
            // 更新直线信息控件
            _lineInfoPopup.UpdateLineInfo(_layer.LineCenter, _layer.ArcCenter);
        }

        /// <summary>
        /// 取消绘制
        /// </summary>
        private void CancelDraw()
        {
            _layer.WorldStart = null;
            _layer.WorldEnd = null;
            _layer.Clear();
        }

        #endregion

        #region 字段

        private DrawLineStage _stage = DrawLineStage.SelectStart;

        private LineToolLayer? _layer = null;
        private PolarTrackingLayer? _polarLayer = null;
        private PointPopup? _pointPopup = null;
        private LineInfoPopup? _lineInfoPopup = null;

        #endregion
    }
}