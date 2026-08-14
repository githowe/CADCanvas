using System.Runtime.InteropServices;
using System.Windows;
using XLogic.AppFrame;
using XLogic.Wpf;

namespace CADCanvas.SubSystem.DrawingSystem
{
    /// <summary>
    /// 几何图形工具
    /// </summary>
    public class GeoTool : IManager
    {
        #region 单例

        private GeoTool() { }
        public static GeoTool Instance { get; } = new GeoTool();

        #endregion

        #region 属性

        public double[] PointCache { get; set; } = new double[32];

        private GCHandle _pointCacheHandle;

        #endregion

        #region 互操作接口

        /// <summary>
        /// 释放曲线
        /// </summary>
        [DllImport("OCCTBridge.dll")]
        private static extern void FreeCurve(IntPtr curve);

        /// <summary>
        /// 初始化二维点缓存。将C#层的数组的地址传递给C++，以在C++中读写该数组
        /// </summary>
        [DllImport("OCCTBridge.dll")]
        private static extern void InitPoint2DCache(IntPtr pointCache, int length);

        /// <summary>
        /// 获取两条曲线的交点
        /// </summary>
        [DllImport("OCCTBridge.dll")]
        private static extern int GetIntersection(IntPtr curve1, IntPtr curve2);

        /// <summary>
        /// 获取曲线与射线的交点
        /// </summary>
        [DllImport("OCCTBridge.dll")]
        private static extern int GetIntersectionWithRay(IntPtr curve, double x, double y, double dx, double dy);

        /// <summary>
        /// 判断曲线与曲线是否相交
        /// </summary>
        [DllImport("OCCTBridge.dll")]
        private static extern bool IsIntersection(IntPtr curve1, IntPtr curve2);

        /// <summary>
        /// 判断曲线是否与矩形相交
        /// </summary>
        [DllImport("OCCTBridge.dll")]
        private static extern bool IsIntersectionWithRect(IntPtr curve, double left, double top, double right, double bottom);

        #endregion

        #region 生命周期

        public void Init()
        {
            _pointCacheHandle = GCHandle.Alloc(PointCache, GCHandleType.Pinned);
            InitPoint2DCache(_pointCacheHandle.AddrOfPinnedObject(), PointCache.Length);
        }

        public void Reset() { }

        public void Clear()
        {
            _pointCacheHandle.Free();
        }

        #endregion

        #region 公开方法

        /// <summary>
        /// 释放曲线
        /// </summary>
        public void FreeCurve(GeoVisual visual)
        {
            if (visual.Handle != IntPtr.Zero)
            {
                FreeCurve(visual.Handle);
                visual.Handle = IntPtr.Zero;
            }
        }

        /// <summary>
        /// 获取两个几何图形的全部交点
        /// </summary>
        public List<Point> GetIntersection(GeoVisual visual1, GeoVisual visual2)
        {
            List<Point> result = new List<Point>();
            // 求交并返回交点数量
            int count = GetIntersection(visual1.Handle, visual2.Handle);
            if (count <= 0) return result;
            // 生成交点列表
            for (int counter = 0; counter < count; counter++)
            {
                double x = PointCache[counter * 2];
                double y = PointCache[counter * 2 + 1];
                result.Add(new Point(x, y));
            }
            // 返回结果
            return result;
        }

        /// <summary>
        /// 获取几何图形与射线的全部交点
        /// </summary>
        public List<Point> GetIntersection(GeoVisual visual, Point start, double angle)
        {
            List<Point> result = new List<Point>();
            // 求交并返回交点数量
            double rad = MathTool.AngleToRadian(angle);
            double dx = Math.Cos(rad);
            double dy = Math.Sin(rad);
            int count = GetIntersectionWithRay(visual.Handle, start.X, start.Y, dx, dy);
            if (count <= 0) return result;
            // 生成交点列表
            for (int counter = 0; counter < count; counter++)
            {
                double x = PointCache[counter * 2];
                double y = PointCache[counter * 2 + 1];
                result.Add(new Point(x, y));
            }
            // 返回结果
            return result;
        }

        /// <summary>
        /// 判断几何图形是否与矩形相交
        /// </summary>
        public bool IsIntersection(GeoVisual visual, Rect rect)
        {
            return IsIntersectionWithRect(visual.Handle, rect.Left, rect.Top, rect.Right, rect.Bottom);
        }

        /// <summary>
        /// 判断两个几何图形是否相交
        /// </summary>
        public bool IsIntersection(GeoVisual visual1, GeoVisual visual2)
        {
            return IsIntersection(visual1.Handle, visual2.Handle);
        }

        #endregion
    }
}