using CADCanvas.SubSystem.EditerSystem.Layer;
using XLogic.Base.UI;

namespace CADCanvas.SubSystem.EditerSystem.Component
{
    /// <summary>
    /// 引导线组件
    /// </summary>
    public class GuideComponent : Component<Editer>
    {
        protected override void Init()
        {

        }

        private PolarTrackingLayer _layer;
    }
}