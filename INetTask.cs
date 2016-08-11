using NetTask.Core;

namespace NetTask.Interface
{
    /// <summary>
    /// 任务接口
    /// </summary>
    [APIExport(Category = ExportType.InterfaceAPI)]
    public interface INetTask
    {
        ///// <summary>
        ///// 获取当前运行任务级别
        ///// </summary>
        //TaskLevel Level { get; set; }

        /// <summary>
        /// 是否禁止运行（默认否)
        /// </summary>
        bool Disabled { get; }

        /// <summary>
        /// 获取或设置任务配置
        /// </summary>
        TaskConfig Config { get; set; }

        /// <summary>
        /// 运行条件配置
        /// </summary>
        ConditionalConfig Conditional { get; set; }

        /// <summary>
        /// 获取或设置任务区别资源地址
        /// </summary>
        string TaskUrl { get; set; }

        ///// <summary>
        ///// 向上构建，直到任务级别可直接运行
        ///// </summary>
        ///// <param name="currentScope">当前会话作用域</param>
        ///// <returns></returns>
        //INetTaskEngine BuildUp(NetTaskScope currentScope);

        ///// <summary>
        ///// 在当前作用域下是否可运行
        ///// </summary>
        //bool CanRun(NetTaskScope scope);

        /// <summary>
        /// 进入作用域
        /// </summary>
        void EnterScope(NetTaskScope scope);

        /// <summary>
        /// 在作用域下运行
        /// </summary>
        void ExecuteInScope(NetTaskScope scope);

        /// <summary>
        /// 退出作用域
        /// </summary>
        void ExitScope(NetTaskScope scope);

    }
}
