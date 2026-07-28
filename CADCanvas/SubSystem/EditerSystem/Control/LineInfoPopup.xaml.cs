using CADCanvas.SubSystem.EditerSystem.Control.Layer;
using CADCanvas.SubSystem.EditerSystem.Layer;
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
        public LineInfoPopup()
        {
            InitializeComponent();
        }

        #region 属性

        public GridLayer Grid { get; set; } = null;

        public Point StartPoint { get; set; } = new Point();

        public Point EndPoint { get; set; } = new Point();

        #endregion

        public void Init()
        {
            if (Grid == null) throw new Exception("未设置网格图层");

            LayerBox.Children.Add(_lineInfoLayer);
            _lineInfoLayer.Init();
            _lineInfoLayer.Grid = Grid;
        }

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

        public void UpdateLineInfo()
        {
            // 更新标注线
            _lineInfoLayer.Width = ActualWidth;
            _lineInfoLayer.Height = ActualHeight;
            _lineInfoLayer.StartPoint = StartPoint;
            _lineInfoLayer.EndPoint = EndPoint;
            _lineInfoLayer.Update();
            // 更新长度输入框
            Point lineCenter = _lineInfoLayer.GetLineCenter();
            double inputWidth = Grid_Length.ActualWidth;
            double inputHeight = Grid_Length.ActualHeight;
            Canvas.SetLeft(Grid_Length, Math.Round(lineCenter.X - inputWidth / 2));
            Canvas.SetTop(Grid_Length, Math.Round(lineCenter.Y - inputHeight / 2));
            Input_Length.Text = _lineInfoLayer.GetLineLength().ToString(_format);
            // 更新角度输入框
            Point arcCenter = _lineInfoLayer.GetArcCenter();
            double arcInputWidth = Grid_Angle.ActualWidth;
            double arcInputHeight = Grid_Angle.ActualHeight;
            Canvas.SetLeft(Grid_Angle, Math.Round(arcCenter.X - arcInputWidth / 2));
            Canvas.SetTop(Grid_Angle, Math.Round(arcCenter.Y - arcInputHeight / 2));
            Input_Angle.Text = _lineInfoLayer.GetLineAngle().ToString(_angleFormat);

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

        private readonly LineInfoLayer _lineInfoLayer = new LineInfoLayer();
        private readonly string _format = "0.#####";
        private readonly string _angleFormat = "0.##";

        private LineInfoFocus _focus = LineInfoFocus.None;
    }
}