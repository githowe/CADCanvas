using System.Windows;
using System.Windows.Media;
using XLogic.Wpf.Drawing;

namespace CADCanvas.SubSystem.EditerSystem.Layer
{
    public class SelectToolLayer : DrawingLayer
    {
        public GridLayer? Grid { get; set; } = null;

        public Point? SelectStart { get; set; } = null;

        public Point? SelectEnd { get; set; } = null;

        public override void Init()
        {
            _border.Freeze();
            _fillBlue.Freeze();
            _fillGreen.Freeze();
        }

        protected override void OnUpdate()
        {
            if (SelectStart == null || SelectEnd == null) return;

            Point worldStart = Grid.ToScreen(SelectStart.Value, true);
            Point worldEnd = Grid.ToScreen(SelectEnd.Value, true);
            Rect rect = new Rect(worldStart, worldEnd);
            if (worldEnd.X >= worldStart.X)
                _dc.DrawRectangle(_fillBlue, _border, rect);
            else
                _dc.DrawRectangle(_fillGreen, _border, rect);
        }

        private readonly Pen _border = new Pen(Brushes.White, 1);
        private readonly Brush _fillBlue = new SolidColorBrush(Color.FromArgb(50, 17, 70, 148));
        private readonly Brush _fillGreen = new SolidColorBrush(Color.FromArgb(50, 17, 142, 50));
    }
}