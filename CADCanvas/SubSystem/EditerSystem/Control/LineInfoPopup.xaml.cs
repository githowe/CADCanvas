using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CADCanvas.SubSystem.EditerSystem.Control
{
    public enum LineInfoFocus
    {
        None,
        Length,
        Angle
    }

    public partial class LineInfoPopup : UserControl
    {
        #region 构造方法

        public LineInfoPopup()
        {
            InitializeComponent();
            Input_Length.PreviewTextInput += Input_Length_PreviewTextInput;
            Input_Angle.PreviewTextInput += Input_Angle_PreviewTextInput;
            Input_Length.LostFocus += Input_Length_LostFocus;
            Input_Angle.LostFocus += Input_Angle_LostFocus;
        }

        #endregion

        #region 属性

        public double 原长度
        {
            get => _length;
            set
            {
                _length = value;
                UpdateLength(value);
            }
        }

        public double 锁定长度 => _locked_length ? double.Parse(Input_Length.Text) : _length;

        public double 输入长度
        {
            get
            {
                // 已编辑，尝试解析编辑后的值
                if (_edited_length)
                    if (TryParseLength(Input_Length.Text, out double length)) return length;
                // 未编辑或解析失败，返回原长度
                return _length;
            }
        }

        public double 原角度
        {
            get => _angle;
            set
            {
                _angle = value;
                UpdateAngle(value);
            }
        }

        public double 锁定角度 => _locked_angle ? double.Parse(Input_Angle.Text) : _angle;

        public double 输入角度
        {
            get
            {
                if (_edited_angle)
                    if (TryParseAngle(Input_Angle.Text, out double angle)) return angle;
                return _angle;
            }
        }

        public bool AngleLocked => _locked_angle;

        #endregion

        #region 公开方法

        public void InitFocus()
        {
            Input_Length.Focus();
            Input_Length.SelectAll();
            _focus = LineInfoFocus.Length;
        }

        public void SwitchFocus()
        {
            switch (_focus)
            {
                case LineInfoFocus.Length:
                    Input_Angle.Focus();
                    Input_Angle.SelectAll();
                    _focus = LineInfoFocus.Angle;
                    break;
                case LineInfoFocus.Angle:
                    Input_Length.Focus();
                    Input_Length.SelectAll();
                    _focus = LineInfoFocus.Length;
                    break;
            }
        }

        public void UpdateLineInfo(Point lineCenter, Point arcCenter)
        {
            // 更新长度输入框
            double inputWidth = Grid_Length.ActualWidth;
            double inputHeight = Grid_Length.ActualHeight;
            Canvas.SetLeft(Grid_Length, Math.Round(lineCenter.X - inputWidth / 2));
            Canvas.SetTop(Grid_Length, Math.Round(lineCenter.Y - inputHeight / 2));
            Grid_Length.Visibility = Visibility.Visible;
            // 更新角度输入框
            double arcInputWidth = Grid_Angle.ActualWidth;
            double arcInputHeight = Grid_Angle.ActualHeight;
            Canvas.SetLeft(Grid_Angle, Math.Round(arcCenter.X - arcInputWidth / 2));
            Canvas.SetTop(Grid_Angle, Math.Round(arcCenter.Y - arcInputHeight / 2));
            Grid_Angle.Visibility = Visibility.Visible;

            switch (_focus)
            {
                case LineInfoFocus.Length:
                    Input_Length.Focus();
                    Input_Length.SelectAll();
                    break;
                case LineInfoFocus.Angle:
                    Input_Angle.Focus();
                    Input_Angle.SelectAll();
                    break;
            }
        }

        public void Reset()
        {
            _edited_length = false;
            _edited_angle = false;
            _locked_length = false;
            _locked_angle = false;
            _length = 0;
            _angle = 0;

            原长度 = 0;
            原角度 = 0;

            InitFocus();

            Lock_Length.Visibility = Visibility.Collapsed;
            Lock_Angle.Visibility = Visibility.Collapsed;
            Grid_Length.Visibility = Visibility.Collapsed;
            Grid_Angle.Visibility = Visibility.Collapsed;
        }

        #endregion

        #region 控件事件

        private void Input_Length_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            _edited_length = true;
        }

        private void Input_Angle_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            _edited_angle = true;
        }

        private void Input_Length_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Input_Length.Text == "")
            {
                _locked_length = false;
                _edited_length = false;
                Input_Length.Text = _length.ToString(_format);
            }
            else if (_edited_length)
            {
                if (TryParseLength(Input_Length.Text, out double _)) _locked_length = true;
                else
                {
                    Input_Length.Text = _length.ToString(_format);
                    _edited_length = false;
                }
            }
            Lock_Length.Visibility = _locked_length ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Input_Angle_LostFocus(object sender, RoutedEventArgs e)
        {
            if (Input_Angle.Text == "")
            {
                _locked_angle = false;
                _edited_angle = false;
                Input_Angle.Text = ToHalfPerigon(_angle).ToString(_angleFormat);
            }
            else if (_edited_angle)
            {
                if (TryParseAngle(Input_Angle.Text, out double _)) _locked_angle = true;
                else
                {
                    Input_Angle.Text = ToHalfPerigon(_angle).ToString(_angleFormat);
                    _edited_angle = false;
                }
            }
            Lock_Angle.Visibility = _locked_angle ? Visibility.Visible : Visibility.Collapsed;
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 尝试解析长度
        /// </summary>
        private bool TryParseLength(string text, out double length)
        {
            // 先尝试解析
            if (double.TryParse(text, out length))
            {
                // 解析成功但为负数，返回失败
                if (length < 0) return false;
                return true;
            }
            // 解析失败
            return false;
        }

        /// <summary>
        /// 尝试解析角度
        /// </summary>
        private bool TryParseAngle(string text, out double angle)
        {
            if (double.TryParse(text, out angle))
            {
                if (angle < 0) return false;
                angle %= 360;
                return true;
            }
            return false;
        }

        private void UpdateLength(double length)
        {
            if (_locked_length) return;
            if (_edited_length) return;
            Input_Length.Text = length.ToString(_format);
        }

        private void UpdateAngle(double angle)
        {
            if (_locked_angle) return;
            if (_edited_angle) return;
            Input_Angle.Text = ToHalfPerigon(angle).ToString(_angleFormat);
        }

        /// <summary>
        /// 转换为半周角
        /// </summary>
        private double ToHalfPerigon(double perigon)
        {
            if (perigon <= 180) return perigon;
            return 360 - perigon;
        }

        #endregion

        #region 字段

        private readonly string _format = "0.########";
        private readonly string _angleFormat = "0.######";

        private LineInfoFocus _focus = LineInfoFocus.None;

        private bool _edited_length = false;
        private bool _edited_angle = false;
        private bool _locked_length = false;
        private bool _locked_angle = false;

        private double _length = 0;
        private double _angle = 0;

        #endregion
    }
}