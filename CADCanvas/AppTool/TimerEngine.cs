using System.Diagnostics;
using XLogic.Base;

namespace CADCanvas.AppTool
{
    /// <summary>
    /// 定时器引擎
    /// </summary>
    public class TimerEngine
    {
        #region 单例

        private TimerEngine()
        {
            // 启动定时器，直到应用程序退出
            _timer.Tick += Timer_Tick;
            _timer.Start();
            // 启动秒表
            _stopwatch.Start();
        }
        public static TimerEngine Instance { get; } = new TimerEngine();

        #endregion

        #region 属性

        public RunState State { get; private set; } = RunState.Stoped;

        #endregion

        #region 公开方法

        /// <summary>
        /// 启动
        /// </summary>
        public void Start()
        {
            _stopwatch.Start();
            State = RunState.Running;
        }

        /// <summary>
        /// 停止
        /// </summary>
        public void Stop()
        {
            _stopwatch.Stop();
            State = RunState.Stoped;
        }

        #endregion

        #region 私有方法

        private void Timer_Tick()
        {

        }

        #endregion

        #region 字段

        /// <summary>高精度定时器</summary>
        private readonly HPTimer _timer = new HPTimer();
        /// <summary>秒表：用作应用程序的精确时间参考</summary>
        private readonly Stopwatch _stopwatch = new Stopwatch();

        #endregion
    }
}