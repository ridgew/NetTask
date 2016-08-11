using NetTask.Core;
using System.ComponentModel;

namespace NetTask.Interface
{
    /// <summary>
    /// 包含条件循环的任务
    /// </summary>
    [APIExport(Category = ExportType.InterfaceAPI)]
    public interface ICircleTask
    {
        /// <summary>
        /// 循环执行模式
        /// </summary>
        CircleMode BeCircleMode { get; }

        /// <summary>
        /// 在当前作用中是否继续执行
        /// </summary>
        bool ContinueRun(NetTaskScope scope);

        /// <summary>
        /// 获取或设置主要执行的委托字符
        /// </summary>
        [ContractConfig(IpcContract.Delegate, typeof(StepAction))]
        string CircleActionDelegate { get; set; }
    }

    /// <summary>
    /// 步进式循环任务
    /// </summary>
    [APIExport(Category = ExportType.InterfaceAPI)]
    public interface IStepIncrementCircleTask : ICircleTask
    {
        /// <summary>
        /// 获取或设置初始化委托字符
        /// </summary>
        [ContractConfig(IpcContract.Delegate, typeof(StepAction))]
        string CircleInitialActionDelegate { get; set; }

        /// <summary>
        /// 获取或设置增量委托字符
        /// </summary>
        [ContractConfig(IpcContract.Delegate, typeof(StepAction))]
        string CircleIncrementActionDelegate { get; set; }
    }

    /// <summary>
    /// 循环执行模式
    /// </summary>
    [APIExport(Category = ExportType.APIDataContract)]
    public enum CircleMode
    {
        /// <summary>
        /// While循环
        /// </summary>
        [Description("While循环")]
        While = 0,

        /// <summary>
        /// 至少执行一次的while循环
        /// </summary>
        [Description("至少执行一次的while循环")]
        DoWhile,

        /// <summary>
        /// 步进循环
        /// </summary>
        [Description("步进循环")]
        StepIncrement,

        /// <summary>
        /// 对配置键值为运行结果的迭代循环
        /// </summary>
        [Description("配置键值迭代循环")]
        Enumerator
    }

}
