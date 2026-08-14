using System.Windows;

namespace XLogic.Wpf.Ex
{
    public static class RectExtension
    {
        public static Rect Extend(this Rect rect, double value)
        {
            return new Rect(rect.X - value, rect.Y - value, rect.Width + 2 * value, rect.Height + 2 * value);
        }
    }
}