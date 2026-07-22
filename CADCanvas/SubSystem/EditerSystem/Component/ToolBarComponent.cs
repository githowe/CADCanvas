using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using XLogic.Base.UI;

namespace CADCanvas.SubSystem.EditerSystem.Component
{
    /// <summary>
    /// 工具栏组件
    /// </summary>
    public class ToolBarComponent : Component<Editer>
    {
        #region 事件

        public event Action<string>? ToolClick = null;

        public event Action<string>? ToggleCheckedChanged = null;

        #endregion

        #region 生命周期

        protected override void Init()
        {
            AddToolListen();
        }

        protected override void Remove()
        {
            RemoveToolListen();
        }

        #endregion

        #region 控件事件

        private void Tool_Click(object sender, RoutedEventArgs e) => ToolClick?.Invoke(((Button)sender).Name);

        private void Toggle_Checked(object sender, RoutedEventArgs e) => ToggleChanged(sender, true);

        private void Toggle_Unchecked(object sender, RoutedEventArgs e) => ToggleChanged(sender, false);

        #endregion

        #region 私有方法

        /// <summary>
        /// 添加工具监听
        /// </summary>
        private void AddToolListen()
        {
            foreach (var item in Host.Stack_ToolBar.Children)
            {
                // 监听按钮
                if (item is Button button) button.Click += Tool_Click;
                // 监听开关
                else if (item is ToggleButton toggle)
                {
                    toggle.Checked += Toggle_Checked;
                    toggle.Unchecked += Toggle_Unchecked;
                }
            }
        }

        /// <summary>
        /// 移除工具监听
        /// </summary>
        private void RemoveToolListen()
        {
            foreach (var item in Host.Stack_ToolBar.Children)
            {
                // 移除按钮监听
                if (item is Button button) button.Click -= Tool_Click;
                // 移除开关监听
                else if (item is ToggleButton toggle)
                {
                    toggle.Checked -= Toggle_Checked;
                    toggle.Unchecked -= Toggle_Unchecked;
                }
            }
        }

        private void ToggleChanged(object sender, bool value)
        {

        }

        #endregion
    }
}