using CADCanvas.AppTool;
using CADCanvas.SubSystem.DrawingSystem;
using CADCanvas.SubSystem.ResourceSystem;
using System.Windows;

namespace CADCanvas
{
    public partial class App : Application
    {
        public App()
        {
            Startup += App_Startup;
            Exit += App_Exit;
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            Init();
        }

        private void Init()
        {
            SystemDataDelegate.Instance.Init();
            CursorManager.Instance.Init();
            ImageManager.Instance.Init();
            TimerEngine.Instance.Start();
            GeoTool.Instance.Init();
            BrushManager.Instance.Init();
        }

        private void App_Exit(object sender, ExitEventArgs e)
        {
            GeoTool.Instance.Clear();
        }
    }
}