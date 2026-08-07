using System.Windows;

namespace XLogic.Wpf
{
    public class PointTool
    {
        /// <summary>
        /// 计算两点之间的长度
        /// </summary>
        public static double GetLength(Point start, Point end)
        {
            double dx = end.X - start.X;
            double dy = end.Y - start.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// 计算两点之间的角度，返回周角度数
        /// </summary>
        public static double GetAngle(Point mathStart, Point mathEnd)
        {
            // 计算偏移
            Point offset = new Point(mathEnd.X - mathStart.X, mathEnd.Y - mathStart.Y);
            // 计算弧度
            double radians;
            if (mathEnd.Y >= mathStart.Y) radians = Math.Atan2(offset.Y, offset.X);
            else radians = Math.Atan2(offset.Y, offset.X) + 2 * Math.PI;
            // 返回角度
            return radians * 180 / Math.PI;
        }
    }
}