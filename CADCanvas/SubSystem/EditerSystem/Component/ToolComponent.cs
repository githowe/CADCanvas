using CADCanvas.SubSystem.DebugSystem;
using CADCanvas.SubSystem.DrawingSystem;
using CADCanvas.SubSystem.EditerSystem.Component.Tool.Snap;
using CADCanvas.SubSystem.EditerSystem.Tool;
using CADCanvas.SubSystem.ResourceSystem;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using XLogic.Base.UI;
using XLogic.Wpf.Ex;

namespace CADCanvas.SubSystem.EditerSystem.Component
{
    /// <summary>
    /// 工具组件
    /// </summary>
    public class ToolComponent : Component<Editer>
    {
        #region 属性

        public DrawLineTool DrawLineTool => _drawLineTool;

        #endregion

        #region 公开方法

        public void SwitchTool(CanvasToolBase tool)
        {
            _currentTool?.Clear();
            _currentTool = tool;
            _layerComponent!.CursorLayer.SwitchCursor(tool.CursorImage!);
            _currentTool.Active();
        }

        #endregion

        #region 输入处理

        public void HandleKeyDown(KeyEventArgs e) => _currentTool.OnKeyDown(e);

        public void HandleMouseEnter() => _currentTool.OnMouseEnter();

        public void HandleMouseLeave() => _currentTool.OnMouseLeave();

        public void HandleMouseMove() => _currentTool.OnMouseMove();

        public void HandleMouseDown(MouseButton button) => _currentTool.OnMouseDown(button);

        public void HandleMouseUp(MouseButton button) => _currentTool.OnMouseUp(button);

        public void HandleMouseWheel(MouseWheelEventArgs e) => _currentTool.OnMouseWheel(e);

        #endregion

        #region 通用工具方法



        public void CaptureOperationLayer() => _host.Layer_Mouse.CaptureMouse();

        public void ReleaseOperationLayer() => _host.Layer_Mouse.ReleaseMouseCapture();

        public void OnMouseMove() { }

        public Point GetMousePoint() => Mouse.GetPosition(_host.Layer_Mouse);

        public Point GetWorldPoint() => _layerComponent!.GetWorldPoint();

        /// <summary>
        /// 获取吸附至捕捉点的世界坐标，如果没有吸附至捕捉点，则返回当前鼠标的世界坐标
        /// </summary>
        public Point GetSnapWorldPoint(out bool snapped, out string snapName)
        {
            snapped = false;
            snapName = "";
            SceneComponent scene = GetComponent<SceneComponent>();
            DebugInfoManager manager = DebugInfoManager.Instance;

            // 获取鼠标坐标
            Point mousePoint = GetMousePoint();
            Point mouseWorldPoint = _layerComponent.GetWorldPoint(mousePoint);
            // 以鼠标为中心点，创建一个小矩形区域
            Rect rect = new Rect(mousePoint.X - 24, mousePoint.Y - 24, 48, 48);
            // 转换为世界坐标
            Point leftTop = _layerComponent.GetWorldPoint(rect.TopLeft);
            Point rightBottom = _layerComponent.GetWorldPoint(rect.BottomRight);
            Rect worldRect = new Rect(leftTop, rightBottom);
            // 更新命中包围盒
            scene.UpdateHitedBounds(worldRect);
            // 更新附近捕捉点
            scene.UpdateSnapPoint(mousePoint);
            // 有捕捉点
            if (scene.SnapPointList.Count > 0)
            {
                // 获取第一个捕捉点
                SnapPoint snapPoint = scene.SnapPointList[0];
                // 获取捕捉点的屏幕坐标
                Point snapScreenPoint = _layerComponent.GetScreenPoint(snapPoint.WorldPoint);
                // 捕捉点屏幕区域
                Rect snapRect = new Rect(snapScreenPoint.X - 8, snapScreenPoint.Y - 8, 16, 16);
                // 如果鼠标在捕捉点屏幕区域内，则返回捕捉点的世界坐标
                if (snapRect.Contains(mousePoint))
                {
                    snapped = true;
                    snapName = snapPoint.TypeName;
                    manager.UpdateInfo("捕捉点类型", snapPoint.TypeName);
                    manager.UpdateInfo("捕捉点坐标", snapPoint.WorldPoint.ToPointString("G17"));
                    return snapPoint.WorldPoint;
                }
                // 否则，返回当前鼠标的世界坐标
                manager.UpdateInfo("捕捉点类型", "无");
                manager.UpdateInfo("捕捉点坐标", "");
                return mouseWorldPoint;
            }
            // 无捕捉点，返回当前鼠标的世界坐标
            manager.UpdateInfo("捕捉点类型", "无");
            manager.UpdateInfo("捕捉点坐标", "");
            return mouseWorldPoint;
        }

