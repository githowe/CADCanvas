using System.Runtime.InteropServices;

namespace CADCanvas.SubSystem.DrawingSystem
{
    /// <summary>
    /// 直线互操作接口
    /// </summary>
    public class LineInterop
    {
        /// <summary>
        /// 创建无限延伸的直线
        /// </summary>
        [DllImport("OCCTBridge.dll")]
        public static extern IntPtr CreateLine(double x1, double y1, double x2, double y2);

        /// <summary>
        /// 创建直线段
        /// </summary>
        [DllImport("OCCTBridge.dll")]
        public static extern IntPtr CreateLineSegment(double x1, double y1, double x2, double y2);
    }
}