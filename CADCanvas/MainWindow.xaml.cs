using CADCanvas.SubSystem.CacheSystem;
using CADCanvas.SubSystem.EditerSystem;
using System.Windows;
using System.Windows.Input;
using XLogic.Wpf.Window;

namespace CADCanvas
{
    public partial class MainWindow : XMainWindow
    {
        #region 构造方法

        public MainWindow()
        {
            InitializeComponent();
        }

        #endregion

        #region 窗口事件

        protected override void XWindowLoaded()
        {
            // 恢复窗口状态并监听窗口缩放
            RecoverWindowState();
            ListenWindowResize();

            // 加载编辑器
            LoadEditer();
        }

        private void MainWindow_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            // 处理系统快捷键
            HandleSystemShortcutKey(e);
            if (e.Handled) return;
            _editer?.HandleKeyDown(e);
        }

        private void MainWindow_PreviewKeyUp(object sender, KeyEventArgs e)
        {
            _editer?.HandleKeyUp(e);
        }

        #endregion

        #region 编辑器事件

        private void Editer_Inited()
        {
            _editer.LoadDocument();
        }

        #endregion

        #region 私有方法

        private void LoadEditer()
        {
            _editer = new Editer
            {
                Inited = Editer_Inited
            };
            MainGrid.Children.Add(_editer);
        }

        /// <summary>
        /// 恢复窗口状态
        /// </summary>
        private void RecoverWindowState()
        {
            WindowState = CacheManager.Instance.Cache.MainWindow.State;
            Width = CacheManager.Instance.Cache.MainWindow.Width;
            Height = CacheManager.Instance.Cache.MainWindow.Height;
            // 居中窗口
            Left = (SystemParameters.WorkArea.Width - Width) / 2;
            Top = (SystemParameters.WorkArea.Height - Height) / 2;
        }

        /// <summary>
        /// 监听窗口缩放
        /// </summary>
        private void ListenWindowResize()
        {
            StateChanged += (s, e) =>
            {
                if (WindowState is WindowState.Normal or WindowState.Maximized)
                {
                    CacheManager.Instance.Cache.MainWindow.State = WindowState;
                    CacheManager.Instance.Save();
                }
            };
            SizeChanged += (s, e) =>
            {
                if (WindowState == WindowState.Maximized) return;
                CacheManager.Instance.Cache.MainWindow.Width = (int)Width;
                CacheManager.Instance.Cache.MainWindow.Height = (int)Height;
                CacheManager.Instance.Save();
            };
        }

        /// <summary>
        /// 处理系统快捷键
        /// </summary>
        private void HandleSystemShortcutKey(KeyEventArgs e)
        {

        }

        #endregion

        #region 字段

        private Editer _editer;

        #endregion
    }
}