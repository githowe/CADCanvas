namespace CADCanvas.SubSystem.EditerSystem
{
    /// <summary>
    /// 缩放级别
    /// </summary>
    public class ResizeLevel
    {
        public ResizeLevel()
        {
            _resizeMax = MainCount * SubCount - 1;
            MainLevelMax = MainCount - 1;
        }

        #region 属性

        public int MainLevel { get; set; } = 0;
        public int MainLevelMax { get; set; }
        public int MainCount { get; set; } = 5;
        public int SubLevel { get; set; } = 0;
        public int SubCount { get; set; } = 21;

        #endregion

        #region 运算符重载

        public static ResizeLevel operator +(ResizeLevel obj, int num)
        {
            // 创建新对象并移植全部属性与字段
            ResizeLevel newObj = new ResizeLevel
            {
                MainLevel = obj.MainLevel,
                MainLevelMax = obj.MainLevelMax,
                MainCount = obj.MainCount,
                SubLevel = obj.SubLevel,
                SubCount = obj.SubCount,
                _realResizeValue = obj._realResizeValue,
                _resizeMax = obj._resizeMax
            };

            // 计算“实际缩放值”
            newObj._realResizeValue += num;
            // 防止“实际缩放值”越界
            if (newObj._realResizeValue < 0) newObj._realResizeValue = 0;
            if (newObj._realResizeValue > newObj._resizeMax) newObj._realResizeValue = newObj._resizeMax;
            // 根据“实际缩放值”计算“MainLevel”与“SubLevel”
            int realResizeValue = newObj._realResizeValue;
            newObj.MainLevel = realResizeValue / newObj.SubCount;
            realResizeValue -= newObj.MainLevel * newObj.SubCount;
            newObj.SubLevel = realResizeValue;

            // 返回新对象
            return newObj;
        }

        #endregion

        public override string ToString() => $"{MainLevel}.{SubLevel}";

        #region 字段

        /// <summary>实际缩放值</summary>
        private int _realResizeValue = 0;

        /// <summary>最大缩放值</summary>
        private int _resizeMax;

        #endregion
    }
}