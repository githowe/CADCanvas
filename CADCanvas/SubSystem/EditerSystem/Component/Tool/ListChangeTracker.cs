namespace CADCanvas.SubSystem.EditerSystem.Component.Tool
{
    /// <summary>
    /// 表示一个用于跟踪列表变化的工具类，能够在更新列表时返回新增和移除的对象
    /// </summary>
    public class ListChangeTracker<T> where T : class
    {
        public Action<List<T>>? OnAdd { get; set; } = null;

        public Action<List<T>>? OnRemove { get; set; } = null;

        /// <summary>
        /// 更新列表
        /// </summary>
        public void UpdateList(List<T> newList)
        {
            // 当前对象集与新对象集
            HashSet<T> currentSet = new HashSet<T>(_currentItems);
            HashSet<T> newSet = new HashSet<T>(newList);
            // 计算新增和移除的对象
            List<T> added = newSet.Except(currentSet).ToList();
            List<T> removed = currentSet.Except(newSet).ToList();
            // 更新当前列表
            _currentItems = newList;
            // 触发事件：先移除后新增
            if (removed.Count > 0) OnRemove?.Invoke(removed);
            if (added.Count > 0) OnAdd?.Invoke(added);
        }

        private List<T> _currentItems = new List<T>();
    }
}