        /// <summary>
        /// 获取吸附至附近图形与极轴的交点
        /// </summary>
        public Point GetSnapToIntersectionWithPolarAxis(Point worldStart, double angle, out bool snapped)
        {
            snapped = false;
            SceneComponent scene = GetComponent<SceneComponent>();

            // 获取鼠标坐标
            Point mousePoint = GetMousePoint();
            Point mouseWorldPoint = _layerComponent.GetWorldPoint(mousePoint);
            // 以鼠标为中心点，创建一个小矩形区域
            Rect rect = new Rect(mousePoint.X - 24, mousePoint.Y - 24, 48, 48);
            // 转换为世界坐标
            Point leftTop = _layerComponent.GetWorldPoint(rect.TopLeft);
            Point rightBottom = _layerComponent.GetWorldPoint(rect.BottomRight);
            Rect worldRect = new Rect(leftTop, rightBottom);
            // 更新命中包围盒
            scene.UpdateHitedBounds(worldRect);
            // 创建捕捉点列表
            List<SnapPoint> snapPointList = new List<SnapPoint>();
            // 遍历命中图形，获取与极轴的交点
            foreach (GeoVisual visual in scene.HoveredVisual)
            {
                List<Point> intersectionPoints = GeoTool.Instance.GetIntersection(visual, worldStart, angle);
                foreach (Point intersection in intersectionPoints)
                {
                    SnapPoint snapPoint = new SnapPoint
                    {
                        Type = SnapType.Intersection,
                        WorldPoint = intersection
                    };
                    snapPointList.Add(snapPoint);
                }
            }
            // 获取鼠标范围内的第一个捕捉点
            snapPointList = scene.UpdateSnapPoint(snapPointList, mousePoint);
            // 有捕捉点
            if (snapPointList.Count > 0)
            {
                // 获取第一个捕捉点
                SnapPoint snapPoint = snapPointList[0];
                // 获取捕捉点的屏幕坐标
                Point snapScreenPoint = _layerComponent.GetScreenPoint(snapPoint.WorldPoint);
                // 捕捉点屏幕区域
                Rect snapRect = new Rect(snapScreenPoint.X - 8, snapScreenPoint.Y - 8, 16, 16);
                // 如果鼠标在捕捉点屏幕区域内，则返回捕捉点的世界坐标
                if (snapRect.Contains(mousePoint))
                {
                    snapped = true;
                    return snapPoint.WorldPoint;
                }
                // 否则，返回当前鼠标的世界坐标
                return mouseWorldPoint;
            }
            // 无捕捉点，返回当前鼠标的世界坐标
            return mouseWorldPoint;
        }

        public Size GetLayerSize() => _host.Layer_Mouse.RenderSize;

        /// <summary>
        /// 开始拖动画布
        /// </summary>
        public void BeginDragCanvas()
        {
            _layerComponent!.CursorLayer.SwitchCursor(ImageManager.Instance.Cursor_Move);
            _mouseDown = Mouse.GetPosition(_host.Layer_Mouse);
        }

        /// <summary>
        /// 拖动画布
        /// </summary>
        public void DragCanvas()
        {
            // 当前鼠标坐标
            Point currentPoint = Mouse.GetPosition(_host.Layer_Mouse);
            _layerComponent!.CursorLayer.MoveCursor(currentPoint);
            // 计算偏移
            Point offset = new Point(currentPoint.X - _mouseDown.X, currentPoint.Y - _mouseDown.Y);
            // 获取图层组件
            LayerComponent layer = GetComponent<LayerComponent>();
            // 平移网格
            layer.MoveGrid(offset);
            // 更新图形
            layer.UpdateLayerPosition();
        }

