using XLogic.AppFrame;

namespace CADCanvas.SubSystem.OptionSystem
{
    public class OptionManager : IManager
    {
        #region 单例

        private OptionManager() { }
        public static OptionManager Instance { get; } = new OptionManager();

        #endregion

        #region 管理器接口

        public void Init()
        {
            CreateFolder(_root);
            CreateFolder(CachePath);
        }

        public void Reset() { }

        public void Clear() { }

        #endregion

        #region 选项

        /// <summary>缓存路径</summary>
        public string CachePath => _root + "\\Cache";

        #endregion

        #region 私有方法

        /// <summary>
        /// 创建文件夹
        /// </summary>
        private void CreateFolder(string path)
        {
            if (!System.IO.Directory.Exists(path)) System.IO.Directory.CreateDirectory(path);
        }

        #endregion

        #region 字段

        private readonly string _root = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\CADCanvas";

        #endregion
    }
}