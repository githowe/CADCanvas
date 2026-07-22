using XLogic.Base.UI;

namespace CADCanvas.SubSystem.EditerSystem.Component
{
    /// <summary>
    /// 绘制组件
    /// </summary>
    public class DrawingComponent : Component<Editer>
    {
        #region 生命周期

        protected override void Init()
        {
            GetComponent<ToolBarComponent>().ToolClick += ToolBar_ToolClick;
        }

        protected override void Remove()
        {
            GetComponent<ToolBarComponent>().ToolClick -= ToolBar_ToolClick;
        }

        #endregion

        #region 组件事件

        private void ToolBar_ToolClick(string name)
        {
            switch (name)
            {
                case "Tool_Line":
                    Tool_Line_Click();
                    break;
                case "Tool_Circle":
                    Tool_Circle_Click();
                    break;
            }
        }

        #endregion

        #region 私有方法

        private void Tool_Line_Click()
        {
            GetComponent<ToolComponent>().SwitchTool(GetComponent<ToolComponent>().DrawLineTool);
        }

        private void Tool_Circle_Click()
        {
        }

        #endregion
    }
}