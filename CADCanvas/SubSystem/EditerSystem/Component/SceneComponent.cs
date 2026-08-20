using CADCanvas.SubSystem.DrawingSystem;
using CADCanvas.SubSystem.EditerSystem.Component.Tool;
using CADCanvas.SubSystem.EditerSystem.Component.Tool.RTree;
using CADCanvas.SubSystem.EditerSystem.Component.Tool.Snap;
using CADCanvas.SubSystem.EditerSystem.Layer;
using System.Windows;
using XLogic.Base.UI;
using XLogic.Wpf.Ex;

namespace CADCanvas.SubSystem.EditerSystem.Component
{
    /// <summary>
    /// 场景组件：管理场景中的对象
    /// </summary>
    public class SceneComponent : Component<Editer>
    {
        #region 属性

        public List<GeoVisual> AllVisual => _visualList;

        public List<GeoVisual> HoveredVisualBounds => _hoveredBoundsList;

        public List<GeoVisual> HoveredVisual => _hoveredList;

        public List<SnapPoint> SnapPointList { get; set; } = new List<SnapPoint>();

        #endregion

        #region 公开方法

        public void AddVisual(GeoVisual visual)
        {
            _visualList.Add(visual);
            _tree.Insert(visual, visual.Bounds);
            _tree.UpdateBoundsLayer();
            UpdateBoundsLayerView();
        }

        public void AddVisual(List<GeoVisual> visualList)
        {
            _visualList.AddRange(visualList);
            foreach (var item in visualList)
                _tree.Insert(item, item.Bounds);
            _tree.UpdateBoundsLayer();
            UpdateBoundsLayerView();
        }

        public void RemoveVisual(GeoVisual visual)
        {
            GeoTool.Instance.FreeCurve(visual);
            _visualList.Remove(visual);
            _tree.Clear();
            _tree.Build(_visualList.Cast<IBox>().ToList());
            _tree.UpdateBoundsLayer();
            UpdateBoundsLayerView();
        }

        public void RemoveVisual(List<GeoVisual> visualList)
        {
            foreach (var item in visualList)
            {
                GeoTool.Instance.FreeCurve(item);
                _visualList.Remove(item);
            }
            _tree.Clear();
            _tree.Build(_visualList.Cast<IBox>().ToList());
            _tree.UpdateBoundsLayer();
            UpdateBoundsLayerView();
        }

        public void ClearVisual()
        {
            foreach (var item in _visualList) GeoTool.Instance.FreeCurve(item);
            _visualList.Clear();
            _tree.Clear();
            _tree.UpdateBoundsLayer();
            UpdateBoundsLayerView();
        }

        public void UpdateBoundsLayerView()
        {
            _rtViewLayer.LayerList = _tree.LayerList;
            _rtViewLayer.Update();
        }

        /// <summary>
        /// 更新命中包围盒。用于粗筛鼠标附近的图形对象
        /// </summary>
        public void UpdateHitedBounds(Rect rect)
        {
            IReadOnlyList<IBox> boundsList = _tree.Find(rect);
            _hoveredBoundsList.Clear();
            _rtViewLayer.HoveredList.Clear();
            foreach (var item in boundsList)
            {
                _hoveredBoundsList.Add((GeoVisual)item);
                _rtViewLayer.HoveredList.Add(item.Bounds);
            }
            _rtViewLayer.Update();
        }

        /// <summary>
        /// 更新命中对象。从命中包围盒中精筛鼠标附近的图形对象
        /// </summary>
        public void UpdateHitedVisual(Rect rect)
        {
            _hoveredList.Clear();
            foreach (var item in _hoveredBoundsList)
            {
                if (GeoTool.Instance.IsIntersection(item, rect))
                    _hoveredList.Add(item);
            }
        }

