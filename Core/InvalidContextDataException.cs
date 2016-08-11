using System;
using NetTask.Interface;

namespace NetTask.Core
{
    /// <summary>
    /// 错误的上下文件数据
    /// </summary>
    [APIExport(Category = ExportType.APIDataContract)]
    public class InvalidContextDataException : ApplicationException
    {
        /// <summary>
        /// 初始化一个 <see cref="InvalidContextDataException"/> class 实例。
        /// </summary>
        /// <param name="message">The message.</param>
        public InvalidContextDataException(string message) : base(message) { }

        /// <summary>
        /// 初始化一个 <see cref="InvalidContextDataException"/> class 实例。
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="innerException">The inner exception.</param>
        public InvalidContextDataException(string message, Exception innerException) : base(message, innerException) { }
    }
}
