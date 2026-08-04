using CADCanvas.SubSystem.DrawingSystem;
using XLogic.Wpf.Drawing;

namespace CADCanvas.SubSystem.EditerSystem.Layer
{
    /// <summary>
    /// 图形图层：绘制图形的图层
    /// </summary>
    public class GraphicLayer : DrawingLayer
    {
        public IWorldGrid Grid { get; set; }

        public List<GeoVisual> GeoVisualList { get; set; } = new List<GeoVisual>();

        public override void Init()
        {
            IsHitTestVisible = false;
        }

        protected override void OnUpdate()
        {
            foreach (var visual in GeoVisualList)
            {
                visual.Draw(_dc, Grid);
            }
        }
    }
}