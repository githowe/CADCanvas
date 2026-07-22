using CADCanvas.AppTool;
using System.Windows;

namespace CADCanvas
{
    public partial class App : Application
    {
        public App()
        {
            Startup += App_Startup;
        }

        private void App_Startup(object sender, StartupEventArgs e)
        {
            Init();
        }

        private void Init()
        {
            SystemDataDelegate.Instance.Init();
        }
    }
}