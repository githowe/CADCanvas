using System.Windows;

namespace XLogic.Wpf
{
    public class MathTool
    {
        /// <summary>
        /// 角度转弧度
        /// </summary>
        public static double AngleToRadian(double angle) => angle * Math.PI / 180.0;

        /// <summary>
        /// 弧度转角度
        /// </summary>
        public static double RadianToAngle(double radian) => radian * 180.0 / Math.PI;

        /// <summary>
        /// 根据角度计算终点坐标
        /// </summary>
        public static Point GetEndPointFromAngle(double angle)
        {
            Point result = new Point();
            double radian = AngleToRadian(angle);
            result.X = Math.Cos(radian);
            result.Y = Math.Sin(radian);
            return result;
        }
    }
}