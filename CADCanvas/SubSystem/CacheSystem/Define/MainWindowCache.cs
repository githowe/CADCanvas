using System.Windows;

namespace CADCanvas.SubSystem.CacheSystem.Define
{
    public class MainWindowCache
    {
        public WindowState State { get; set; } = WindowState.Normal;

        public int Width { get; set; } = 1280;

        public int Height { get; set; } = 720;
    }
}