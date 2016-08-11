
namespace NetTask.Core
{
    /// <summary>
    /// 运行步骤状态
    /// </summary>
    public enum StepStatus : byte
    {
        /// <summary>
        /// 未知(未设置)
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// 已禁止运行
        /// </summary>
        Disabled = 1,

        /// <summary>
        /// 正在运行
        /// </summary>
        Running = 2,

        /// <summary>
        /// 正在等待
        /// </summary>
        Waitting = 3,

        /// <summary>
        /// 已暂停
        /// </summary>
        Paused = 4,

        /// <summary>
        /// 出现异常停止
        /// </summary>
        Exception = 5,

        /// <summary>
        /// 运行完成
        /// </summary>
        Finished = 6
    }
}
