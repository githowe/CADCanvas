using CADCanvas.SubSystem.DrawingSystem;
using CADCanvas.SubSystem.EditerSystem.Component.Tool.RTree;
using CADCanvas.SubSystem.EditerSystem.Layer;
using System.Windows;
using XLogic.Base.UI;

namespace CADCanvas.SubSystem.EditerSystem.Component
{
    /// <summary>
    /// 场景组件：管理场景中的对象
    /// </summary>
    public class SceneComponent : Component<Editer>
    {
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
            _layer.LayerList = _tree.LayerList;
            _layer.Update();
        }

        public void UpdateHitedBounds(Rect rect)
        {
            IReadOnlyList<IBox> boundsList = _tree.Find(rect);
            _layer.HoveredList.Clear();
            foreach (var item in boundsList)
                _layer.HoveredList.Add(item.Bounds);
            _layer.Update();
        }

        #region 生命周期

        protected override void Enable()
        {
            _layer = GetComponent<LayerComponent>().RTreeViewLayer;
        }

        #endregion

        #region 私有方法



        #endregion

        /// <summary>可视对象列表</summary>
        private readonly List<GeoVisual> _visualList = new List<GeoVisual>();

        private readonly RTree _tree = new RTree();

        private RTreeViewLayer _layer;
    }
}