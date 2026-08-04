using CADCanvas.SubSystem.EditerSystem.Component.Tool.RTree;
using System.Windows;
using System.Windows.Media;

namespace CADCanvas.SubSystem.DrawingSystem
{
    /// <summary>
    /// 表示几何图形的可视化对象
    /// </summary>
    public abstract class GeoVisual : IBox
    {
        public abstract Rect Bounds { get; }

        public virtual void Init() { }

        public abstract void Draw(DrawingContext dc, IWorldGrid grid);
    }
}