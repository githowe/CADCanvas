using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace CADCanvas.SubSystem.DrawingSystem
{
    /// <summary>
    /// 表示几何图形的可视化对象
    /// </summary>
    public abstract class GeoVisual
    {
        public virtual void Init() { }

        public abstract void Draw(DrawingContext dc, IWorldGrid grid);
    }
}