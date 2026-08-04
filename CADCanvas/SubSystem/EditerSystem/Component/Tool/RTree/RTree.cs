using System.Windows;

namespace CADCanvas.SubSystem.EditerSystem.Component.Tool.RTree
{
    /// <summary>
    /// R树实现，用于二维空间中盒子对象的空间索引和快速查询
    /// </summary>
    public class RTree
    {
        #region 构造方法

        public RTree(int maxEntryCount = 8)
        {
            _maxEntryCount = maxEntryCount;
            _minEntryCount = Math.Max(2, maxEntryCount / 2);
            _root = new Node(true);
        }

        #endregion

        #region 属性

        /// <summary>包围盒层列表</summary>
        public List<BoundsLayer> LayerList { get; set; } = new List<BoundsLayer>();

        #endregion

        #region 公开方法

        /// <summary>
        /// 清空R树
        /// </summary>
        public void Clear() => _root = new Node(true);

        /// <summary>
        /// 重建R树
        /// </summary>
        public void Build(List<IBox> entryList)
        {
            // 清空
            Clear();
            // 重新插入
            foreach (var item in entryList) Insert(item, item.Bounds);
        }

        /// <summary>
        /// 插入一个对象
        /// </summary>
        public void Insert(IBox box, Rect bounds)
        {
            // 创建对象条目
            Entry entry = new Entry(bounds, box);
            // 插入条目，并检查是否有分裂
            Node? splitNode = Insert(_root, entry);
            if (splitNode != null)
            {
                // 创建新的根节点
                Node newRoot = new Node(false);
                // 添加根节点条目
                newRoot.EntryList.Add(new Entry(_root.Bounds, _root));
                // 添加分裂节点条目
                newRoot.EntryList.Add(new Entry(splitNode.Bounds, splitNode));
                // 重新计算根节点的包围盒
                RecalculateBounds(newRoot);
                // 更新根节点
                _root = newRoot;
            }
        }

        /// <summary>
        /// 查询与指定区域相交的对象
        /// </summary>
        public IReadOnlyList<IBox> Find(Rect searchBounds)
        {
            List<IBox> resultList = new List<IBox>();

            if (_root.EntryList.Count == 0) return resultList;

            Find(_root, searchBounds, resultList);
            return resultList;
        }

        /// <summary>
        /// 查询包含指定点的对象
        /// </summary>
        public IReadOnlyList<IBox> Find(Point point)
        {
            List<IBox> resultList = new List<IBox>();

            if (_root.EntryList.Count == 0) return resultList;

            Find(_root, point, resultList);
            return resultList;
        }

