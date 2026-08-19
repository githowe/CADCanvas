using CADCanvas.SubSystem.EditerSystem.Component.Tool.Snap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using XLogic.Wpf.Drawing;

namespace CADCanvas.SubSystem.DrawingSystem
{
    public class VisualArc : GeoVisual
    {
        #region 属性

        public Point Center { get; set; } = new Point();

        public double Radius { get; set; } = 0;

        public double StartAngle { get; set; } = 0;

        public double EndAngle { get; set; } = 0;

        public RotateDirection Direction { get; set; } = RotateDirection.CounterClockwise;

        public override Rect Bounds => Rect.Empty;

        #endregion

        #region 公开方法

        public override void Init()
        {
            
        }

        public override void Draw(DrawingContext dc, IWorldGrid grid)
        {
            
        }

        public override void DrawHover(DrawingContext dc, IWorldGrid grid)
        {
            
        }

        public override void DrawSelect(DrawingContext dc, IWorldGrid grid)
        {
            
        }

        public override List<SnapPoint> GetSnapPointList()
        {
            return new List<SnapPoint>();
        }

        public override List<GeoVisual> SplitByIntersectionPoint(List<Point> pointList)
        {
            return new List<GeoVisual>();
        }

        public override List<GeoVisual> JointSplitVisual(List<GeoVisual> visualList)
        {
            return new List<GeoVisual>();
        }

        #endregion

        #region 字段

        private Pen? _pen = null;

        #endregion
    }
}