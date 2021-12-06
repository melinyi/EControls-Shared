namespace EControl.Data
{
    /// <summary>
    /// 指定剩余空间的分布方式。
    /// </summary>
	public enum SpacingMode
    {
        /// <summary>
        /// 间距被禁用，所有项目将尽可能紧密地排列。
        /// </summary>
        None,
        /// <summary>
        /// 剩余空间均匀分布在布局行上的项目之间，以及每行的起点和终点。
        /// </summary>
        Uniform,
        /// <summary>       
        /// 剩余空间均匀分布在布局行上的项目之间，不包括每行的起点和终点。
        /// </summary>
        BetweenItemsOnly,
        /// <summary>
		/// 剩余空间均匀分布在每行的开始和结束之间。
		/// </summary>
        StartAndEndOnly
    }
}