        /// <summary>
        /// 根据鼠标，获取附近的捕捉点，并绘制捕捉点
        /// </summary>
        public void UpdateSnapPoint(Point mousePoint)
        {
            Rect rect = new Rect(mousePoint.X - 24, mousePoint.Y - 24, 48, 48);
            Point leftTop = GetComponent<LayerComponent>().GetWorldPoint(rect.TopLeft);
            Point rightBottom = GetComponent<LayerComponent>().GetWorldPoint(rect.BottomRight);
            Rect worldRect = new Rect(leftTop, rightBottom);

            List<SnapPoint> allSnap = SnapPicker.PickSnapPoint(_hoveredBoundsList, worldRect);
            // 删除无需捕捉的点
            List<SnapPoint> filterResult = new List<SnapPoint>();
            SnapOption snapOption = GetComponent<EditerComponent>().EditOption.SnapOption;
            foreach (var item in allSnap)
            {
                switch (item.Type)
                {
                    case SnapType.Endpoint:
                        if (snapOption.Endpoint) filterResult.Add(item);
                        break;
                    case SnapType.Midpoint:
                        if (snapOption.Midpoint) filterResult.Add(item);
                        break;
                    case SnapType.Center:
                        if (snapOption.Center) filterResult.Add(item);
                        break;
                    case SnapType.Intersection:
                        if (snapOption.Intersection) filterResult.Add(item);
                        break;
                    case SnapType.Tangent:
                        if (snapOption.Tangent) filterResult.Add(item);
                        break;
                    case SnapType.Perpendicular:
                        break;
                    case SnapType.Parallel:
                        break;
                    case SnapType.Point:
                        break;
                }
            }
            SnapPointList = filterResult;
            _snapMarkLayer.SnapPointList = filterResult;
            _snapMarkLayer.Update();
        }

        public List<SnapPoint> UpdateSnapPoint(List<SnapPoint> snapPointList, Point mousePoint)
        {
            Rect rect = new Rect(mousePoint.X - 24, mousePoint.Y - 24, 48, 48);
            Point leftTop = GetComponent<LayerComponent>().GetWorldPoint(rect.TopLeft);
            Point rightBottom = GetComponent<LayerComponent>().GetWorldPoint(rect.BottomRight);
            Rect worldRect = new Rect(leftTop, rightBottom);

            List<SnapPoint> result = SnapPicker.PickSnapPoint(snapPointList, worldRect);
            _snapMarkLayer.SnapPointList = result;
            _snapMarkLayer.Update();
            return result;
        }

        /// <summary>
        /// 获取与指定对象相交的对象列表
        /// </summary>
        public List<GeoVisual> GetintersectVisual(GeoVisual visual)
        {
            List<GeoVisual> result = new List<GeoVisual>();
            // 查找包围盒相交的可视对象
            IReadOnlyList<IBox> boundsList = _tree.Find(visual.Bounds.Extend(0.001));
            // 精筛相交的可视对象
            foreach (GeoVisual item in boundsList)
            {
                // 跳过自身
                if (item == visual) continue;
                // 判断是否相交
                if (GeoTool.Instance.IsIntersection(item, visual))
                    result.Add(item);
            }
            return result;
        }

        /// <summary>
        /// 获取区域内的可视对象列表
        /// </summary>
        public List<GeoVisual> GetVisualByRect(Rect rect, bool intersect)
        {
            List<GeoVisual> result = new List<GeoVisual>();

            // 查找包围盒相交的可视对象
            IReadOnlyList<IBox> boundsList = _tree.Find(rect);
            // 遍历对象
            foreach (GeoVisual box in boundsList)
            {
                // 添加框内对象
                if (rect.Contains(box.Bounds))
                {
                    result.Add(box);
                    continue;
                }
                // 交叉选择，添加相交对象
                if (intersect && GeoTool.Instance.IsIntersection(box, rect))
                    result.Add(box);
            }

            return result;
        }

        #endregion

        #region 生命周期

        protected override void Enable()
        {
            _rtViewLayer = GetComponent<LayerComponent>().RTreeViewLayer;
            _snapMarkLayer = GetComponent<LayerComponent>().SnapMarkLayer;
        }

        #endregion

        #region 私有方法



        #endregion

        #region 字段

        /// <summary>可视对象列表</summary>
        private readonly List<GeoVisual> _visualList = new List<GeoVisual>();
        /// <summary>悬停包围盒列表</summary>
        private readonly List<GeoVisual> _hoveredBoundsList = new List<GeoVisual>();
        /// <summary>悬停对象列表</summary>
        private readonly List<GeoVisual> _hoveredList = new List<GeoVisual>();

        private readonly RTree _tree = new RTree();

        private RTreeViewLayer _rtViewLayer;
        private SnapMarkLayer _snapMarkLayer;

        #endregion
    }
}