using CADCanvas.SubSystem.DebugSystem;
using CADCanvas.SubSystem.DrawingSystem;
using CADCanvas.SubSystem.EditerSystem.Component;
using CADCanvas.SubSystem.EditerSystem.Component.Tool;
using CADCanvas.SubSystem.EditerSystem.Layer;
using CADCanvas.SubSystem.ResourceSystem;
using System.Windows;
using System.Windows.Input;
using XLogic.Wpf;
using XLogic.Wpf.Behavior;

namespace CADCanvas.SubSystem.EditerSystem.Tool
{
    /// <summary>
    /// 修剪工具
    /// </summary>
    public class TrimTool : CanvasToolBase
    {
        #region 构造方法

        public TrimTool(ToolComponent host) : base(host) { }

        #endregion

        #region 生命周期

        public override void Init()
        {
            CursorImage = ImageManager.Instance.Cursor_Trim;

            鼠标进入();
            鼠标离开();
            鼠标移动();

            命中空白();
            命中对象();
            中键按下();

            _handler.TreeRootKeyDown = HandleTreeRootKeyDown;
            _hitedVisualTracker.OnAdd = OnHitedAdd;
            _hitedVisualTracker.OnRemove = OnHitedRemove;
        }

        public override void Enable()
        {
            LayerComponent layerComponent = _host.GetComponent<LayerComponent>();
            网格图层 = layerComponent.GridLayer;
            图形图层 = layerComponent.GraphicLayer;
            工具图层 = layerComponent.TrimToolLayer;
            光标图层 = layerComponent.CursorLayer;
            _scene = _host.GetComponent<SceneComponent>();
        }

        public override void Clear()
        {
            ResetTree();
            _host.ReleaseOperationLayer();
            DebugInfoManager.Instance.ClearInfo();
        }