        /// <summary>
        /// 结束拖动画布
        /// </summary>
        public void EndDragCanvas()
        {
            _layerComponent!.CursorLayer.SwitchCursor(_currentTool.CursorImage);
            _layerComponent.ApplyMoveGrid();
        }

        /// <summary>
        /// 缩放画布
        /// </summary>
        public void ResizeCanvas(MouseWheelEventArgs e)
        {
            _layerComponent!.ResizeGrid(e);
            _layerComponent.UpdateLayerPosition();
        }

        public UserControl LoadControl(PopupType popupType, Point point)
        {
            return GetComponent<ControlComponent>().LoadControl(popupType, point);
        }

        public void MoveControl(Point point)
        {
            GetComponent<ControlComponent>().MoveControl(point);
        }

        public void SetControlSize(double width, double height)
        {
            GetComponent<ControlComponent>().SetControlSize(width, height);
        }

        public void UnloadControl(PopupType popupType)
        {
            GetComponent<ControlComponent>().UnloadControl(popupType);
        }

        #endregion

        #region 选择工具方法

        /// <summary>
        /// 更新悬停对象
        /// </summary>
        public void UpdateHoverObject()
        {
            // 获取鼠标坐标
            Point mousePoint = GetMousePoint();
            // 以鼠标为中心点，创建一个小矩形区域
            Rect rect = new Rect(mousePoint.X - 24, mousePoint.Y - 24, 48, 48);
            // 转换为世界坐标
            Point leftTop = _layerComponent!.GetWorldPoint(rect.TopLeft);
            Point rightBottom = _layerComponent.GetWorldPoint(rect.BottomRight);
            Rect worldRect = new Rect(leftTop, rightBottom);
            // 更新命中包围盒
            GetComponent<SceneComponent>().UpdateHitedBounds(worldRect);
        }

        #endregion

        #region 直线工具方法

        /// <summary>
        /// 选择起点
        /// </summary>
        public void LineTool_SelectStart()
        {

        }

        /// <summary>
        /// 设置下一点
        /// </summary>
        public void LineTool_SetNext()
        {
            // 获取起点与终点
            Point start = _layerComponent!.GetLineToolWorldStart();
            Point end = _layerComponent.GetLineToolWorldEnd();
            // 两点相同则不创建直线段
            if (start == end) return;

            DebugInfoManager.Instance.UpdateInfo("添加直线起点", start.ToPointString("G17"));
            DebugInfoManager.Instance.UpdateInfo("添加直线终点", end.ToPointString("G17"));

            // 创建直线段
            VisualLineSegment line = GeoCreator.Instance.CreateLineSegment(start.X, start.Y, end.X, end.Y);
            // 添加直线段
            _layerComponent.AddGraphic(line);
            // 更新图形
            _layerComponent.UpdateGraphic();

            // 清除直线工具并设置起点
            _layerComponent.ClearLineTool();
            _layerComponent.SetLineToolStart(end);

            GetComponent<SceneComponent>().AddVisual(line);
        }

        #endregion

        #region 生命周期

        protected override void Init()
        {
            // 初始化工具
            _selectTool = new SelectTool(this);
            _drawLineTool = new DrawLineTool(this);
            _drawLineTool.Finished = OnToolFinished;

            _layerComponent = GetComponent<LayerComponent>();
        }

        protected override void Enable()
        {
            // 初始化当前工具为选择工具
            SwitchTool(_selectTool);

            _selectTool.Enable();
            _drawLineTool.Enable();
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

        private CanvasToolBase _currentTool;

        /// <summary>鼠标按下时的坐标</summary>
        private Point _mouseDown = new Point();
        /// <summary>鼠标按下时的世界坐标</summary>
        private Point _worldPointDown = new Point();

        private LayerComponent? _layerComponent;

        #endregion
    }
}