using System;
using NetTask.Interface;

namespace NetTask.Core
{
    /// <summary>
    /// 数据交互标识
    /// </summary>
    [APIExport(Category = ExportType.APIDataContract), Flags]
    public enum ArgumentExchange : int
    {
        /// <summary>
        /// 自动应用并添加
        /// </summary>
        AutoApply = 1,
        /// <summary>
        /// 自动移除添加参数
        /// </summary>
        AutoClear = 2,
        /// <summary>
        /// 双向，进入步骤时应用，退出时清除。
        /// </summary>
        Duplex = AutoApply | AutoClear
    }
}
