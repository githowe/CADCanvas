using System.Windows;
using System.Windows.Media;

namespace XLogic.Wpf.Drawing
{
    /// <summary>
    /// 绘图图层：仅包含单个DrawingVisual
    /// </summary>
    public abstract class DrawingLayer : FrameworkElement
    {
        public DrawingLayer()
        {
            AddVisualChild(_visual);
            AddLogicalChild(_visual);
        }

        #region FrameworkElement 成员

        protected override int VisualChildrenCount => 1;

        protected override Visual GetVisualChild(int index) => _visual;

        #endregion

        #region 公开方法

        public virtual void Init() { }

        public void Update()
        {
            _dc = _visual.RenderOpen();
            OnUpdate();
            _dc.Close();
        }

        public void Clear() => _visual.RenderOpen().Close();

        #endregion

        #region 内部方法

        protected virtual void OnUpdate() { }

        #endregion

        #region 字段

        private readonly DrawingVisual _visual = new DrawingVisual();
        protected DrawingContext? _dc;

        #endregion
    }
}