using NetTask.Core;

namespace NetTask.Interface
{
    /// <summary>
    /// 包含条件分支的任务
    /// </summary>
    [APIExport(Category = ExportType.InterfaceAPI)]
    public interface IBranchTask
    {
        /// <summary>
        /// 在当前作用中是否继续执行
        /// </summary>
        bool CanRun(NetTaskScope scope);
    }
}
