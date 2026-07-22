using CADCanvas.SubSystem.EditerSystem.Component;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using XLogic.Base.UI;
using XLogic.Wpf.UI;

namespace CADCanvas.SubSystem.EditerSystem
{
    public partial class Editer : UserControl, IPanel
    {
        #region 构造方法

        public Editer()
        {
            InitializeComponent();
            Loaded += Editer_Loaded;
        }

        #endregion

        #region 面板接口

        public void AddFocus()
        {

        }

        public void RemoveFocus()
        {

        }

        public void HandleKeyDown(KeyEventArgs e)
        {

        }

        public void HandleKeyUp(KeyEventArgs e)
        {

        }

        #endregion

        #region 属性、事件

        /// <summary>文档名称</summary>
        public string DocumentName { get; set; } = "未命名文档";

        /// <summary>文档路径</summary>
        public string DocumentPath { get; set; } = "";

        /// <summary>已保存</summary>
        public bool Saved { get; set; } = true;

        /// <summary>已初始化</summary>
        public Action? Inited { get; set; } = null;

        #endregion

        #region 公开方法

        public void Init()
        {
            _editerComponent = _componentBox.AddComponent<EditerComponent>(this, "编辑器组件");
            _toolBarComponent = _componentBox.AddComponent<ToolBarComponent>(this, "工具栏组件");
            _layerComponent = _componentBox.AddComponent<LayerComponent>(this, "图层组件");
            _toolComponent = _componentBox.AddComponent<ToolComponent>(this, "工具组件");
            _interactionComponent = _componentBox.AddComponent<InteractionComponent>(this, "交互组件");
            _drawingComponent = _componentBox.AddComponent<DrawingComponent>(this, "绘制组件");
            _componentBox.Init();
        }

        public void LoadDocument()
        {
            _editerComponent.LoadDocument();
        }

        #endregion

        #region 控件事件

        private void Editer_Loaded(object sender, RoutedEventArgs e)
        {
            if (_inited) return;
            Init();
            _inited = true;
            Inited?.Invoke();
        }

        #endregion

        #region 组件

        /// <summary>组件箱</summary>
        private ComponentBox<Editer> _componentBox = new ComponentBox<Editer>();

        private EditerComponent _editerComponent;

        private ToolBarComponent _toolBarComponent;
        private LayerComponent _layerComponent;
        private ToolComponent _toolComponent;
        private InteractionComponent _interactionComponent;
        private DrawingComponent _drawingComponent;

        #endregion

        #region 字段

        /// <summary>已初始化</summary>
        private bool _inited = false;

        #endregion
    }
}