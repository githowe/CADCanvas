using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace CADCanvas.SubSystem.EditerSystem.Control
{
    public partial class LengthPopup : UserControl
    {
        public LengthPopup()
        {
            InitializeComponent();
            Input_Length.PreviewTextInput += Input_Length_PreviewTextInput;
            Input_Length.LostFocus += Input_Length_LostFocus;
        }

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

        public void InitFocus()
        {
            Input_Length.Focus();
            Input_Length.SelectAll();
        }

        public void UpdateLength(Point lineCenter)
        {
            double inputWidth = Grid_Length.ActualWidth;
            double inputHeight = Grid_Length.ActualHeight;
            Canvas.SetLeft(Grid_Length, Math.Round(lineCenter.X - inputWidth / 2));
            Canvas.SetTop(Grid_Length, Math.Round(lineCenter.Y - inputHeight / 2));
            Grid_Length.Visibility = Visibility.Visible;

            Input_Length.Focus();
            Input_Length.SelectAll();
        }

        public void Reset()
        {
            _edited_length = false;
            _locked_length = false;
            _length = 0;

            原长度 = 0;

            InitFocus();

            Lock_Length.Visibility = Visibility.Collapsed;
            Grid_Length.Visibility = Visibility.Collapsed;
        }

        private void Input_Length_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            _edited_length = true;
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

        private void UpdateLength(double length)
        {
            if (_locked_length) return;
            if (_edited_length) return;
            Input_Length.Text = length.ToString(_format);
        }

        private readonly string _format = "0.########";
        private bool _edited_length = false;
        private bool _locked_length = false;

        private double _length = 0;
    }
}