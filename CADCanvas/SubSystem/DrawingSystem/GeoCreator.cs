using System.Windows;

namespace CADCanvas.SubSystem.DrawingSystem
{
    /// <summary>
    /// 几何图形创建器
    /// </summary>
    public class GeoCreator
    {
        #region 单例

        private GeoCreator() { }
        public static GeoCreator Instance { get; } = new GeoCreator();

        #endregion

        #region 公开方法

        /// <summary>
        /// 创建直线
        /// </summary>
        public VisualLine CreateLine(double x1, double y1, double x2, double y2)
        {
            IntPtr lineHandle = LineInterop.CreateLine(x1, y1, x2, y2);
            VisualLine line = new VisualLine
            {
                Handle = lineHandle,
                Start = new Point(x1, y1),
                End = new Point(x2, y2),
            };
            return line;
        }

        #endregion
    }
}