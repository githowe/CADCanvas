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


        private List<VisualLine> _listList = new List<VisualLine>();
    }
}