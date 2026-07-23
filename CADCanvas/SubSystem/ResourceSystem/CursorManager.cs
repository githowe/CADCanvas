using System.Windows;
using System.Windows.Input;
using System.Windows.Resources;
using XLogic.AppFrame;

namespace CADCanvas.SubSystem.ResourceSystem
{
    public class CursorManager : IManager
    {
        #region 单例

        private CursorManager() { }
        public static CursorManager Instance { get; } = new CursorManager();

        #endregion

        #region 光标

        /// <summary>绘制</summary>
        public Cursor? Draw { get; set; }

        #endregion

        public void Init()
        {
            Draw = LoadCursor("Assets/Cursor/Draw.cur");
        }

        public void Reset() { }

        public void Clear() { }

        #region 私有方法

        private Cursor LoadCursor(string cursorPath)
        {
            StreamResourceInfo resourceInfo = Application.GetResourceStream(new Uri(cursorPath, UriKind.Relative));
            return new Cursor(resourceInfo.Stream);
        }

        #endregion
    }
}