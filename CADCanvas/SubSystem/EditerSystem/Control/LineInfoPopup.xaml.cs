using System.Windows;
using System.Windows.Controls;

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
        public LineInfoPopup() => InitializeComponent();

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

        public void UpdateLineInfo(double lineLength, Point lineCenter, double lineAngle, Point arcCenter)
        {
            // 更新长度输入框
            double inputWidth = Grid_Length.ActualWidth;
            double inputHeight = Grid_Length.ActualHeight;
            Canvas.SetLeft(Grid_Length, Math.Round(lineCenter.X - inputWidth / 2));
            Canvas.SetTop(Grid_Length, Math.Round(lineCenter.Y - inputHeight / 2));
            Input_Length.Text = lineLength.ToString(_format);
            // 更新角度输入框
            double arcInputWidth = Grid_Angle.ActualWidth;
            double arcInputHeight = Grid_Angle.ActualHeight;
            Canvas.SetLeft(Grid_Angle, Math.Round(arcCenter.X - arcInputWidth / 2));
            Canvas.SetTop(Grid_Angle, Math.Round(arcCenter.Y - arcInputHeight / 2));
            Input_Angle.Text = lineAngle.ToString(_angleFormat);

            switch (_focus)
            {
                case LineInfoFocus.Length:
                    Input_Length.SelectAll();
                    break;
                case LineInfoFocus.Angle:
                    Input_Angle.SelectAll();
                    break;
            }
        }

        private readonly string _format = "0.#####";
        private readonly string _angleFormat = "0.##";

        private LineInfoFocus _focus = LineInfoFocus.None;
    }
}