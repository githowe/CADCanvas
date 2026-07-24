namespace XLogic.Base.StateMachine
{
    /// <summary>
    /// 输入事件
    /// </summary>
    public enum InputEvent
    {
        // 鼠标进入、移动、离开
        Enter,
        Move,
        Leave,
        // 鼠标按下、松开
        LeftDown,
        LeftUp,
        MiddleDown,
        MiddleUp,
        RightDown,
        RightUp,
        // 鼠标单击、双击、长按
        Click,
        DoubleClick,
        LongPress,
        // 鼠标滚轮
        Wheel,
        // 键盘按下、松开
        KeyDown,
        KeyUp,
    }
}