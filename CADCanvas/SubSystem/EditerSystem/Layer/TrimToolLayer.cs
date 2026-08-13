using CADCanvas.SubSystem.DrawingSystem;
using XLogic.Wpf.Drawing;

namespace CADCanvas.SubSystem.EditerSystem.Layer
{
    /// <summary>
    /// 修剪工具图层
    /// </summary>
    public class TrimToolLayer : DrawingLayer
    {
        public GridLayer? Grid { get; set; } = null;

        public List<GeoVisual> GeoVisualList { get; set; } = new List<GeoVisual>();

        protected override void OnUpdate()
        {
            foreach (var visual in GeoVisualList)
            {
                if (visual.Hidden) continue;
                if (visual.Opacity < 1.0) _dc.PushOpacity(visual.Opacity);
                visual.Draw(_dc, Grid);
                if (visual.Opacity < 1.0) _dc.Pop();
            }
        }
    }
}