using CADCanvas.SubSystem.DrawingSystem;
using XLogic.Wpf.Drawing;

namespace CADCanvas.SubSystem.EditerSystem.Layer
{
    public class SelectMarkLayer : DrawingLayer
    {
        public GridLayer? Grid { get; set; } = null;

        public List<GeoVisual> HoveredVisual { get; set; } = new List<GeoVisual>();

        public List<GeoVisual> SelectedVisual { get; set; } = new List<GeoVisual>();

        protected override void OnUpdate()
        {
            foreach (var item in HoveredVisual)
            {
                if (SelectedVisual.Contains(item)) continue;
                item.DrawHover(_dc, Grid);
            }
            foreach (var item in SelectedVisual)
            {
                item.DrawSelect(_dc, Grid);
            }
        }
    }
}