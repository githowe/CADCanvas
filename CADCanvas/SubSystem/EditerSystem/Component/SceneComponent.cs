using CADCanvas.SubSystem.DrawingSystem;
using CADCanvas.SubSystem.EditerSystem.Component.Tool.RTree;
using CADCanvas.SubSystem.EditerSystem.Component.Tool.Snap;
using CADCanvas.SubSystem.EditerSystem.Layer;
using System.Collections.Generic;
using System.Windows;
using XLogic.Base.UI;

namespace CADCanvas.SubSystem.EditerSystem.Component
{
    /// <summary>
    /// 场景组件：管理场景中的对象
    /// </summary>
    public class SceneComponent : Component<Editer>
    {
        public List<GeoVisual> AllVisual => _visualList;

        public List<GeoVisual> HoveredVisual => _hoveredList;

        public List<SnapPoint> SnapPointList { get; set; } = new List<SnapPoint>();

        public void AddVisual(GeoVisual visual)
        {
            _visualList.Add(visual);
            _tree.Insert(visual, visual.Bounds);
            _tree.UpdateBoundsLayer();
            UpdateBoundsLayerView();
        }

        public void RemoveVisual(GeoVisual visual)
        {
            _visualList.Remove(visual);
            _tree.Clear();
            _tree.Build(_visualList.Cast<IBox>().ToList());
            _tree.UpdateBoundsLayer();
            UpdateBoundsLayerView();
        }

        public void ClearVisual()
        {
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

        public void UpdateHitedBounds(Rect rect)
        {
            IReadOnlyList<IBox> boundsList = _tree.Find(rect);
            _hoveredList.Clear();
            _rtViewLayer.HoveredList.Clear();
            foreach (var item in boundsList)
            {
                _hoveredList.Add((GeoVisual)item);
                _rtViewLayer.HoveredList.Add(item.Bounds);
            }
            _rtViewLayer.Update();
        }

        /// <summary>
        /// 根据鼠标，获取附近的捕捉点，并绘制捕捉点
        /// </summary>
        public void UpdateSnapPoint(Point mousePoint)
        {
            Rect rect = new Rect(mousePoint.X - 16, mousePoint.Y - 16, 32, 32);
            Point leftTop = GetComponent<LayerComponent>().GetWorldPoint(rect.TopLeft);
            Point rightBottom = GetComponent<LayerComponent>().GetWorldPoint(rect.BottomRight);
            Rect worldRect = new Rect(leftTop, rightBottom);

            SnapPointList = SnapPicker.PickSnapPoint(_hoveredList, worldRect);
            _snapMarkLayer.SnapPointList = SnapPointList;
            _snapMarkLayer.Update();
        }

        public List<SnapPoint> UpdateSnapPoint(List<SnapPoint> snapPointList, Point mousePoint)
        {
            Rect rect = new Rect(mousePoint.X - 16, mousePoint.Y - 16, 32, 32);
            Point leftTop = GetComponent<LayerComponent>().GetWorldPoint(rect.TopLeft);
            Point rightBottom = GetComponent<LayerComponent>().GetWorldPoint(rect.BottomRight);
            Rect worldRect = new Rect(leftTop, rightBottom);

            List<SnapPoint> result = SnapPicker.PickSnapPoint(snapPointList, worldRect);
            _snapMarkLayer.SnapPointList = result;
            _snapMarkLayer.Update();
            return result;
        }

        #region 生命周期

        protected override void Enable()
        {
            _rtViewLayer = GetComponent<LayerComponent>().RTreeViewLayer;
            _snapMarkLayer = GetComponent<LayerComponent>().SnapMarkLayer;
        }

        #endregion

        #region 私有方法



        #endregion

        /// <summary>可视对象列表</summary>
        private readonly List<GeoVisual> _visualList = new List<GeoVisual>();

        private readonly List<GeoVisual> _hoveredList = new List<GeoVisual>();

        private readonly RTree _tree = new RTree();

        private RTreeViewLayer _rtViewLayer;
        private SnapMarkLayer _snapMarkLayer;
    }
}