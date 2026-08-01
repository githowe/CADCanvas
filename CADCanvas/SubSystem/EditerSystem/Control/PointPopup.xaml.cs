using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CADCanvas.SubSystem.EditerSystem.Control
{
    public enum FocusInput
    {
        None,
        X,
        Y
    }

    public partial class PointPopup : UserControl
    {
        #region 构造方法

        public PointPopup()
        {
            InitializeComponent();
            Input_x.PreviewTextInput += Input_x_PreviewTextInput;
            Input_y.PreviewTextInput += Input_y_PreviewTextInput;
            Input_x.LostFocus += Input_x_LostFocus;
            Input_y.LostFocus += Input_y_LostFocus;
        }

        #endregion

        #region 属性

        public Point Point
        {
            get
            {
                double x = double.TryParse(Input_x.Text, out double parsedX) ? parsedX : _x;
                double y = double.TryParse(Input_y.Text, out double parsedY) ? parsedY : _y;
                return new Point(x, y);
            }
        }

        #endregion

        #region 公开方法

        public void UpdatePoint(Point point)
        {
            // 记录当前坐标
            _x = point.X;
            _y = point.Y;
            // 更新输入框
            UpdateX(point.X);
            UpdateY(point.Y);
        }

        public void FocusX()
        {
            Input_x.Focus();
            Input_x.SelectAll();
            _focus = FocusInput.X;
        }

        public void SelectAll()
        {
            switch (_focus)
            {
                case FocusInput.X:
                    if (_edited_x) return;
                    Input_x.SelectAll();
                    break;
                case FocusInput.Y:
                    if (_edited_y) return;
                    Input_y.SelectAll();
                    break;
            }
        }

        public void SwitchFocus()
        {
            if (_focus == FocusInput.X)
            {
                Input_y.Focus();
                Input_y.SelectAll();
                _focus = FocusInput.Y;
            }
            else if (_focus == FocusInput.Y)
            {
                Input_x.Focus();
                Input_x.SelectAll();
                _focus = FocusInput.X;
            }
        }

        #endregion

        #region 控件事件

        private void Input_x_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            _edited_x = true;
            _edited_y = false;
        }

        private void Input_y_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            _edited_x = false;
            _edited_y = true;
        }

        private void Input_x_LostFocus(object sender, RoutedEventArgs e)
        {
            // 删除文本后，需要解除锁定并设为未编辑
            if (Input_x.Text == "")
            {
                _locked_x = false;
                _edited_x = false;
                Input_x.Text = _x.ToString(_format);
            }
            // 有文本，且是已编辑状态
            else if (_edited_x)
            {
                // 能解析为数字，则锁定
                if (double.TryParse(Input_x.Text, out double x)) _locked_x = true;
                // 否则，恢复原值，并设为未编辑
                else
                {
                    Input_x.Text = _x.ToString(_format);
                    _edited_x = false;
                }
            }
            // 更新锁图标
            Lock_x.Visibility = _locked_x ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Input_y_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Input_y.Text == "")
            {
                _locked_y = false;
                _edited_y = false;
                Input_y.Text = _y.ToString(_format);
            }
            else if (_edited_y)
            {
                if (double.TryParse(Input_y.Text, out double y)) _locked_y = true;
                else
                {
                    Input_y.Text = _y.ToString(_format);
                    _edited_y = false;
                }
            }
            Lock_y.Visibility = _locked_y ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion

        #region 私有方法

        private void UpdateX(double x)
        {
            if (_locked_x) return;
            if (_edited_x) return;
            Input_x.Text = x.ToString(_format);
        }

        private void UpdateY(double y)
        {
            if (_locked_y) return;
            if (_edited_y) return;
            Input_y.Text = y.ToString(_format);
        }

        #endregion

        #region 字段

        private readonly string _format = "0.####";

        /// <summary>当前焦点</summary>
        private FocusInput _focus = FocusInput.None;
        /// <summary>横坐标已编辑</summary>
        private bool _edited_x = false;
        /// <summary>纵坐标已编辑</summary>
        private bool _edited_y = false;
        /// <summary>横坐标已锁定</summary>
        private bool _locked_x = false;
        /// <summary>纵坐标已锁定</summary>
        private bool _locked_y = false;

        private double _x = 0;
        private double _y = 0;

        #endregion
    }
}