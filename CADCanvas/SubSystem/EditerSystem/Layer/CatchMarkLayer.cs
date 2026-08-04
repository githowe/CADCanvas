using XLogic.Wpf.Drawing;

namespace CADCanvas.SubSystem.EditerSystem.Layer
{
    /// <summary>
    /// 捕捉标记
    /// </summary>
    public class CatchMarkLayer : DrawingLayer
    {
        public GridLayer? Grid { get; set; } = null;
    }
}