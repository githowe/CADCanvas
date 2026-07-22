using CADCanvas.SubSystem.EditerSystem.Layer;
using XLogic.Base.UI;

namespace CADCanvas.SubSystem.EditerSystem.Component
{
    /// <summary>
    /// 图层组件
    /// </summary>
    public class LayerComponent : Component<Editer>
    {
        #region 公开方法

        public void UpdateGrid()
        {
            _gridLayer.Width = _host.LayerBox.ActualWidth;
            _gridLayer.Height = _host.LayerBox.ActualHeight;
            _gridLayer.Update();
        }

        #endregion

        #region 生命周期

        protected override void Init()
        {
            _gridLayer = new GridLayer();
            _host.LayerBox.Children.Add(_gridLayer);
            _gridLayer.Init();
        }

        #endregion

        #region 字段

        private GridLayer? _gridLayer;

        #endregion
    }
}