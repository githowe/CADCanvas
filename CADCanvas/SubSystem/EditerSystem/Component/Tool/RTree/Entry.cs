using System.Windows;

namespace CADCanvas.SubSystem.EditerSystem.Component.Tool.RTree
{
    /// <summary>
    /// 条目，指向盒子对象或子节点
    /// </summary>
    public class Entry
    {
        public Entry(Rect bounds, IBox box)
        {
            Bounds = bounds;
            Box = box;
        }

        public Entry(Rect bounds, Node child)
        {
            Bounds = bounds;
            ChildNode = child;
        }

        /// <summary>包围盒</summary>
        public Rect Bounds { get; set; }

        /// <summary>条目对象</summary>
        public IBox? Box { get; }

        /// <summary>子节点</summary>
        public Node? ChildNode { get; }
    }
}