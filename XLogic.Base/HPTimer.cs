using System.Runtime.InteropServices;

namespace XLogic.Base
{
    /// <summary>
    /// 计时器分辨率
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct TimerCaps
    {
        /// <summary>支持的最小分辨率。单位：毫秒</summary>
        public int periodMin;
        /// <summary>支持的最大分辨率。单位：毫秒</summary>
        public int periodMax;
    }

    /// <summary>
    /// 高精度定时器
    /// </summary>
    public sealed class HPTimer
    {
        #region 系统接口

        [DllImport("winmm.dll")]
        /// <summary>查询计时器分辨率</summary>
        private static extern int timeGetDevCaps(ref TimerCaps caps, int sizeOfTimerCaps);

        [DllImport("winmm.dll")]
        /// <summary>在系统中创建定时器</summary>
        private static extern int timeSetEvent(int delay, int resolution, TimerCallback callback, int user, int mode);

        [DllImport("winmm.dll")]
        /// <summary>终止定时器</summary>
        private static extern int timeKillEvent(int id);

        #endregion

        #region 属性、事件

        public int Interval
        {
            get => _interval;
            set
            {
                if (value < _caps.periodMin || value > _caps.periodMax) throw new Exception("超出计时范围！");
                _interval = value;
            }
        }

        public event Action? Tick;

        #endregion

        #region 构造、析构方法

        static HPTimer() => _ = timeGetDevCaps(ref _caps, Marshal.SizeOf(_caps));

        public HPTimer()
        {
            _interval = _caps.periodMin;
            _timerCallback = new TimerCallback(TimerEventCallback);
        }

        ~HPTimer() { _ = timeKillEvent(_timerID); }

        #endregion

        #region 公开方法

        /// <summary>
        /// 启动定时器
        /// </summary>
        public void Start()
        {
            if (!_running)
            {
                // 尝试在系统中设置一个定时器，设置成功会返回定时器编号
                _timerID = timeSetEvent(_interval, 0, _timerCallback, 0, 1);
                // 设置定时器失败
                if (_timerID == 0) throw new Exception("设置定时器失败");
                _running = true;
            }
        }

        /// <summary>
        /// 停止定时器
        /// </summary>
        public void Stop()
        {
            if (_running)
            {
                _ = timeKillEvent(_timerID);
                _running = false;
            }
        }

        #endregion

        #region 私有方法

        /// <summary>
        /// 系统定时器回调
        /// </summary>
        private void TimerEventCallback(int id, int msg, int user, int param1, int param2) => Tick?.Invoke();

        #endregion

        #region 委托定义

        /// <summary>系统定时器回调</summary>
        private delegate void TimerCallback(int id, int msg, int user, int param1, int param2);

        #endregion

        #region 字段

        /// <summary>系统定时器分辨率</summary>
        private static TimerCaps _caps;

        /// <summary>定时器间隔</summary>
        private int _interval = 1;
        /// <summary>定时器回调</summary>
        private readonly TimerCallback _timerCallback;
        /// <summary>定时器编号</summary>
        private int _timerID;

        /// <summary>正在运行</summary>
        private bool _running = false;

        #endregion
    }
}