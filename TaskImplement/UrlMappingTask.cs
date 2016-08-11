using System;
using NetTask.Core;
using NetTask.Interface;

namespace NetTask.TaskImplement
{
    /// <summary>
    /// 指定地址的其他任务
    /// </summary>
    [APIExport(Category = ExportType.INetTask, IdentityName = "url", Description="指定文件地址映射的任务"), Serializable]
    public class UrlMappingTask : XmlDefineTask
    {
        /// <summary>
        /// 在作用域下运行
        /// </summary>
        /// <param name="scope">作用域</param>
        public override void ExecuteInScope(NetTaskScope scope)
        {
            string taskUrl = TaskUrl;
            if (scope.ContainsKey("TaskUrl"))
            {
                taskUrl = scope["TaskUrl"].ToString();
            }
            else
            {
                scope["TaskUrl"] = taskUrl;
            }

            if (string.IsNullOrEmpty(taskUrl))
            {
                throw new InvalidContextDataException("没有指定TaskUrl参数或者键名为该值的数据不存在！");
            }
            else
            {
                XmlDefineTask.CreateFrom(taskUrl).RunInScope(scope);
            }
        }

        /// <summary>
        /// 在特定容器下运行
        /// </summary>
        /// <param name="scope">运行上下文</param>
        /// <param name="container">调用任务容器</param>
        [APIExport(Category = ExportType.APIMethod)]
        public static void ExecuteInContainer(NetTaskScope scope, INetTask container)
        {
            new UrlMappingTask().RunInScope(scope);
        }

        
    }
}
