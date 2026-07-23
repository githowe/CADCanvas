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

        /// <summary>选择</summary>
        public Cursor? Select { get; set; }

        /// <summary>绘制</summary>
        public Cursor? Draw { get; set; }

        /// <summary>移动</summary>
        public Cursor? Move { get; set; }

        #endregion

        public void Init()
        {
            Select = LoadCursor("Assets/Cursor/Select.cur");
            Draw = LoadCursor("Assets/Cursor/Draw.cur");
            Move = LoadCursor("Assets/Cursor/Move.cur");
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