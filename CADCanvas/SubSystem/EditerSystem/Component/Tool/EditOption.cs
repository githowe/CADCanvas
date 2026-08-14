namespace CADCanvas.SubSystem.EditerSystem.Component.Tool
{
    /// <summary>
    /// 捕捉选项
    /// </summary>
    public class SnapOption
    {
        /// <summary>捕捉端点</summary>
        public bool Endpoint { get; set; } = true;

        /// <summary>捕捉中点</summary>
        public bool Midpoint { get; set; } = true;

        /// <summary>捕捉圆心</summary>
        public bool Center { get; set; } = true;

        /// <summary>捕捉交点</summary>
        public bool Intersection { get; set; } = true;

        /// <summary>捕捉切点</summary>
        public bool Tangent { get; set; } = true;
    }

    /// <summary>
    /// 极轴追踪选项
    /// </summary>
    public class PolarTrackingOption
    {
        /// <summary>启用极轴追踪</summary>
        public bool Enable { get; set; } = true;

        /// <summary>追踪角度</summary>
        public double AngleIncrement { get; set; } = 15.0;
    }

    /// <summary>
    /// 编辑选项
    /// </summary>
    public class EditOption
    {
        /// <summary>捕捉选项</summary>
        public SnapOption SnapOption { get; set; } = new SnapOption();

        /// <summary>极轴追踪选项</summary>
        public PolarTrackingOption PolarTrackingOption { get; set; } = new PolarTrackingOption();
    }
}