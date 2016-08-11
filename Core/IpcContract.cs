using System;
using NetTask.Interface;

namespace NetTask.Core
{
    /// <summary>
    /// 进程间通信契约类型
    /// </summary>
    [APIExport(Category = ExportType.APIDataContract)]
    public enum IpcContract
    {
        /// <summary>
        /// 实现特定接口
        /// </summary>
        Interface = 0,
        /// <summary>
        /// 实现特定委托函数
        /// </summary>
        Delegate = 1,
        /// <summary>
        /// 继承特定类型
        /// </summary>
        Inherit = 2,
        /// <summary>
        /// 单一的CLR数据类型
        /// </summary>
        ClrData = 3
    }
}
