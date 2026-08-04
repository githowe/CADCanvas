namespace CADCanvas.SubSystem.EditerSystem.Component.Tool.Snap
{
    /// <summary>
    /// 捕捉类型
    /// </summary>
    public enum SnapType
    {
        None,

        /// <summary>端点</summary>
        Endpoint,
        /// <summary>中点</summary>
        Midpoint,
        /// <summary>圆心</summary>
        Center,
        /// <summary>交点</summary>
        Intersection,
        /// <summary>切点</summary>
        Tangent,
        /// <summary>垂足</summary>
        Perpendicular,
        /// <summary>平行</summary>
        Parallel,
        /// <summary>坐标点</summary>
        Point,
    }
}