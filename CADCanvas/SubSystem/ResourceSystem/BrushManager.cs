using System.Windows.Media;
using XLogic.AppFrame;

namespace CADCanvas.SubSystem.ResourceSystem
{
    public class BrushManager : IManager
    {
        #region 单例

        private BrushManager() { }
        public static BrushManager Instance { get; } = new BrushManager();

        #endregion

        #region 属性

        public List<Brush> BrushList { get; set; } = new List<Brush>();

        #endregion

        #region 生命周期

        public void Init()
        {
            BrushList.Add(new SolidColorBrush(Color.FromRgb(252, 98, 85)));
            BrushList.Add(new SolidColorBrush(Color.FromRgb(88, 196, 221)));
            BrushList.Add(new SolidColorBrush(Color.FromRgb(255, 134, 47)));
            BrushList.Add(new SolidColorBrush(Color.FromRgb(154, 114, 172)));
            BrushList.Add(new SolidColorBrush(Color.FromRgb(197, 95, 115)));
            BrushList.Add(new SolidColorBrush(Color.FromRgb(131, 193, 103)));
            BrushList.Add(new SolidColorBrush(Color.FromRgb(255, 255, 0)));
            BrushList.Add(new SolidColorBrush(Color.FromRgb(209, 71, 189)));
        }

        public void Reset() { }

        public void Clear() { }

        #endregion

        #region 公开方法

        public Brush GetBrush()
        {
            if (_index >= BrushList.Count) _index = 0;
            Brush result = BrushList[_index];
            _index++;
            return result;
        }

        public Color GetColor()
        {
            if (_index >= BrushList.Count) _index = 0;
            Brush brush = BrushList[_index];
            Color result = ((SolidColorBrush)brush).Color;
            _index++;
            return result;
        }

        public void ResetIndex() => _index = 0;

        #endregion

        #region 字段

        private int _index = 0;

        #endregion
    }
}