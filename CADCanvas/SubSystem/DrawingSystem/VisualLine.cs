using System.Windows;
using System.Windows.Media;

namespace CADCanvas.SubSystem.DrawingSystem
{
    /// <summary>
    /// 表示直线的可视化对象
    /// </summary>
    public class VisualLine : GeoVisual
    {
        /// <summary>直线句柄</summary>
        public IntPtr Handle { get; set; } = IntPtr.Zero;

        public Point Start { get; set; } = new Point(0, 0);

        public Point End { get; set; } = new Point(0, 0);

        public Color LineColor { get; set; } = Colors.White;

        public double LineWidth { get; set; } = 1.0;

        public override void Init()
        {
            _pen = new Pen(new SolidColorBrush(LineColor), LineWidth);
            _pen.Freeze();
        }

        public override void Draw(DrawingContext dc, IWorldGrid grid)
        {
            dc.DrawLine(_pen, grid.ToScreen(Start), grid.ToScreen(End));
        }

        private Pen? _pen = null;
    }
}