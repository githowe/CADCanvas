using System.Windows;

namespace CADCanvas.SubSystem.EditerSystem.Component.Tool.RTree
{
    /// <summary>
    /// 包围盒图层
    /// </summary>
    public class BoundsLayer
    {
        public List<Rect> BoundsList { get; private set; } = new List<Rect>();
    }
}