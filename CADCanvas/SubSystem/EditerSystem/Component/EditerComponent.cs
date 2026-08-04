using XLogic.Base.UI;

namespace CADCanvas.SubSystem.EditerSystem.Component
{
    public class EditerComponent : Component<Editer>
    {
        #region 公开方法

        public void LoadDocument()
        {
            // 更新网格
            GetComponent<LayerComponent>().UpdateGrid();
        }

        #endregion

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

        }

        #endregion
    }
}