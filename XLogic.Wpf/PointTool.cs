using System.Windows;

namespace XLogic.Wpf
{
    /// <summary>
    /// 终点位置
    /// </summary>
    public enum EndPosition
    {
        LeftTop,
        RightTop,
        LeftBottom,
        RightBottom,

        LeftAxis,
        RightAxis,
        TopAxis,
        BottomAxis,

        Origin,
    }

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

        /// <summary>
        /// 获取终点相对于起点的位置
        /// </summary>
        public static EndPosition GetEndPosition(Point start, Point end)
        {
            // 横坐标终点等于起点
            if (end.X == start.X)
            {
                if (end.Y == start.Y) return EndPosition.Origin;
                else if (end.Y < start.Y) return EndPosition.BottomAxis;
                else return EndPosition.TopAxis;
            }
            // 横坐标终点小于起点
            else if (end.X < start.X)
            {
                if (end.Y == start.Y) return EndPosition.LeftAxis;
                else if (end.Y < start.Y) return EndPosition.LeftBottom;
                else return EndPosition.LeftTop;
            }
            // 横坐标终点大于起点
            else
            {
                if (end.Y == start.Y) return EndPosition.RightAxis;
                else if (end.Y < start.Y) return EndPosition.RightBottom;
                else return EndPosition.RightTop;
            }
        }

        /// <summary>
        /// 获取两点之间的中点
        /// </summary>
        public static Point GetMidPoint(Point start, Point end) => new Point((start.X + end.X) / 2, (start.Y + end.Y) / 2);

        /// <summary>
        /// 创建矩形区域
        /// </summary>
        public static Rect CreateRect(Point center, double size) => CreateRect(center, size, size);

        /// <summary>
        /// 创建矩形区域
        /// </summary>
        public static Rect CreateRect(Point center, double width, double height)
        {
            double left = center.X - width / 2;
            double top = center.Y - height / 2;
            return new Rect(left, top, width, height);
        }
    }
}