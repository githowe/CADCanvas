using System.Windows.Media.Imaging;
using XLogic.AppFrame;

namespace CADCanvas.SubSystem.ResourceSystem
{
    public class ImageManager : IManager
    {
        #region 单例

        private ImageManager() { }
        public static ImageManager Instance { get; } = new ImageManager();

        #endregion

        #region 光标图片

        public BitmapImage Cursor_Select { get; private set; }

        public BitmapImage Cursor_Move { get; private set; }

        public BitmapImage Cursor_Draw { get; private set; }

        public BitmapImage Cursor_Trim { get; private set; }

        #endregion

        #region 接口实现

        public void Init()
        {
            LoadCursorImage();
        }

        public void Reset() { }

        public void Clear() { }

        #endregion

        #region 私有方法

        /// <summary>
        /// 获取图片
        /// </summary>
        public BitmapImage GetImage(string path)
        {
            // 已加载过此图片，直接返回
            if (_imageResDict.ContainsKey(path))
                return _imageResDict[path].CloneCurrentValue();

            // 创建图片实例
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            // 设置加载图片后释放文件
            image.CacheOption = BitmapCacheOption.OnLoad;
            // 设置图片源
            image.UriSource = new Uri(path);
            image.EndInit();
            // 保存图片引用
            _imageResDict.Add(path, image);

            // 返回图片实例
            return image;
        }

        /// <summary>
        /// 获取资源图片
        /// </summary>
        public BitmapImage? GetAssetsImage(string path)
        {
            if (path == "") return null;
            return GetImage($"pack://application:,,,/Assets/{path}");
        }

        /// <summary>
        /// 加载光标图片
        /// </summary>
        private void LoadCursorImage()
        {
            Cursor_Select = GetAssetsImage("Image/Cursor/Select.png")!;
            Cursor_Select.Freeze();
            Cursor_Move = GetAssetsImage("Image/Cursor/Move.png")!;
            Cursor_Move.Freeze();
            Cursor_Draw = GetAssetsImage("Image/Cursor/Draw.png")!;
            Cursor_Draw.Freeze();
            Cursor_Trim = GetAssetsImage("Image/Cursor/Trim.png")!;
            Cursor_Trim.Freeze();
        }

        #endregion

        private readonly Dictionary<string, BitmapImage> _imageResDict = new Dictionary<string, BitmapImage>();
    }
}