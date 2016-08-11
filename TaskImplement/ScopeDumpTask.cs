using System;
using NetTask.Core;
using NetTask.Interface;

namespace NetTask.TaskImplement
{
    /// <summary>
    /// 输出作用域中变量的值以方便调试
    /// </summary>
    [Serializable, APIExport(Category = ExportType.INetTask, IdentityName = "dump", Description = "通过适配器设置调试作用中的值")]
    public class ScopeDumpTask : XmlDefineTask
    {
        /// <summary>
        /// 初始化一个 <see cref="ScopeDumpTask"/> class 实例。
        /// </summary>
        public ScopeDumpTask()
            : base()
        {
            ApplyConfigDict.Add("Adapter", o =>
            {
                if (o.Value.Equals("Console", StringComparison.InvariantCultureIgnoreCase))
                {
                    writerAgent = new DumpWriterDelegate(Console.Out.Write);
                }
                else if (o.Value.Equals("Debugger", StringComparison.InvariantCultureIgnoreCase))
                {
                    writerAgent = new DumpWriterDelegate((s, m) =>
                    {
                        System.Diagnostics.Debug.Write(string.Format(s, m));
                    });
                }
                else
                {
                    //其他符合输出委托的静态方法
                    string delegateCfg = o.RefType ?? o.Value;
                    writerAgent = delegateCfg.CreateFromConfig<DumpWriterDelegate>();
                }
            });
        }

        DumpWriterDelegate writerAgent = null;

        /// <summary>
        /// 在作用域下运行
        /// </summary>
        /// <param name="scope">作用域</param>
        public override void ExecuteInScope(NetTaskScope scope)
        {
            if (writerAgent == null && !string.IsNullOrEmpty(TaskUrl))
            {
                //关键字
                if (TaskUrl.Equals("Console", StringComparison.InvariantCultureIgnoreCase))
                {
                    writerAgent = new DumpWriterDelegate(Console.Out.Write);
                }
            }

            if (writerAgent != null)
            {
                writerAgent("*键值总数：{0}, 调用类型：{1}\r\n", scope.Keys.Count,
                    scope.Path.GetExecuteCursor(scope.Path.Cursor).CallerType);

                foreach (string k in scope.Keys)
                {
                    writerAgent(string.Format("Key:{0}, Type:{1}, Value:{2}\r\n", k, scope[k].GetType(), scope[k]));
                }
            }
        }

        /// <summary>
        /// 创建标准控制台输出任务
        /// </summary>
        public static ScopeDumpTask CreateConsoleDumpTask()
        {
            return new ScopeDumpTask()
            {
                Config = new TaskConfig()
                {
                    Items = new ConfigItem[] {
                        new ConfigItem { Key = "Adapter", Value = "Console" }
                    }
                }
            };
        }

    }

    /// <summary>
    /// 调试输出委托
    /// </summary>
    /// <param name="format">格式化字符串。</param>
    /// <param name="arg">要写入格式化字符串的对象数组。</param>
    public delegate void DumpWriterDelegate(string format, params object[] arg);
}
