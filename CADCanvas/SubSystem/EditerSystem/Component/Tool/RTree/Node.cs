using System.Windows;

namespace CADCanvas.SubSystem.EditerSystem.Component.Tool.RTree
{
    /// <summary>
    /// 节点，用于存储条目
    /// </summary>
    public class Node
    {
        public Node(bool isLeaf) => IsLeaf = isLeaf;

        public bool IsLeaf { get; }

        public Rect Bounds { get; set; } = Rect.Empty;

        public List<Entry> EntryList { get; } = new List<Entry>();
    }
}