using System.Windows;
using System.Windows.Media;
using XLogic.Wpf.Drawing;

namespace CADCanvas.SubSystem.EditerSystem.Layer
{
    /// <summary>
    /// 直线工具图层
    /// </summary>
    public class LineToolLayer : DrawingLayer
    {
        public GridLayer? Grid { get; set; } = null;

        public Point? StartPoint { get; set; } = null;

        public Point? EndPoint { get; set; } = null;

        public override void Init()
        {
            _pen.Freeze();
            IsHitTestVisible = false;
        }

        protected override void OnUpdate()
        {
            if (StartPoint == null || EndPoint == null)
                return;
            _dc.DrawLine(_pen, Grid.ToScreen(StartPoint.Value), Grid.ToScreen(EndPoint.Value));
        }

        private readonly Pen _pen = new Pen(Brushes.White, 1);
    }
}