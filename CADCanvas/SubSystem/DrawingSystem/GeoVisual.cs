using CADCanvas.SubSystem.EditerSystem.Component.Tool.RTree;
using CADCanvas.SubSystem.EditerSystem.Component.Tool.Snap;
using System.Windows;
using System.Windows.Media;

namespace CADCanvas.SubSystem.DrawingSystem
{
    /// <summary>
    /// 表示几何图形的可视化对象
    /// </summary>
    public abstract class GeoVisual : IBox
    {
        /// <summary>图形句柄</summary>
        public IntPtr Handle { get; set; } = IntPtr.Zero;

        public abstract Rect Bounds { get; }

        public virtual void Init() { }

        public abstract void Draw(DrawingContext dc, IWorldGrid grid);

        public abstract List<SnapPoint> GetSnapPointList();
    }
}