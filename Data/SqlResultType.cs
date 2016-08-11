using NetTask.Interface;

namespace NetTask.Data
{
    /// <summary>
    /// SQL数据结果类型
    /// </summary>
    [APIExport(Category = ExportType.APIDataContract)]
    public enum SqlResultType
    {
        /// <summary>
        /// 数据表
        /// </summary>
        DataTable,
        /// <summary>
        /// 数据只读器
        /// </summary>
        DataReader,
        /// <summary>
        /// 单行当列值
        /// </summary>
        ScalarValue,
        /// <summary>
        /// 当行数据
        /// </summary>
        SingleRow,
        /// <summary>
        /// 没有实际意义的返回值
        /// </summary>
        NoValue
    }
}
