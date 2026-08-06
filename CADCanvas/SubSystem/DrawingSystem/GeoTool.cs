using System.Runtime.InteropServices;
using System.Windows;
using XLogic.AppFrame;

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
        /// 初始化二维点缓存。将C#层的数组的地址传递给C++，以在C++中读写该数组
        /// </summary>
        [DllImport("OCCTBridge.dll")]
        private static extern void InitPoint2DCache(IntPtr pointCache, int length);

        /// <summary>
        /// 获取两条曲线的交点
        /// </summary>
        [DllImport("OCCTBridge.dll")]
        private static extern int GetIntersection(IntPtr curve1, IntPtr curve2);

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

        #endregion
    }
}