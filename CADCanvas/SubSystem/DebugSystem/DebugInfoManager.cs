using XLogic.WpfControl;

namespace CADCanvas.SubSystem.DebugSystem
{
    public class DebugInfoManager
    {
        #region 单例

        private DebugInfoManager() { }
        public static DebugInfoManager Instance { get; } = new DebugInfoManager();

        #endregion

        public void SetInfoBoard(InfoBoard infoBoard)
        {
            _infoBoard = infoBoard;
        }

        public void ToggleInfoBoard()
        {
            if (_infoBoard == null) return;

            if (_infoBoard.Visibility == System.Windows.Visibility.Visible)
                _infoBoard.Visibility = System.Windows.Visibility.Collapsed;
            else if (_infoBoard.Visibility == System.Windows.Visibility.Collapsed)
                _infoBoard.Visibility = System.Windows.Visibility.Visible;
        }

        public void AddInfo(params string[] titleArray)
        {
            if (_infoBoard == null) return;
            _infoBoard.AddInfo(titleArray);
        }

        public void UpdateInfo(string title, string info)
        {
            if (_infoBoard == null) return;
            _infoBoard.UpdateInfo(title, info);
        }

        public void ClearInfo()
        {
            if (_infoBoard == null) return;
            _infoBoard.ClearInfo();
        }

        private InfoBoard? _infoBoard;
    }
}