        public override void Active()
        {
            DebugInfoManager.Instance.AddInfo("命中对象数量", "");
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
                UpdateHitedVisual();
                _host.OnMouseMove();
            });
            Finish();
        }

        private void 命中空白()
        {
            NewTree("命中空白", (_) =>
            {

            });
            NewNode(Behaviors.LeftUp, (_) =>
            {
                ResetTree();
            });
            Finish();
        }

        private void 命中对象()
        {
            NewTree("命中对象", (_) =>
            {
                TrimHitedVisual();
            });
            NewNode(Behaviors.LeftUp, (_) =>
            {
                ResetTree();
            });
            Finish();
        }

        private void 中键按下()
        {

        }

        #endregion

        #region 工具事件

        public override void OnLeftButtonDown(BehaviorArgs? args = null)
        {
            if (工具图层.GeoVisualList.Count == 0) Invoke("命中空白");
            else Invoke("命中对象");
        }

        public override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (e.Key is Key.Tab or Key.Escape or Key.Enter) e.Handled = true;
        }

        private void HandleTreeRootKeyDown(KeyEventArgs e)
        {

        }

        #endregion

        #region 私有方法

        private void OnHitedAdd(List<GeoVisual> list)
        {
            // 遍历新赠命中对象
            foreach (var visual in list)
            {
                visual.Hidden = true;
                // 获取与命中对象相交的对象
                List<GeoVisual> intersectList = _scene.GetintersectVisual(visual);
                // 获取全部与命中对象相交的交点
                List<Point> allIntersectPoint = new List<Point>();
                foreach (var intersectItem in intersectList)
                {
                    List<Point> intersectPoints = GeoTool.Instance.GetIntersection(visual, intersectItem);
                    allIntersectPoint.AddRange(intersectPoints);
                }
                // 生成按交点分割后的图形对象
                List<GeoVisual> split = visual.SplitByIntersectionPoint(allIntersectPoint);
                _splitVisualCache.Add(visual, split);
                工具图层.GeoVisualList.AddRange(split);
            }
            图形图层.Update();
        }

        private void OnHitedRemove(List<GeoVisual> list)
        {
            foreach (var visual in list)
            {
                visual.Hidden = false;
                // 释放分割对象
                if (_splitVisualCache.TryGetValue(visual, out List<GeoVisual>? splitList))
                {
                    foreach (var splitItem in splitList)
                    {
                        // 释放句柄
                        GeoTool.Instance.FreeCurve(splitItem);
                        // 从工具图层中移除
                        工具图层.GeoVisualList.Remove(splitItem);
                    }
                    // 清除缓存
                    _splitVisualCache.Remove(visual);
                }
            }
            图形图层.Update();
        }

        /// <summary>
        /// 更新命中对象
        /// </summary>
        private void UpdateHitedVisual()
        {
            // 获取鼠标坐标
            Point mousePoint = _host.GetMousePoint();
            Point mouseWorldPoint = 网格图层.ToWorld(mousePoint);
            // 以鼠标为中心，创建一个矩形区域
            Rect rectForHitBound = PointTool.CreateRect(mousePoint, 48);
            Rect rectForHitVisual = PointTool.CreateRect(mousePoint, 14);
            // 更新命中包围盒
            _scene.UpdateHitedBounds(网格图层.ToWorld(rectForHitBound));
            // 更新命中对象
            _rectForHitVisual = 网格图层.ToWorld(rectForHitVisual);
            _scene.UpdateHitedVisual(_rectForHitVisual);
            _hitedVisualTracker.UpdateList(new List<GeoVisual>(_scene.HoveredVisual));
            // 更新修剪预览
            UpdateTrimPreview();

            DebugInfoManager.Instance.UpdateInfo("命中对象数量", _scene.HoveredVisual.Count.ToString());
        }

        /// <summary>
        /// 更新修剪预览
        /// </summary>
        private void UpdateTrimPreview()
        {
            foreach (var item in 工具图层.GeoVisualList)
            {
                if (GeoTool.Instance.IsIntersection(item, _rectForHitVisual))
                    item.Opacity = 0.2;
                else
                    item.Opacity = 1;
            }
            工具图层.Update();
        }

        /// <summary>
        /// 修剪命中对象
        /// </summary>
        private void TrimHitedVisual()
        {
            // 遍历命中对象
            foreach (var visual in _scene.HoveredVisual)
            {
                // 删除命中对象
                图形图层.GeoVisualList.Remove(visual);
                _scene.RemoveVisual(visual);
                // 获取与对象关联的分割对象
                List<GeoVisual> splitList = _splitVisualCache[visual];
                // 删除工具图层中的命中部分
                List<GeoVisual> 剩余部分 = RemoveHitedSplit(splitList);
                // 拼接剩余部分
                List<GeoVisual> 拼接结果 = visual.JointSplitVisual(剩余部分);
                // 添加拼接后的对象
                _scene.AddVisual(拼接结果);
                图形图层.GeoVisualList.AddRange(拼接结果);
                _splitVisualCache.Remove(visual);
            }
            // 清空工具图层中的分割对象
            工具图层.GeoVisualList.Clear();
            工具图层.Update();
            // 更新图形图层
            图形图层.Update();
            // 更新命中对象
            UpdateHitedVisual();
        }

        private List<GeoVisual> RemoveHitedSplit(List<GeoVisual> source)
        {
            List<GeoVisual> result = new List<GeoVisual>();

            // 添加需要保留的部分
            foreach (var item in source)
                if (item.Opacity == 1) result.Add(item);
            // 释放全部句柄
            foreach (var item in source)
                GeoTool.Instance.FreeCurve(item);

            return result;
        }

        #endregion

        #region 字段

        private GridLayer 网格图层;
        private GraphicLayer 图形图层;
        private TrimToolLayer 工具图层;
        private CursorLayer 光标图层;

        private SceneComponent _scene;

        private Rect _rectForHitVisual = Rect.Empty;

        private readonly ListChangeTracker<GeoVisual> _hitedVisualTracker = new ListChangeTracker<GeoVisual>();
        private readonly Dictionary<GeoVisual, List<GeoVisual>> _splitVisualCache = new Dictionary<GeoVisual, List<GeoVisual>>();

        #endregion
    }
}