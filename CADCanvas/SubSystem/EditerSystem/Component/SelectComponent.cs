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

        public void ClearHover()
        {
            HoveredVisual.Clear();
            _layer.HoveredVisual = HoveredVisual;
            _layer.Update();
        }

        public void AddSelect()
        {
            // 添加悬停对象
            foreach (var item in HoveredVisual)
            {
                if (SelectedVisual.Contains(item)) continue;
                SelectedVisual.Add(item);
            }
            // 更新选中对象
            _layer.SelectedVisual = SelectedVisual;
            _layer.Update();
        }

        public void AddSelect(Rect rect, bool intersect)
        {
            List<GeoVisual> selected = GetComponent<SceneComponent>().GetVisualByRect(rect, intersect);
            foreach (var item in selected)
            {
                if (SelectedVisual.Contains(item)) continue;
                SelectedVisual.Add(item);
            }
            _layer.SelectedVisual = SelectedVisual;
            _layer.Update();
        }

        public void ClearSelect()
        {
            SelectedVisual.Clear();
            _layer.SelectedVisual = SelectedVisual;
            _layer.Update();
        }

        private SelectMarkLayer _layer;
    }
}