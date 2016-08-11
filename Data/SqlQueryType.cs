using NetTask.Interface;

namespace NetTask.Data
{
    /// <summary>
    /// SQL查询类型
    /// </summary>
    [APIExport(Category = ExportType.APIDataContract)]
    public enum SqlQueryType
    {
        /// <summary>
        /// 添加
        /// </summary>
        Insert,
        /// <summary>
        /// 更新
        /// </summary>
        Update,
        /// <summary>
        /// 删除
        /// </summary>
        Delete,
        /// <summary>
        /// 查询
        /// </summary>
        Select,
        /// <summary>
        /// 存储过程
        /// </summary>
        StoredProcedure,
        /// <summary>
        /// 多种查询，例如：GO分隔的多个语句
        /// </summary>
        MultiQuery
    }
}
