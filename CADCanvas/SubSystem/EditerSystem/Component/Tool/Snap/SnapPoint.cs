using System.Windows;

namespace CADCanvas.SubSystem.EditerSystem.Component.Tool.Snap
{
    /// <summary>
    /// 捕捉点
    /// </summary>
    public class SnapPoint
    {
        public SnapType Type { get; set; } = SnapType.None;

        public Point WorldPoint { get; set; } = new Point();
    }
}