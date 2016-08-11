using NetTask.Interface;

namespace NetTask.Data
{
    /// <summary>
    /// SQL数据库类型
    /// </summary>
    [APIExport(Category = ExportType.APIDataContract)]
    public enum SqlDatabaseType : int
    {
        /// <summary>
        /// Access数据库
        /// </summary>
        Access = 10,

        /// <summary>
        /// SQL Server 2000数据库
        /// </summary>
        SQL2000 = 20,

        /// <summary>
        /// SQL Server 2005及以上数据库
        /// </summary>
        SQL2005 = 25,
    }

}
