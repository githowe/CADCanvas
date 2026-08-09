using System.Windows;
using System.Windows.Controls;
using XLogic.Base.UI;

namespace CADCanvas.SubSystem.EditerSystem.Component
{
    public enum PopupType
    {
        /// <summary>点坐标</summary>
        PointPopup,
        /// <summary>直线绘制信息：长度、角度</summary>
        DrawLineInfo,
        /// <summary>长度</summary>
        LengthPopup,
    }

    /// <summary>
    /// 控件组件
    /// </summary>
    public class ControlComponent : Component<Editer>
    {
        #region 公开方法

        /// <summary>
        /// 加载控件
        /// </summary>
        public UserControl LoadControl(PopupType popupType, Point point)
        {
            _control = _popupControls[popupType]();
            _control.Margin = new Thickness(point.X, point.Y, 0, 0);
            _host.Layer_Control.Children.Add(_control);
            _control.IsHitTestVisible = false;
            return _control;
        }

        /// <summary>
        /// 移动控件
        /// </summary>
        public void MoveControl(Point point)
        {
            if (_control == null) return;
            _control.Margin = new Thickness(point.X, point.Y, 0, 0);
        }

        public void SetControlSize(double width, double height)
        {
            if (_control == null) return;
            _control.Width = width;
            _control.Height = height;
        }

        /// <summary>
        /// 卸载控件
        /// </summary>
        public void UnloadControl(PopupType popupType)
        {
            if (_control == null) return;
            _host.Layer_Control.Children.Remove(_control);
            _control = null;
        }

        #endregion

        #region 生命周期

        protected override void Init()
        {
            // 注册控件
            _popupControls.Add(PopupType.PointPopup, CreatePointPopup);
            _popupControls.Add(PopupType.DrawLineInfo, CreateLineInfoPopup);
            _popupControls.Add(PopupType.LengthPopup, CreateLengthPopup);
        }

        #endregion

        #region 私有方法

        private UserControl CreatePointPopup() => new Control.PointPopup();

        private UserControl CreateLineInfoPopup() => new Control.LineInfoPopup();

        private UserControl CreateLengthPopup() => new Control.LengthPopup();

        #endregion

        private readonly Dictionary<PopupType, Func<UserControl>> _popupControls = new Dictionary<PopupType, Func<UserControl>>();
        private UserControl? _control = null;
    }
}