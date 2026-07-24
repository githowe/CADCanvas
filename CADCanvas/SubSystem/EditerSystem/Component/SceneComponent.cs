using CADCanvas.SubSystem.DrawingSystem;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XLogic.Base.UI;

namespace CADCanvas.SubSystem.EditerSystem.Component
{
    /// <summary>
    /// 场景组件：管理场景中的对象
    /// </summary>
    public class SceneComponent : Component<Editer>
    {
        public void AddVisual(GeoVisual visual)
        {
            _visualList.Add(visual);
        }

        public void RemoveVisual(GeoVisual visual)
        {
            _visualList.Remove(visual);
        }

        public void ClearVisual()
        {
            _visualList.Clear();
        }

        /// <summary>可视对象列表</summary>
        private readonly List<GeoVisual> _visualList = new List<GeoVisual>();
    }
}