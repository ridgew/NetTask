using System;
using System.Collections.Generic;
using NetTask.Core;

namespace NetTask.Interface
{
    /// <summary>
    /// 任务容器接口
    /// 创建者：wangqj@WANGQJ
    /// 创建时间：2010-9-309:39
    /// </summary>
    [APIExport(Category = ExportType.InterfaceAPI)]
    public interface ITaskContainer : INetTask
    {
        /// <summary>
        /// 添加子级任务
        /// </summary>
        void AddSubTask(INetTask task);

        /// <summary>
        /// 添加多个子级任务
        /// </summary>
        void AddSubTaskRange(IEnumerable<NetTask.Interface.INetTask> tasks);

        /// <summary>
        /// 直接获取所有需要执行的任务
        /// </summary>
        INetTask[] GetAllNetTasks();
    }
}
