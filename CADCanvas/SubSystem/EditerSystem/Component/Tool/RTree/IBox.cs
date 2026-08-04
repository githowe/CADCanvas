using System.Windows;

namespace CADCanvas.SubSystem.EditerSystem.Component.Tool.RTree
{
    /// <summary>
    /// 盒子对象，表示具有包围盒的对象
    /// </summary>
    public interface IBox
    {
        Rect Bounds { get; }
    }
}