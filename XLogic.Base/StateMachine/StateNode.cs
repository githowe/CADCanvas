namespace XLogic.Base.StateMachine
{
    public class NextState
    {
        public string State { get; set; } = "";

        public NextState() { }

        public NextState(string state)
        {
            State = state;
        }
    }

    /// <summary>
    /// 状态节点
    /// </summary>
    public class StateNode
    {
        /// <summary>状态</summary>
        public string State { get; set; } = "";

        /// <summary>进入状态节点</summary>
        public Action? OnEnter { get; set; } = null;

        /// <summary>处理事件并返回下一个状态。参数：事件类型；事件数据</summary>
        public Func<InputEvent, string, NextState>? HandleEvent { get; set; } = null;

        /// <summary>退出状态节点</summary>
        public Action? OnExit { get; set; } = null;

        /// <summary>转换状态失败</summary>
        public Action<string>? TransitionFail { get; set; } = null;
    }
}