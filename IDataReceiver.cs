using NetTask.Core;

namespace NetTask.Interface
{
    /// <summary>
    /// 数据接收器
    /// 创建者：wangqj@WANGQJ
    /// 创建时间：2010-9-2911:46
    /// </summary>
    [APIExport(Category = ExportType.InterfaceAPI)]
    public interface IDataReceiver
    {
        /// <summary>
        /// 是否能够接收当前数据
        /// </summary>
        /// <param name="objData">当前传输的数据</param>
        /// <returns></returns>
        bool Accept(object objData);

        /// <summary>
        /// 数据存储器
        /// </summary>
        ClrDataStorage Storage { get; set; }
    }
}
