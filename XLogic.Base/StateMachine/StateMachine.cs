namespace XLogic.Base.StateMachine
{
    public class StateMachine
    {
        /// <summary>节点列表</summary>
        public List<StateNode> NodeList { get; set; } = new List<StateNode>();

        /// <summary>当前节点</summary>
        public StateNode? CurrentNode { get; private set; } = null;

        public virtual void Init(string initState = "")
        {
            // 设置初识状态节点
            foreach (var node in NodeList)
            {
                if (node.State == initState)
                {
                    TransitionNode(node);
                    break;
                }
            }
        }

        public void HandleEvent(InputEvent inputEvent, string eventData)
        {
            // 当前节点为空或当前节点没有处理事件的方法时，直接返回
            if (CurrentNode == null || CurrentNode.HandleEvent == null) return;

            // 处理事件，并获取下一个状态
            NextState next = CurrentNode.HandleEvent(inputEvent, eventData);
            // 无需切换状态时，直接返回
            if (next.State == "") return;
            // 设置下一个状态节点
            bool changed = false;
            foreach (var node in NodeList)
            {
                if (node.State == next.State)
                {
                    TransitionNode(node);
                    changed = true;
                    break;
                }
            }
            if (!changed) CurrentNode?.TransitionFail?.Invoke(next.State);
        }

        /// <summary>
        /// 转换节点
        /// </summary>
        private void TransitionNode(StateNode next)
        {
            CurrentNode?.OnExit?.Invoke();
            CurrentNode = next;
            CurrentNode?.OnEnter?.Invoke();
        }
    }
}