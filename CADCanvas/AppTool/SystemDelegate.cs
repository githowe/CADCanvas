using CADCanvas.SubSystem.CacheSystem;
using CADCanvas.SubSystem.OptionSystem;
using XLogic.AppFrame;

namespace CADCanvas.AppTool
{
    /// <summary>
    /// 管理器代理
    /// </summary>
    public class ManagerDelegate
    {
        public List<IManager> ManagerList { get; private set; } = new List<IManager>();

        public virtual void Init()
        {
            foreach (var manager in ManagerList) manager.Init();
            InitFinish();
        }

        public virtual void Clear()
        {
            foreach (var manager in ManagerList) manager.Clear();
        }

        protected virtual void InitFinish() { }
    }

    public class SystemDataDelegate : ManagerDelegate
    {
        private SystemDataDelegate()
        {
            // 基础数据：选项、缓存
            ManagerList.Add(OptionManager.Instance);
            ManagerList.Add(CacheManager.Instance);
        }
        public static SystemDataDelegate Instance { get; } = new SystemDataDelegate();
    }
}