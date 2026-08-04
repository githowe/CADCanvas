using System.Windows;
using System.Windows.Media.Imaging;
using XLogic.Wpf.Drawing;

namespace CADCanvas.SubSystem.EditerSystem.Layer
{
    public class CursorLayer : DrawingLayer
    {
        /// <summary>
        /// 初始化光标
        /// </summary>
        public void InitCursor(BitmapImage cursorImage)
        {
            _cursorImage = cursorImage;
        }

        /// <summary>
        /// 显示光标
        /// </summary>
        public void ShowCursor()
        {
            _isVisible = true;
            Update();
        }

        /// <summary>
        /// 隐藏光标
        /// </summary>
        public void HideCursor()
        {
            _isVisible = false;
            Update();
        }

        /// <summary>
        /// 切换光标
        /// </summary>
        public void SwitchCursor(BitmapImage cursorImage)
        {
            _cursorImage = cursorImage;
            Update();
        }

        /// <summary>
        /// 移动光标
        /// </summary>
        public void MoveCursor(Point point)
        {
            _cursorPoint = point;
            Update();
        }

        protected override void OnUpdate()
        {
            if (_cursorImage == null) return;
            if (!_isVisible) return;

            double x = _cursorPoint.X - _cursorImage.PixelWidth / 2;
            double y = _cursorPoint.Y - _cursorImage.PixelHeight / 2;
            _dc.DrawImage(_cursorImage, new Rect(x, y, _cursorImage.PixelWidth, _cursorImage.PixelHeight));
        }

        private BitmapImage? _cursorImage = null;
        private Point _cursorPoint = new Point();
        private bool _isVisible = false;
    }
}