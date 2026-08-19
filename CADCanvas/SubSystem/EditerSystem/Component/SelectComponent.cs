using CADCanvas.SubSystem.DrawingSystem;
using CADCanvas.SubSystem.EditerSystem.Layer;
using System.Windows;
using XLogic.Base.UI;

namespace CADCanvas.SubSystem.EditerSystem.Component
{
    /// <summary>
    /// 选择组件
    /// </summary>
    public class SelectComponent : Component<Editer>
    {
        public List<GeoVisual> HoveredVisual { get; set; } = new List<GeoVisual>();

        public List<GeoVisual> SelectedVisual { get; set; } = new List<GeoVisual>();

        protected override void Enable()
        {
            _layer = GetComponent<LayerComponent>().SelectMarkLayer;
        }

        public void UpdateHoveredVisual(Rect rectWorld)
        {
            GetComponent<SceneComponent>().UpdateHitedVisual(rectWorld);
            HoveredVisual = GetComponent<SceneComponent>().HoveredVisual;
            _layer.HoveredVisual = HoveredVisual;
            _layer.Update();
        }

        private SelectMarkLayer _layer;
    }
}