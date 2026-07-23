using System.Runtime.InteropServices;

namespace CADCanvas.SubSystem.DrawingSystem
{
    /// <summary>
    /// 直线互操作接口
    /// </summary>
    public class LineInterop
    {
        [DllImport("OCCTBridge.dll")]
        public static extern IntPtr CreateLine(double x1, double y1, double x2, double y2);
    }
}