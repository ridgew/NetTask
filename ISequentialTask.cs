using NetTask.Core;
using System.ComponentModel;

namespace NetTask.Interface
{
    /// <summary>
    /// 依据顺序执行的任务
    /// </summary>
    [APIExport(Category = ExportType.InterfaceAPI)]
    public interface ISequentialTask
    {
        /// <summary>
        /// 获取或设置实现模型
        /// </summary>
        SequentialMode ImplementMode { get; set; }

        /// <summary>
        /// 直接获取所有需要执行的任务
        /// </summary>
        /// <returns></returns>
        INetTask[] GetAllNetTasks();

        /// <summary>
        /// 获取所有需要执行的任务引用
        /// </summary>
        /// <returns></returns>
        [ContractConfig(IpcContract.Delegate, typeof(StepAction))]
        string[] GetAllNetTaskDelegates();

    }

    /// <summary>
    /// 顺序模型
    /// </summary>
    [APIExport(Category = ExportType.APIDataContract)]
    public enum SequentialMode : byte 
    { 
        /// <summary>
        /// 实例配置
        /// </summary>
        [Description("配置实例")]
        Instance = 0,
        /// <summary>
        /// 静态委托函数
        /// </summary>
        [Description("静态委托函数")]
        StaticDelegate 
    }


}
