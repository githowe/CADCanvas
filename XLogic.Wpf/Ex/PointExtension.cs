using System.Windows;

namespace XLogic.Wpf.Ex
{
    public static class PointExtension
    {
        /// <summary>
        /// 偏移坐标
        /// </summary>
        public static Point OffsetPoint(this Point point, double offsetX, double offsetY)
        {
            return new Point(point.X + offsetX, point.Y + offsetY);
        }
    }
}