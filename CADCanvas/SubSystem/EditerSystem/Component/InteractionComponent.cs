using System.Windows;
using System.Windows.Input;
using XLogic.Base.UI;

namespace CADCanvas.SubSystem.EditerSystem.Component
{
    /// <summary>
    /// 交互组件
    /// </summary>
    public class InteractionComponent : Component<Editer>
    {
        #region 公开方法

        public void HandleKeyDown(KeyEventArgs e)
        {
            // 全选
            if (e.Key == Key.A && Keyboard.Modifiers == ModifierKeys.Control)
            {

            }
            // 删除
            else if (e.Key == Key.Delete)
            {

            }
            else _toolComponent.CurrentTool.OnKeyDown(e.Key);
        }

        #endregion

        #region 生命周期

        protected override void Init()
        {
            _host.Layer_Mouse.MouseDown += Layer_Mouse_MouseDown;
            _host.Layer_Mouse.MouseMove += Layer_Mouse_MouseMove;
            _host.Layer_Mouse.MouseUp += Layer_Mouse_MouseUp;
            _host.Layer_Mouse.MouseWheel += Layer_Mouse_MouseWheel;
            _host.MainGrid.SizeChanged += MainGrid_SizeChanged;
            _toolComponent = GetComponent<ToolComponent>();
            _layerComponent = GetComponent<LayerComponent>();
        }

        protected override void Remove()
        {
            _host.Layer_Mouse.MouseDown -= Layer_Mouse_MouseDown;
            _host.Layer_Mouse.MouseMove -= Layer_Mouse_MouseMove;
            _host.Layer_Mouse.MouseUp -= Layer_Mouse_MouseUp;
            _host.Layer_Mouse.MouseWheel -= Layer_Mouse_MouseWheel;
            _host.Layer_Mouse.SizeChanged -= MainGrid_SizeChanged;
            _toolComponent = null;
            _layerComponent = null;
        }

        #endregion

        #region 控件事件

        private void Layer_Mouse_MouseDown(object sender, MouseButtonEventArgs e)
        {
            _toolComponent.CurrentTool.OnMouseDown(e.ChangedButton);
        }

        private void Layer_Mouse_MouseMove(object sender, MouseEventArgs e)
        {
            _toolComponent.CurrentTool.OnMouseMove();
        }

        private void Layer_Mouse_MouseUp(object sender, MouseButtonEventArgs e)
        {
            _toolComponent.CurrentTool.OnMouseUp(e.ChangedButton);
        }

        private void Layer_Mouse_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            _toolComponent.CurrentTool.OnMouseWheel(e);
        }

        private void MainGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _layerComponent.UpdateAll();
        }

        #endregion

        #region 字段

        private ToolComponent? _toolComponent;
        private LayerComponent? _layerComponent;

        #endregion
    }
}