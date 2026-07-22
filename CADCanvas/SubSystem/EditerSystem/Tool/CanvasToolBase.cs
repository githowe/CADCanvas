using CADCanvas.SubSystem.EditerSystem.Component;
using System.Windows.Input;
using XLogic.Wpf.Tool;

namespace CADCanvas.SubSystem.EditerSystem.Tool
{
    public abstract class CanvasToolBase : ToolBase<ToolComponent>
    {
        public CanvasToolBase(ToolComponent host) : base(host) { }

        /// <summary>光标</summary>
        public Cursor Cursor { get; set; } = Cursors.Arrow;
    }
}