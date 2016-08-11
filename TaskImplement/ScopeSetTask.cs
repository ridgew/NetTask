using System;
using NetTask.Core;
using NetTask.Interface;

namespace NetTask.TaskImplement
{
    /// <summary>
    /// 操作域定义设置任务
    /// </summary>
    [APIExport(Category = ExportType.INetTask, IdentityName = "set", Description = "设置作用域中特定键名的值"), Serializable]
    public class ScopeSetTask : XmlDefineTask
    {
        /// <summary>
        /// 在作用域下运行
        /// </summary>
        /// <param name="scope">作用域</param>
        public override void ExecuteInScope(NetTaskScope scope)
        {
            //通过上下文参数配置
            if (Config != null && Config.Items.Length > 0)
            {
                foreach (ConfigItem item in Config.Items)
                {
                    scope[item.Key] = item.GetRuntimeValue();
                }
            }
        }

    }
}
