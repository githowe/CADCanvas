using CADCanvas.SubSystem.EditerSystem.Component.Tool.RTree;
using System.Windows;
using System.Windows.Media;
using XLogic.Wpf.Drawing;

namespace CADCanvas.SubSystem.EditerSystem.Layer
{
    public class RTreeViewLayer : DrawingLayer
    {
        public GridLayer? Grid { get; set; } = null;

        public List<BoundsLayer> LayerList { get; set; } = new List<BoundsLayer>();

        public List<Rect> HoveredList { get; set; } = new List<Rect>();

        public override void Init()
        {
            // 添加画笔
            CreatePen(255, 102, 102);
            CreatePen(255, 225, 102);
            CreatePen(163, 255, 77);
            CreatePen(102, 255, 103);
            CreatePen(102, 255, 226);
            CreatePen(102, 162, 255);
            CreatePen(165, 102, 255);
            CreatePen(255, 102, 222);
            CreatePen(255, 26, 26);
            CreatePen(255, 210, 26);
            CreatePen(116, 255, 26);
            CreatePen(116, 255, 26);
            CreatePen(26, 255, 119);
            CreatePen(26, 207, 255);
            CreatePen(28, 26, 255);
            CreatePen(212, 26, 255);
            _white.Freeze();
            _hover.Freeze();
        }

        protected override void OnUpdate()
        {
            // DrawBounds();
            DrawHovered();
        }

        private void CreatePen(byte r, byte g, byte b)
        {
            Pen pen = new Pen(new SolidColorBrush(Color.FromArgb(128, r, g, b)), 1);
            pen.Freeze();
            _penList.Add(pen);
        }

        private void DrawBounds()
        {
            for (int index = 0; index < LayerList.Count; index++)
            {
                BoundsLayer? layer = LayerList[index];
                foreach (var bounds in layer.BoundsList)
                {
                    Point start = Grid.ToScreen(bounds.TopLeft);
                    Point end = Grid.ToScreen(bounds.BottomRight);
                    _dc.DrawRectangle(null, _penList[index], new Rect(start, end));
                }
            }
        }

        private void DrawHovered()
        {
            foreach (var bounds in HoveredList)
            {
                Point start = Grid.ToScreen(bounds.TopLeft);
                Point end = Grid.ToScreen(bounds.BottomRight);
                _dc.DrawRectangle(null, _hover, new Rect(start, end));
            }
        }

        private readonly List<Pen> _penList = new List<Pen>();
        private readonly Pen _white = new Pen(Brushes.White, 1);
        private readonly Pen _hover = new Pen(new SolidColorBrush(Color.FromArgb(255, 255, 0, 255)), 1);
    }
}