        /// <summary>
        /// 更新包围盒层
        /// </summary>
        public void UpdateBoundsLayer()
        {
            LayerList.Clear();

            int height = GetTreeHeight();
            if (height == 0) return;

            for (int counter = 0; counter < height; counter++)
                LayerList.Add(new BoundsLayer());

            List<Node> currentNodeList = new List<Node> { _root };

            for (int level = 0; level < height; level++)
            {
                List<Node> nextNodeList = new List<Node>();
                foreach (Node node in currentNodeList)
                {
                    foreach (Entry entry in node.EntryList)
                    {
                        LayerList[level].BoundsList.Add(entry.Bounds);
                        if (!node.IsLeaf)
                            nextNodeList.Add(entry.ChildNode!);
                    }
                }
                currentNodeList = nextNodeList;
            }

            LayerList.Reverse();
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 插入条目至节点
        /// </summary>
        private Node? Insert(Node node, Entry entry)
        {
            // 如果是叶子节点，直接添加
            if (node.IsLeaf)
            {
                // 先直接添加条目
                node.EntryList.Add(entry);
                ExpandBounds(node, entry.Bounds);
                // 条目数量超过最大值，则分裂节点
                if (node.EntryList.Count > _maxEntryCount) return SplitNode(node);
                // 否则返回空，表示没有分裂
                return null;
            }

            // 非叶子节点，先选择最佳子节点进行插入
            Entry bestEntry = ChooseEntry(node, entry.Bounds);
            Node childNode = bestEntry.ChildNode!;
            Node? splitNode = Insert(childNode, entry);
            // 更新最佳子节点的包围盒
            bestEntry.Bounds = childNode.Bounds;
            // 创建指向分裂节点的条目，并添加到当前节点
            if (splitNode != null)
                node.EntryList.Add(new Entry(splitNode.Bounds, splitNode));
            // 重新计算当前节点的包围盒
            RecalculateBounds(node);
            // 如果当前节点的条目数量超过最大值，则分裂节点
            if (node.EntryList.Count > _maxEntryCount) return SplitNode(node);
            // 否则返回空，表示没有分裂
            return null;
        }

        private void Find(Node node, Rect searchBounds, List<IBox> results)
        {
            foreach (Entry entry in node.EntryList)
            {
                if (!entry.Bounds.IntersectsWith(searchBounds)) continue;

                if (node.IsLeaf)
                    results.Add(entry.Box!);
                else
                    Find(entry.ChildNode!, searchBounds, results);
            }
        }

        private void Find(Node node, Point point, List<IBox> results)
        {
            foreach (Entry entry in node.EntryList)
            {
                if (!entry.Bounds.Contains(point)) continue;

                if (node.IsLeaf)
                    results.Add(entry.Box!);
                else
                    Find(entry.ChildNode!, point, results);
            }
        }

        /// <summary>
        /// 选择最佳条目。选择标准是选择扩展面积最小的条目，如果有多个条目扩展面积相同，则选择当前面积最小的条目
        /// </summary>
        private Entry ChooseEntry(Node node, Rect bounds)
        {
            Entry? bestEntry = null;
            double bestExpansion = double.MaxValue;
            double bestArea = double.MaxValue;

            foreach (Entry entry in node.EntryList)
            {
                double currentArea = GetArea(entry.Bounds);
                Rect merged = Union(entry.Bounds, bounds);
                double expandedArea = GetArea(merged);
                double expansion = expandedArea - currentArea;

                if (expansion < bestExpansion)
                {
                    bestExpansion = expansion;
                    bestArea = currentArea;
                    bestEntry = entry;
                    continue;
                }

                if (Math.Abs(expansion - bestExpansion) < _doubleEpsilon && currentArea < bestArea)
                {
                    bestArea = currentArea;
                    bestEntry = entry;
                }
            }

            return bestEntry!;
        }

        /// <summary>
        /// 分裂节点
        /// </summary>
        /// <param name="node">要分裂的节点</param>
        /// <returns>新的分裂节点</returns>
        private Node SplitNode(Node node)
        {
            // 获取当前节点的所有条目，并清空节点的条目列表
            List<Entry> sourceEntryList = new List<Entry>(node.EntryList);
            node.EntryList.Clear();
            node.Bounds = Rect.Empty;
            // 创建一个同层级的节点作为分裂节点
            Node splitNode = new Node(node.IsLeaf);
            // 选择种子条目
            PickSeed(sourceEntryList, out int seedAIndex, out int seedBIndex);
            // 获取种子条目
            Entry seedA = sourceEntryList[seedAIndex];
            Entry seedB = sourceEntryList[seedBIndex];
            // 根据索引顺序移除种子条目，确保先移除索引较大的条目，以避免索引变化导致的错误
            if (seedAIndex > seedBIndex)
            {
                sourceEntryList.RemoveAt(seedAIndex);
                sourceEntryList.RemoveAt(seedBIndex);
            }
            else
            {
                sourceEntryList.RemoveAt(seedBIndex);
                sourceEntryList.RemoveAt(seedAIndex);
            }
            // 将种子条目添加到相应的节点
            AddEntry(node, seedA);
            AddEntry(splitNode, seedB);
            // 循环分配剩余的条目，直到所有条目都被分配
            while (sourceEntryList.Count > 0)
            {
                // 先确保节点的条目数量不低于最小条目数
                if (node.EntryList.Count + sourceEntryList.Count == _minEntryCount)
                {
                    AddAllEntry(node, sourceEntryList);
                    break;
                }
                if (splitNode.EntryList.Count + sourceEntryList.Count == _minEntryCount)
                {
                    AddAllEntry(splitNode, sourceEntryList);
                    break;
                }
                // 选择下一个要分配的条目
                int nextIndex = PickNext(node, splitNode, sourceEntryList);
                Entry nextEntry = sourceEntryList[nextIndex];
                // 计算将条目添加到两个节点所需的扩展面积
                double expandA = GetRequiredExpansion(node.Bounds, nextEntry.Bounds);
                double expandB = GetRequiredExpansion(splitNode.Bounds, nextEntry.Bounds);
                // 根据扩展面积选择将条目添加到哪个节点
                if (expandA < expandB)
                    AddEntry(node, nextEntry);
                else if (expandB < expandA)
                    AddEntry(splitNode, nextEntry);
                else
                {
                    double areaA = GetArea(node.Bounds);
                    double areaB = GetArea(splitNode.Bounds);

                    if (areaA < areaB)
                        AddEntry(node, nextEntry);
                    else if (areaB < areaA)
                        AddEntry(splitNode, nextEntry);
                    else if (node.EntryList.Count <= splitNode.EntryList.Count)
                        AddEntry(node, nextEntry);
                    else
                        AddEntry(splitNode, nextEntry);
                }
                // 移除已分配的条目
                sourceEntryList.RemoveAt(nextIndex);
            }
            // 重新计算两个节点的包围盒
            RecalculateBounds(node);
            RecalculateBounds(splitNode);
            // 返回新分裂的节点
            return splitNode;
        }

        /// <summary>
        /// 选择种子条目，作为分裂的起点。选择标准是找到两个条目，它们的包围盒合并后产生的浪费面积最大
        /// </summary>
        private void PickSeed(List<Entry> entryList, out int seedAIndex, out int seedBIndex)
        {
            double maxWaste = double.MinValue;
            seedAIndex = 0;
            seedBIndex = 1;

            for (int index1 = 0; index1 < entryList.Count - 1; index1++)
            {
                for (int index2 = index1 + 1; index2 < entryList.Count; index2++)
                {
                    // 合并两个条目的包围盒，并计算浪费面积
                    Rect union = Union(entryList[index1].Bounds, entryList[index2].Bounds);
                    double waste = GetArea(union) - GetArea(entryList[index1].Bounds) - GetArea(entryList[index2].Bounds);
                    // 如果浪费面积大于当前最大浪费面积，则更新种子条目索引
                    if (waste > maxWaste)
                    {
                        maxWaste = waste;
                        seedAIndex = index1;
                        seedBIndex = index2;
                    }
                }
            }
        }

        /// <summary>
        /// 选择下一个要分配的条目。选择标准是找到一个条目，它在两个节点中产生的扩展面积差异最大
        /// </summary>
        private int PickNext(Node nodeA, Node nodeB, List<Entry> entryList)
        {
            int bestIndex = 0;
            double bestDiff = double.MinValue;

            for (int index = 0; index < entryList.Count; index++)
            {
                Entry entry = entryList[index];
                double expandA = GetRequiredExpansion(nodeA.Bounds, entry.Bounds);
                double expandB = GetRequiredExpansion(nodeB.Bounds, entry.Bounds);
                double diff = Math.Abs(expandA - expandB);

                if (diff > bestDiff)
                {
                    bestDiff = diff;
                    bestIndex = index;
                }
            }

            return bestIndex;
        }

        /// <summary>
        /// 添加条目列表至节点
        /// </summary>
        private void AddAllEntry(Node node, List<Entry> entryList)
        {
            foreach (Entry entry in entryList) AddEntry(node, entry);
            entryList.Clear();
        }

        /// <summary>
        /// 添加条目至节点
        /// </summary>
        private void AddEntry(Node node, Entry entry)
        {
            node.EntryList.Add(entry);
            ExpandBounds(node, entry.Bounds);
        }

        /// <summary>
        /// 扩展包围盒
        /// </summary>
        private void ExpandBounds(Node node, Rect bounds)
        {
            // 如果当前包围盒为空，则直接设置为新的包围盒
            if (node.Bounds.IsEmpty)
            {
                node.Bounds = bounds;
                return;
            }
            // 否则，设置为合并后的包围盒
            node.Bounds = Union(node.Bounds, bounds);
        }

        /// <summary>
        /// 重新计算节点的包围盒
        /// </summary>
        private void RecalculateBounds(Node node)
        {
            // 无条目时，包围盒为空
            if (node.EntryList.Count == 0)
            {
                node.Bounds = Rect.Empty;
                return;
            }
            // 获取第一个条目的包围盒作为初始值
            Rect bounds = node.EntryList[0].Bounds;
            // 遍历剩余条目，合并包围盒
            for (int index = 1; index < node.EntryList.Count; index++)
                bounds = Union(bounds, node.EntryList[index].Bounds);
            // 更新节点的包围盒
            node.Bounds = bounds;
        }

        /// <summary>
        /// 获取树高度
        /// </summary>
        private int GetTreeHeight()
        {
            if (_root.EntryList.Count == 0) return 0;

            int height = 1;
            Node currentNode = _root;

            while (!currentNode.IsLeaf)
            {
                height++;
                currentNode = currentNode.EntryList[0].ChildNode!;
            }

            return height;
        }

        /// <summary>
        /// 合并区域
        /// </summary>
        private static Rect Union(Rect a, Rect b)
        {
            if (a.IsEmpty) return b;
            if (b.IsEmpty) return a;

            double left = Math.Min(a.Left, b.Left);
            double top = Math.Min(a.Top, b.Top);
            double right = Math.Max(a.Right, b.Right);
            double bottom = Math.Max(a.Bottom, b.Bottom);

            return new Rect(new Point(left, top), new Point(right, bottom));
        }

        /// <summary>
        /// 计算将当前区域扩展以包含目标区域所需的面积增量
        /// </summary>
        private static double GetRequiredExpansion(Rect current, Rect target)
        {
            if (current.IsEmpty) return GetArea(target);

            Rect merged = Union(current, target);
            return GetArea(merged) - GetArea(current);
        }

        /// <summary>
        /// 计算区域面积
        /// </summary>
        private static double GetArea(Rect rect) => rect.IsEmpty ? 0 : rect.Width * rect.Height;

        #endregion

        #region 字段

        /// <summary>浮点数比较的最小差值，用于避免浮点数精度问题</summary>
        private const double _doubleEpsilon = 1e-9;

        /// <summary>最大条目数</summary>
        private readonly int _maxEntryCount;
        /// <summary>最小条目数</summary>
        private readonly int _minEntryCount;
        /// <summary>根节点</summary>
        private Node _root;

        #endregion
    }
}