using System.Windows;

namespace CADCanvas.SubSystem.DrawingSystem
{
    public interface IWorldGrid
    {
        /// <summary>
        /// 世界坐标转屏幕坐标
        /// </summary>
        Point ToScreen(Point world);

        /// <summary>
        /// 屏幕坐标转世界坐标
        /// </summary>
        Point ToWorld(Point screen);
    }
}