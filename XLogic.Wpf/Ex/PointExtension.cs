using System.Windows;

namespace XLogic.Wpf.Ex
{
    public static class PointExtension
    {
        /// <summary>
        /// 偏移坐标
        /// </summary>
        public static Point OffsetTo(this Point point, double offset)
        {
            return new Point(point.X + offset, point.Y + offset);
        }

        /// <summary>
        /// 偏移坐标
        /// </summary>
        public static Point OffsetTo(this Point point, double offsetX, double offsetY)
        {
            return new Point(point.X + offsetX, point.Y + offsetY);
        }

        /// <summary>
        /// 转换为数学坐标系的点
        /// </summary>
        public static Point ToMathPoint(this Point screenPoint) => new Point(screenPoint.X, -screenPoint.Y);
    }
}